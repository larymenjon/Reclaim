using System.Collections.Generic;
using Reclaim.Construction;
using Reclaim.Economy;
using Reclaim.Economy.Forestry;
using Reclaim.Gameplay;
using Reclaim.Grid;
using Reclaim.Input;
using Reclaim.Population;
using Reclaim.Simulation;
using Reclaim.Work;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reclaim.Settlement
{
    /// <summary>
    /// Monta o começo do jogo na cena de jogo: acampamento em círculo no meio do mapa com uma barraca
    /// por família, os moradores (homem, mulher, criança) e os cachorros, árvores e sucata espalhadas,
    /// o horizonte de montanhas e as pendências iniciais. Também liga os moradores ao sistema de trabalho
    /// e move cada família para a casa quando ela fica pronta.
    /// Nasce sozinho em qualquer cena que tenha a ColonySimulation.
    /// </summary>
    public class SettlementBootstrap : MonoBehaviour
    {
        private const float SyncIntervalSeconds = 1f;
        private const int MaxPlacementAttemptsPerItem = 6;

        private SettlementSettings _settings;
        private SettlementContext _context;
        private ColonySimulation _simulation;
        private FamilyManager _families;
        private BuildingSystem _buildings;
        private CitizenRoster _roster;
        private WorkSystem _work;

        private Transform _peopleRoot;
        private Transform _tentsRoot;
        private readonly Dictionary<Family, FamilyHome> _homes = new Dictionary<Family, FamilyHome>();
        private int _tentsPlaced;
        private float _nextSync;

        // ---------- Entrada automática ----------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void HookSceneLoads()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (FindAnyObjectByType<ColonySimulation>() == null || FindAnyObjectByType<SettlementBootstrap>() != null)
            {
                return;
            }

            new GameObject("Assentamento").AddComponent<SettlementBootstrap>();
        }

        // ---------- Montagem ----------

        private void Start()
        {
            _settings = SettlementSettings.LoadOrDefault();
            _simulation = FindAnyObjectByType<ColonySimulation>();
            _families = _simulation.Families;
            _buildings = FindAnyObjectByType<BuildingSystem>();

            _roster = new CitizenRoster();
            _context = new SettlementContext
            {
                Settings = _settings,
                Clock = _simulation.Clock,
                Stock = _simulation.Resources,
                Forestry = FindAnyObjectByType<TreeForestrySystem>(),
                Grid = FindAnyObjectByType<GridManager>()
            };
            _work = new WorkSystem(_context, _roster);
            _context.Work = _work;
            _context.CampCenter = ResolveCampCenter();

            ReplaceLegacySystems();

            _peopleRoot = new GameObject("Moradores").transform;
            _peopleRoot.SetParent(transform, false);
            _tentsRoot = new GameObject("Barracas").transform;
            _tentsRoot.SetParent(transform, false);

            BuildCamp();
            SpawnDogs();
            ScatterTrees();
            ScatterScrap();
            CreateHorizon();
            CreateWorkUi();
            CreateObjectives();

            if (_buildings != null)
            {
                _buildings.OnBuildingCompleted += HandleBuildingCompleted;
            }

            _families.OnFamilyRemoved += HandleFamilyRemoved;
        }

        private void OnDestroy()
        {
            if (_buildings != null)
            {
                _buildings.OnBuildingCompleted -= HandleBuildingCompleted;
            }

            if (_families != null)
            {
                _families.OnFamilyRemoved -= HandleFamilyRemoved;
            }
        }

        private void Update()
        {
            if (_families == null || UnityEngine.Time.time < _nextSync)
            {
                return;
            }

            _nextSync = UnityEngine.Time.time + SyncIntervalSeconds;
            SyncCitizensWithFamilies();
        }

        /// <summary>Corte automático e moradores aleatórios do começo do projeto dão lugar aos moradores com ofício.</summary>
        private void ReplaceLegacySystems()
        {
            if (_context.Forestry != null)
            {
                _context.Forestry.SetAutoHarvest(false);
            }

            foreach (PopulationCharacterSpawner spawner in FindObjectsByType<PopulationCharacterSpawner>(FindObjectsSortMode.None))
            {
                spawner.enabled = false;
            }
        }

        private Vector3 ResolveCampCenter()
        {
            Vector3 center;
            if (_context.Grid != null)
            {
                center = _context.Grid.GridToWorld(new GridCoordinate(_context.Grid.GridWidth / 2, _context.Grid.GridHeight / 2));
            }
            else if (WorldGround.TryGetTerrainRect(out Rect rect))
            {
                center = new Vector3(rect.center.x, 0f, rect.center.y);
            }
            else
            {
                center = Vector3.zero;
            }

            return WorldGround.Snap(center);
        }

        // ---------- Acampamento e moradores ----------

        private void BuildCamp()
        {
            IReadOnlyList<Family> families = _families.Families;
            for (int i = 0; i < families.Count; i++)
            {
                families[i].SetHomeless(true);
                PlaceTentFor(families[i], i, families.Count);
            }

            SyncCitizensWithFamilies();
        }

        private void PlaceTentFor(Family family, int index, int total)
        {
            bool inCircle = index < total && _tentsPlaced < total;
            float angle = (index / Mathf.Max(1f, total)) * Mathf.PI * 2f;
            float radius = inCircle ? _settings.CampRadius : _settings.CampRadius * 1.6f;
            if (!inCircle)
            {
                angle = Random.Range(0f, Mathf.PI * 2f);
            }

            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Vector3 position = WorldGround.Snap(_context.CampCenter + offset);
            Vector3 toCenter = (_context.CampCenter - position).normalized;
            Quaternion rotation = Quaternion.LookRotation(new Vector3(toCenter.x, 0f, toCenter.z));

            GameObject tent = _settings.TentPrefab != null ? Instantiate(_settings.TentPrefab) : PlaceholderFactory.CreateTent();
            tent.name = $"Barraca_{family.FamilyName}";
            tent.transform.SetParent(_tentsRoot, false);
            tent.transform.SetPositionAndRotation(position, rotation);
            _tentsPlaced++;

            _homes[family] = new FamilyHome
            {
                Position = WorldGround.Snap(position + toCenter * 3f),
                Tent = tent
            };
        }

        private void SyncCitizensWithFamilies()
        {
            IReadOnlyList<Family> families = _families.Families;
            for (int i = 0; i < families.Count; i++)
            {
                Family family = families[i];
                if (!_homes.ContainsKey(family))
                {
                    family.SetHomeless(true);
                    PlaceTentFor(family, i, families.Count);
                }

                int desired = family.MembersCount;
                while (_roster.CountOf(family) < desired)
                {
                    SpawnCitizen(family);
                }

                while (_roster.CountOf(family) > desired)
                {
                    DestroyCitizen(_roster.RemoveOne(family));
                }
            }
        }

        private void SpawnCitizen(Family family)
        {
            FamilyHome home = _homes[family];
            int adults = _roster.CountAdultsOf(family);
            CitizenKind kind;
            if (adults == 0)
            {
                kind = family.MembersCount == 1 && Random.value < 0.5f ? CitizenKind.Woman : CitizenKind.Man;
            }
            else if (adults == 1)
            {
                kind = ExistingAdultKind(family) == CitizenKind.Man ? CitizenKind.Woman : CitizenKind.Man;
            }
            else
            {
                kind = CitizenKind.Child;
            }

            Vector2 jitter = Random.insideUnitCircle * 2.5f;
            Vector3 position = WorldGround.Snap(home.Position + new Vector3(jitter.x, 0f, jitter.y));

            GameObject body = InstantiatePerson(kind);
            body.name = $"{kind}_{family.FamilyName}";
            body.transform.SetParent(_peopleRoot, false);
            body.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

            Citizen citizen = body.AddComponent<Citizen>();
            citizen.Initialize(_context, family, kind, home);
            _roster.Add(family, citizen);
        }

        private CitizenKind ExistingAdultKind(Family family)
        {
            IReadOnlyList<Citizen> members = _roster.OfFamily(family);
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].IsAdult)
                {
                    return members[i].Kind;
                }
            }

            return CitizenKind.Man;
        }

        private GameObject InstantiatePerson(CitizenKind kind)
        {
            GameObject prefab = kind == CitizenKind.Man ? _settings.ManPrefab
                : kind == CitizenKind.Woman ? _settings.WomanPrefab
                : _settings.ChildPrefab;
            return prefab != null ? Instantiate(prefab) : PlaceholderFactory.CreatePerson(kind);
        }

        private static void DestroyCitizen(Citizen citizen)
        {
            if (citizen != null)
            {
                Destroy(citizen.gameObject);
            }
        }

        private void HandleFamilyRemoved(Family family)
        {
            foreach (Citizen citizen in _roster.RemoveFamily(family))
            {
                DestroyCitizen(citizen);
            }

            if (_homes.TryGetValue(family, out FamilyHome home))
            {
                if (home.Tent != null)
                {
                    Destroy(home.Tent);
                }

                _homes.Remove(family);
            }
        }

        private void SpawnDogs()
        {
            int dogs = Mathf.CeilToInt(_families.FamilyCount * _settings.DogsPerTwoFamilies / 2f);
            Transform root = new GameObject("Cachorros").transform;
            root.SetParent(transform, false);

            for (int i = 0; i < dogs; i++)
            {
                Vector2 offset = Random.insideUnitCircle * (_settings.CampRadius + 6f);
                Vector3 position = WorldGround.Snap(_context.CampCenter + new Vector3(offset.x, 0f, offset.y));

                GameObject dog = _settings.DogPrefab != null ? Instantiate(_settings.DogPrefab) : PlaceholderFactory.CreateDog();
                dog.name = "Cachorro_" + (i + 1);
                dog.transform.SetParent(root, false);
                dog.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                dog.AddComponent<DogWanderer>().Initialize(_context);
            }
        }

        // ---------- Casas e locais de trabalho ----------

        private void HandleBuildingCompleted(Building building)
        {
            if (building.Data.JobSlots.Count > 0)
            {
                building.gameObject.AddComponent<Workplace>().Initialize(building, _work, _context);
            }

            if (building.Data.CountsAsHouse)
            {
                MoveFamilyIntoHouse(building);
            }
        }

        private void MoveFamilyIntoHouse(Building house)
        {
            foreach (KeyValuePair<Family, FamilyHome> pair in _homes)
            {
                if (pair.Value.IsHouse)
                {
                    continue;
                }

                float cell = _context.Grid != null ? _context.Grid.CellSize : 1f;
                pair.Value.House = house;
                pair.Value.Position = BuildingEntry.PointFor(house, cell);
                if (pair.Value.Tent != null)
                {
                    Destroy(pair.Value.Tent);
                    pair.Value.Tent = null;
                }

                pair.Key.SetHomeless(false);
                return;
            }
        }

        // ---------- Mundo ----------

        private void ScatterTrees()
        {
            int target = _settings.TreeCount;
            if (target <= 0)
            {
                return;
            }

            Transform root = new GameObject("Arvores").transform;
            root.SetParent(transform, false);

            List<Vector3> clusters = new List<Vector3>();
            for (int i = 0; i < _settings.ForestClusters; i++)
            {
                clusters.Add(RandomPointAround(_context.CampCenter, _settings.TreeClearRadiusAroundCamp * 2f, _settings.PopulateRadius * 0.9f));
            }

            HashSet<Vector2Int> taken = new HashSet<Vector2Int>();
            int placed = 0;
            int attempts = 0;
            int maxAttempts = target * MaxPlacementAttemptsPerItem;
            while (placed < target && attempts++ < maxAttempts)
            {
                bool inForest = clusters.Count > 0 && Random.value < 0.75f;
                Vector3 point = inForest
                    ? RandomPointAround(clusters[Random.Range(0, clusters.Count)], 6f, 70f)
                    : RandomPointAround(_context.CampCenter, _settings.TreeClearRadiusAroundCamp, _settings.PopulateRadius);

                if (!IsGoodSpot(point, _settings.TreeClearRadiusAroundCamp) || !ClaimSpacing(taken, point, _settings.MinTreeSpacing))
                {
                    continue;
                }

                CreateTree(root, WorldGround.Snap(point));
                placed++;
            }
        }

        private void CreateTree(Transform parent, Vector3 position)
        {
            GameObject tree = new GameObject("Arvore");
            tree.SetActive(false);
            tree.transform.SetParent(parent, false);
            tree.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            tree.transform.localScale = Vector3.one * Random.Range(0.85f, 1.2f);

            CapsuleCollider capsule = tree.AddComponent<CapsuleCollider>();
            capsule.radius = 0.6f;
            capsule.height = 6f;
            capsule.center = new Vector3(0f, 3f, 0f);

            TreeResourceNode node = tree.AddComponent<TreeResourceNode>();
            node.Configure(_settings.TreeStandingPrefab, _settings.TreeStumpPrefab, _settings.TreeSaplingPrefab,
                _settings.WoodPerTree, _settings.ChopSeconds, _settings.PlantSeconds, _settings.GrowSeconds);
            tree.SetActive(true);
        }

        private void ScatterScrap()
        {
            int target = _settings.ScrapPileCount;
            if (target <= 0)
            {
                return;
            }

            Transform root = new GameObject("Sucata").transform;
            root.SetParent(transform, false);

            HashSet<Vector2Int> taken = new HashSet<Vector2Int>();
            int placed = 0;
            int attempts = 0;
            int maxAttempts = target * MaxPlacementAttemptsPerItem;
            while (placed < target && attempts++ < maxAttempts)
            {
                bool nearCamp = Random.value < 0.4f;
                Vector3 point = nearCamp
                    ? RandomPointAround(_context.CampCenter, 25f, 160f)
                    : RandomPointAround(_context.CampCenter, 25f, _settings.PopulateRadius);

                if (!IsGoodSpot(point, 20f) || !ClaimSpacing(taken, point, 6f))
                {
                    continue;
                }

                GameObject pile = new GameObject("Sucata");
                pile.SetActive(false);
                pile.transform.SetParent(root, false);
                pile.transform.SetPositionAndRotation(WorldGround.Snap(point), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

                bool rubble = Random.value < 0.35f;
                int amount = Mathf.Max(1, Mathf.RoundToInt(_settings.ScrapPerPile * (rubble ? 0.6f : 1f) * Random.Range(0.7f, 1.3f)));
                pile.AddComponent<ScrapPile>().Configure(amount, _settings.CollectSeconds, _settings.ScrapPilePrefab, rubble);
                pile.SetActive(true);
                placed++;
            }
        }

        private static Vector3 RandomPointAround(Vector3 center, float minRadius, float maxRadius)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Mathf.Sqrt(Random.Range(minRadius * minRadius, maxRadius * maxRadius));
            return center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        }

        /// <summary>Dentro do terreno (com folga da borda), fora do acampamento e sem encosta íngreme.</summary>
        private bool IsGoodSpot(Vector3 point, float campClearance)
        {
            if ((point - _context.CampCenter).sqrMagnitude < campClearance * campClearance)
            {
                return false;
            }

            if (WorldGround.TryGetTerrainRect(out Rect rect))
            {
                Rect inner = new Rect(rect.xMin + 20f, rect.yMin + 20f, rect.width - 40f, rect.height - 40f);
                if (!inner.Contains(new Vector2(point.x, point.z)))
                {
                    return false;
                }
            }

            return WorldGround.SlopeAt(point.x, point.z) <= _settings.MaxSlopeDegrees;
        }

        /// <summary>Evita itens colados: só um por célula de grade de tamanho "spacing".</summary>
        private static bool ClaimSpacing(HashSet<Vector2Int> taken, Vector3 point, float spacing)
        {
            Vector2Int cell = new Vector2Int(Mathf.FloorToInt(point.x / spacing), Mathf.FloorToInt(point.z / spacing));
            return taken.Add(cell);
        }

        private void CreateHorizon()
        {
            gameObject.AddComponent<MapHorizon>().Initialize(_settings, _context.Clock);
        }

        // ---------- Interface ----------

        private void CreateWorkUi()
        {
            GameObject host = new GameObject("PainelDeTrabalho");
            host.transform.SetParent(transform, false);
            host.AddComponent<WorkAssignmentPanel>().Initialize(
                _context, _work, FindAnyObjectByType<InputHandler>(), FindAnyObjectByType<ToolModeController>());
        }

        private void CreateObjectives()
        {
            GameObject host = new GameObject("Pendencias");
            host.transform.SetParent(transform, false);
            host.AddComponent<StartingObjectives>().Initialize(_settings, _buildings, _roster, _families.FamilyCount);
        }
    }
}
