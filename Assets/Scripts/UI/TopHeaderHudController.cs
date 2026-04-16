using Reclaim.Survival.Managers;
using TMPro;
using UnityEngine;

namespace Reclaim.UI
{
    public class TopHeaderHudController : MonoBehaviour
    {
        [Header("Header Texts (TMP)")]
        [SerializeField] private TMP_Text cidadeText;
        [SerializeField] private TMP_Text populacaoText;
        [SerializeField] private TMP_Text comidaText;
        [SerializeField] private TMP_Text aguaText;
        [SerializeField] private TMP_Text madeiraText;
        [SerializeField] private TMP_Text combustivelText;
        [SerializeField] private TMP_Text sucataText;
        [SerializeField] private TMP_Text moedasText;
        [SerializeField] private TMP_Text casasText;
        [SerializeField] private TMP_Text contaminacaoText;
        [SerializeField] private TMP_Text reflorestamentoText;
        [SerializeField] private TMP_Text municaoText;
        [SerializeField] private TMP_Text defesaText;

        [Header("Time")]
        [SerializeField] private TimeSystem timeSystem;
        [SerializeField] private int daysPerMonth = 30;

        [Header("Initial Values")]
        [SerializeField] private int startingFamilies = 7;
        [SerializeField] private float startingFoodMonths = 3f;
        [SerializeField] private float startingWaterPercent = 100f;
        [SerializeField] private int startingWood = 120;
        [SerializeField] private float startingFuelMonths = 2f;
        [SerializeField] private int startingScrap = 120;
        [SerializeField] private int startingCoins = 0;
        [SerializeField] private int startingHouses = 0;
        [SerializeField, Range(0f, 100f)] private float startingContaminationPercent = 0f;
        [SerializeField, Range(0f, 100f)] private float startingReforestationPercent = 0f;
        [SerializeField] private float startingAmmoMonths = 0f;
        [SerializeField, Range(0f, 100f)] private float startingDefensePercent = 0f;

        [Header("Auto Start Material Budget")]
        [SerializeField] private bool autoBudgetWoodAndScrap = true;
        [SerializeField] private int woodFactoryWoodCost = 40;
        [SerializeField] private int woodFactoryScrapCost = 20;
        [SerializeField] private int scrapFactoryWoodCost = 20;
        [SerializeField] private int scrapFactoryScrapCost = 40;
        [SerializeField] private int houseWoodCost = 25;
        [SerializeField] private int houseScrapCost = 10;

        [Header("Consumption Per Day")]
        [SerializeField] private float foodConsumptionPerFamilyPerDay = 0.033f; // ~1 mês de comida por família
        [SerializeField] private float waterConsumptionPerFamilyPerDay = 0.5f; // 0.5% de água por família por dia

        [Header("Progression Per Month")]
        [SerializeField] private int familiesPerMonth;
        [SerializeField] private int woodPerMonth;
        [SerializeField] private float fuelMonthsPerMonth;
        [SerializeField] private int scrapPerMonth;
        [SerializeField] private int coinsPerMonth;
        [SerializeField] private int housesPerMonth;
        [SerializeField] private float contaminationPercentPerMonth;
        [SerializeField] private float reforestationPercentPerMonth;
        [SerializeField] private float ammoMonthsPerMonth;
        [SerializeField] private float defensePercentPerMonth;

        public int Families { get; private set; }
        public float FoodMonths { get; private set; }
        public float WaterPercent { get; private set; }
        public int Wood { get; private set; }
        public float FuelMonths { get; private set; }
        public int Scrap { get; private set; }
        public int Coins { get; private set; }
        public int Houses { get; private set; }
        public float ContaminationPercent { get; private set; }
        public float ReforestationPercent { get; private set; }
        public float AmmoMonths { get; private set; }
        public float DefensePercent { get; private set; }

        private void Awake()
        {
            TryResolveTimeSystem();

            ResetToInitialValues();
        }

        private void OnEnable()
        {
            TryResolveTimeSystem();
            if (timeSystem != null)
            {
                timeSystem.OnDayAdvanced += HandleDayAdvanced;
            }
        }

        private void OnDisable()
        {
            if (timeSystem != null)
            {
                timeSystem.OnDayAdvanced -= HandleDayAdvanced;
            }
        }

        public void ResetToInitialValues()
        {
            EnsureStartingBudget();

            Families = Mathf.Max(0, startingFamilies);
            FoodMonths = Mathf.Max(0f, startingFoodMonths);
            WaterPercent = Mathf.Clamp(startingWaterPercent, 0f, 100f);
            Wood = Mathf.Max(0, startingWood);
            FuelMonths = Mathf.Max(0f, startingFuelMonths);
            Scrap = Mathf.Max(0, startingScrap);
            Coins = Mathf.Max(0, startingCoins);
            Houses = Mathf.Max(0, startingHouses);
            ContaminationPercent = Mathf.Clamp(startingContaminationPercent, 0f, 100f);
            ReforestationPercent = Mathf.Clamp(startingReforestationPercent, 0f, 100f);
            AmmoMonths = Mathf.Max(0f, startingAmmoMonths);
            DefensePercent = Mathf.Clamp(startingDefensePercent, 0f, 100f);

            RefreshHud();
        }

        public void AdvanceMonth()
        {
            Families = Mathf.Max(0, Families + familiesPerMonth);
            Wood = Mathf.Max(0, Wood + woodPerMonth);
            FuelMonths = Mathf.Max(0f, FuelMonths + fuelMonthsPerMonth);
            Scrap = Mathf.Max(0, Scrap + scrapPerMonth);
            Coins = Mathf.Max(0, Coins + coinsPerMonth);
            Houses = Mathf.Max(0, Houses + housesPerMonth);
            ContaminationPercent = Mathf.Clamp(ContaminationPercent + contaminationPercentPerMonth, 0f, 100f);
            ReforestationPercent = Mathf.Clamp(ReforestationPercent + reforestationPercentPerMonth, 0f, 100f);
            AmmoMonths = Mathf.Max(0f, AmmoMonths + ammoMonthsPerMonth);
            DefensePercent = Mathf.Clamp(DefensePercent + defensePercentPerMonth, 0f, 100f);

            RefreshHud();
        }

