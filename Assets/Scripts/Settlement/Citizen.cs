using System.Collections;
using Reclaim.Economy;
using Reclaim.Economy.Forestry;
using Reclaim.Population;
using Reclaim.Work;
using UnityEngine;

namespace Reclaim.Settlement
{
    public enum CitizenKind
    {
        Man,
        Woman,
        Child
    }

    /// <summary>
    /// Morador com autonomia. Adultos com ofício saem para o trabalho na hora certa, executam o
    /// ciclo do ofício e voltam; sem ofício e as crianças circulam perto de casa; à noite todos dormem.
    /// Só anda e executa. Quem decide o ofício é o jogador, pelo painel da construção.
    /// </summary>
    public class Citizen : MonoBehaviour
    {
        private const float ArriveDistance = 0.6f;
        private const float SteerLookAhead = 1.8f;
        private static readonly float[] SteerAngles = { 0f, 40f, -40f, 80f, -80f, 120f, -120f };

        private SettlementContext _context;
        private Renderer[] _renderers;
        private float _speed;
        private bool _inside;

        private TreeResourceNode _reservedTree;
        private ScrapPile _reservedPile;

        public Family Family { get; private set; }
        public CitizenKind Kind { get; private set; }
        public FamilyHome Home { get; private set; }
        public Workplace Workplace { get; private set; }
        public JobType Job { get; private set; }

        public bool IsAdult => Kind != CitizenKind.Child;
        public bool HasJob => Job != JobType.None && Workplace != null;

        public void Initialize(SettlementContext context, Family family, CitizenKind kind, FamilyHome home)
        {
            _context = context;
            Family = family;
            Kind = kind;
            Home = home;

            float baseSpeed = kind == CitizenKind.Child ? context.Settings.ChildSpeed : context.Settings.AdultSpeed;
            _speed = baseSpeed * Random.Range(0.9f, 1.1f);
            _renderers = GetComponentsInChildren<Renderer>();
            StartCoroutine(Life());
        }

        public void SetHome(FamilyHome home)
        {
            Home = home;
        }

        public void AssignJob(JobType job, Workplace workplace)
        {
            Job = job;
            Workplace = workplace;
        }

        public void ClearJob()
        {
            Job = JobType.None;
            Workplace = null;
        }

        private void OnDestroy()
        {
            ReleaseReservations();
            if (Workplace != null)
            {
                Workplace.NotifyWorkerLost(this);
            }
        }

        // ---------- Rotina de vida ----------

        private IEnumerator Life()
        {
            yield return new WaitForSeconds(Random.Range(0f, 2f));

            while (true)
            {
                if (_context.IsSleepTime)
                {
                    yield return Sleep();
                }
                else if (IsAdult && HasJob && _context.IsWorkTime && Workplace.IsOperational)
                {
                    yield return WorkShift();
                }
                else
                {
                    yield return Roam();
                }
            }
        }

        private IEnumerator Sleep()
        {
            yield return MoveTo(HomePoint(), ArriveDistance);
            SetInside(Home != null && Home.IsHouse);
            while (_context.IsSleepTime)
            {
                yield return new WaitForSeconds(1f);
            }

            SetInside(false);
        }

        private IEnumerator Roam()
        {
            float radius = Kind == CitizenKind.Child ? 7f : 13f;
            Vector2 offset = Random.insideUnitCircle * radius;
            Vector3 target = HomePoint() + new Vector3(offset.x, 0f, offset.y);
            if (!_context.IsBlocked(target))
            {
                yield return MoveTo(target, ArriveDistance);
            }

            yield return new WaitForSeconds(Random.Range(2f, 7f));
        }

        private IEnumerator WorkShift()
        {
            Vector3 entry = Workplace.EntryPoint;
            yield return MoveTo(entry, ArriveDistance);

            while (_context.IsWorkTime && !_context.IsSleepTime && HasJob && Workplace.IsOperational)
            {
                switch (Job)
                {
                    case JobType.Lumberjack:
                        yield return LumberjackCycle(entry);
                        break;
                    case JobType.Scavenger:
                        yield return ScavengerCycle(entry);
                        break;
                    case JobType.Planter:
                        yield return PlanterCycle(entry);
                        break;
                    default:
                        yield return new WaitForSeconds(2f);
                        break;
                }
            }

            yield return MoveTo(HomePoint(), ArriveDistance);
        }

