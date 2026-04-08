using Reclaim.Survival.Managers;
using TMPro;
using UnityEngine;

namespace Reclaim.UI
{
    public class TopHeaderHudController : MonoBehaviour
    {
        [Header("Header Texts (TMP)")]
        [SerializeField] private TMP_Text populacaoText;
        [SerializeField] private TMP_Text madeiraText;
        [SerializeField] private TMP_Text combustivelText;
        [SerializeField] private TMP_Text sucataText;
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
        [SerializeField] private int startingWood = 120;
        [SerializeField] private float startingFuelMonths = 2f;
        [SerializeField] private int startingScrap = 120;
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

        [Header("Progression Per Month")]
        [SerializeField] private int familiesPerMonth;
        [SerializeField] private int woodPerMonth;
        [SerializeField] private float fuelMonthsPerMonth;
        [SerializeField] private int scrapPerMonth;
        [SerializeField] private int housesPerMonth;
        [SerializeField] private float contaminationPercentPerMonth;
        [SerializeField] private float reforestationPercentPerMonth;
        [SerializeField] private float ammoMonthsPerMonth;
        [SerializeField] private float defensePercentPerMonth;

        public int Families { get; private set; }
        public int Wood { get; private set; }
        public float FuelMonths { get; private set; }
        public int Scrap { get; private set; }
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
            Wood = Mathf.Max(0, startingWood);
            FuelMonths = Mathf.Max(0f, startingFuelMonths);
            Scrap = Mathf.Max(0, startingScrap);
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

        public void AddFamilies(int amount)
        {
            Families = Mathf.Max(0, Families + amount);
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

            if (day % daysPerMonth == 0)
            {
                AdvanceMonth();
            }
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
            if (populacaoText != null) populacaoText.text = $"{Families}";
            if (madeiraText != null) madeiraText.text = $"{Wood}";
            if (combustivelText != null) combustivelText.text = $"{FuelMonths:0.#}";
            if (sucataText != null) sucataText.text = $"{Scrap}";
            if (casasText != null) casasText.text = $"{Houses}";
            if (contaminacaoText != null) contaminacaoText.text = $"{ContaminationPercent:0.#}%";
            if (reflorestamentoText != null) reflorestamentoText.text = $"{ReforestationPercent:0.#}%";
            if (municaoText != null) municaoText.text = $"{AmmoMonths:0.#}";
            if (defesaText != null) defesaText.text = $"{DefensePercent:0.#}%";
        }
    }
}