        public void AddWood(int amount)
        {
            Wood = Mathf.Max(0, Wood + amount);
            RefreshHud();
        }

        public void AddScrap(int amount)
        {
            Scrap = Mathf.Max(0, Scrap + amount);
            RefreshHud();
        }

        public void AddHouses(int amount)
        {
            Houses = Mathf.Max(0, Houses + amount);
            RefreshHud();
        }

        public void AddCoins(int amount)
        {
            Coins = Mathf.Max(0, Coins + amount);
            RefreshHud();
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (Coins < amount)
            {
                return false;
            }

            Coins -= amount;
            RefreshHud();
            return true;
        }

        public void AddFamilies(int amount)
        {
            Families = Mathf.Max(0, Families + amount);
            RefreshHud();
        }

        public void AddFoodMonths(float amount)
        {
            FoodMonths = Mathf.Max(0f, FoodMonths + amount);
            RefreshHud();
        }

        public void AddWaterPercent(float amountPercent)
        {
            WaterPercent = Mathf.Clamp(WaterPercent + amountPercent, 0f, 100f);
            RefreshHud();
        }

        public void AddFuelMonths(float amount)
        {
            FuelMonths = Mathf.Max(0f, FuelMonths + amount);
            RefreshHud();
        }

        public void AddAmmoMonths(float amount)
        {
            AmmoMonths = Mathf.Max(0f, AmmoMonths + amount);
            RefreshHud();
        }

        public void AddContamination(float amountPercent)
        {
            ContaminationPercent = Mathf.Clamp(ContaminationPercent + amountPercent, 0f, 100f);
            RefreshHud();
        }

        public void AddReforestation(float amountPercent)
        {
            ReforestationPercent = Mathf.Clamp(ReforestationPercent + amountPercent, 0f, 100f);
            RefreshHud();
        }

        public void AddDefense(float amountPercent)
        {
            DefensePercent = Mathf.Clamp(DefensePercent + amountPercent, 0f, 100f);
            RefreshHud();
        }

        private void TryResolveTimeSystem()
        {
            if (timeSystem == null)
            {
                timeSystem = FindFirstObjectByType<TimeSystem>();
            }
        }

        private void HandleDayAdvanced(int day)
        {
            if (day <= 0 || daysPerMonth <= 0)
            {
                return;
            }

            // Consumo diário de recursos
            ConsumeDailyResources();

            if (day % daysPerMonth == 0)
            {
                AdvanceMonth();
            }
        }

        private void ConsumeDailyResources()
        {
            // Consumo de comida
            float foodConsumed = Families * foodConsumptionPerFamilyPerDay;
            FoodMonths = Mathf.Max(0f, FoodMonths - foodConsumed);

            // Consumo de água
            float waterConsumed = Families * waterConsumptionPerFamilyPerDay;
            WaterPercent = Mathf.Clamp(WaterPercent - waterConsumed, 0f, 100f);

            RefreshHud();
        }

        private void EnsureStartingBudget()
        {
            if (!autoBudgetWoodAndScrap)
            {
                return;
            }

            int minimumWood = Mathf.Max(0, woodFactoryWoodCost + scrapFactoryWoodCost + houseWoodCost);
            int minimumScrap = Mathf.Max(0, woodFactoryScrapCost + scrapFactoryScrapCost + houseScrapCost);

            startingWood = Mathf.Max(startingWood, minimumWood);
            startingScrap = Mathf.Max(startingScrap, minimumScrap);
        }

        private void RefreshHud()
        {
            // Nome da cidade
            if (cidadeText != null) cidadeText.text = GetCityName();

            // População com cálculo de casas (-3)
            int housesNeeded = Mathf.CeilToInt(Families / 3f);
            int housesAvailable = Houses;
            int houseDeficit = Mathf.Max(0, housesNeeded - housesAvailable);
            
            if (populacaoText != null) 
            {
                if (houseDeficit > 0)
                    populacaoText.text = $"{Families} (-{houseDeficit})";
                else
                    populacaoText.text = $"{Families}";
            }

            // Recursos
            if (comidaText != null) comidaText.text = $"{FoodMonths:0.#}";
            if (aguaText != null) aguaText.text = $"{WaterPercent:0.#}%";
            if (madeiraText != null) madeiraText.text = $"{Wood}";
            if (combustivelText != null) combustivelText.text = $"{FuelMonths:0.#}";
            if (sucataText != null) sucataText.text = $"{Scrap}";
            if (moedasText != null) moedasText.text = $"{Coins}";
            if (casasText != null) casasText.text = $"{Houses}";
            if (contaminacaoText != null) contaminacaoText.text = $"{ContaminationPercent:0.#}%";
            if (reflorestamentoText != null) reflorestamentoText.text = $"{ReforestationPercent:0.#}%";
            if (municaoText != null) municaoText.text = $"{AmmoMonths:0.#}";
            if (defesaText != null) defesaText.text = $"{DefensePercent:0.#}%";
        }

        private string GetCityName()
        {
            // Tenta obter o nome da cidade do NewGameSetupManager
            var setupManager = FindObjectOfType<NewGameSetupManager>();
            if (setupManager != null)
            {
                return setupManager.SelectedLeaderName;
            }
            
            // Nome padrão se não encontrar
            return "Nova Cidade";
        }
    }
}