        // ---------- Ofícios ----------

        private IEnumerator LumberjackCycle(Vector3 entry)
        {
            _reservedTree = _context.Work.ReserveTree(entry, this);
            if (_reservedTree == null)
            {
                yield return new WaitForSeconds(3f);
                yield break;
            }

            TreeResourceNode tree = _reservedTree;
            yield return MoveTo(tree.transform.position, 1.4f);
            if (tree != null)
            {
                yield return new WaitForSeconds(tree.ChopDurationSeconds);
            }

            int wood = tree != null ? tree.CompleteChop() : 0;
            ReleaseReservations();

            yield return MoveTo(entry, ArriveDistance);
            if (wood > 0)
            {
                _context.Work.DepositWood(wood, entry);
            }
        }

        private IEnumerator ScavengerCycle(Vector3 entry)
        {
            _reservedPile = _context.Work.ReservePile(entry, this);
            if (_reservedPile == null)
            {
                yield return new WaitForSeconds(3f);
                yield break;
            }

            ScrapPile pile = _reservedPile;
            yield return MoveTo(pile.transform.position, 1.4f);
            if (pile != null)
            {
                yield return new WaitForSeconds(pile.CollectDurationSeconds);
            }

            int scrap = pile != null ? pile.Collect() : 0;
            ReleaseReservations();

            yield return MoveTo(entry, ArriveDistance);
            if (scrap > 0)
            {
                _context.Work.DepositScrap(scrap);
            }
        }

        private IEnumerator PlanterCycle(Vector3 entry)
        {
            _reservedTree = _context.Work.ReserveStump(entry, this);
            if (_reservedTree == null)
            {
                yield return new WaitForSeconds(3f);
                yield break;
            }

            TreeResourceNode stump = _reservedTree;
            yield return MoveTo(stump.transform.position, 1.2f);
            if (stump != null)
            {
                yield return new WaitForSeconds(stump.PlantDurationSeconds);
                stump.Plant();
            }

            ReleaseReservations();
        }

        private void ReleaseReservations()
        {
            if (_reservedTree != null)
            {
                _reservedTree.Release(this);
                _reservedTree = null;
            }

            if (_reservedPile != null)
            {
                _reservedPile.Release(this);
                _reservedPile = null;
            }
        }

        // ---------- Movimento ----------

        private Vector3 HomePoint()
        {
            return Home != null ? Home.Position : _context.CampCenter;
        }

        /// <summary>Anda até o ponto contornando construções. Desiste se demorar demais (caminho preso).</summary>
        private IEnumerator MoveTo(Vector3 target, float stopDistance)
        {
            SetInside(false);
            float elapsed = 0f;
            float timeout = Mathf.Max(10f, Vector3.Distance(transform.position, target) / _speed * 3f);

            while (elapsed < timeout)
            {
                Vector3 toTarget = target - transform.position;
                toTarget.y = 0f;
                float distance = toTarget.magnitude;
                if (distance <= stopDistance)
                {
                    yield break;
                }

                Vector3 direction = Steer(toTarget / distance);
                float step = Mathf.Min(distance, _speed * UnityEngine.Time.deltaTime);
                Vector3 next = transform.position + direction * step;
                next.y = WorldGround.HeightAt(next.x, next.z, next.y);
                transform.position = next;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * UnityEngine.Time.deltaTime);

                elapsed += UnityEngine.Time.deltaTime;
                yield return null;
            }
        }

        private Vector3 Steer(Vector3 desired)
        {
            for (int i = 0; i < SteerAngles.Length; i++)
            {
                Vector3 candidate = Quaternion.Euler(0f, SteerAngles[i], 0f) * desired;
                if (!_context.IsBlocked(transform.position + candidate * SteerLookAhead))
                {
                    return candidate;
                }
            }

            return desired;
        }

        private void SetInside(bool inside)
        {
            if (_inside == inside)
            {
                return;
            }

            _inside = inside;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _renderers[i].enabled = !inside;
                }
            }
        }
    }
}
