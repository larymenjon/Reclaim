using Reclaim.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reclaim.UI.NewGame
{
    /// <summary>
    /// Fluxo da cena Novo Jogo: guarda as escolhas (líder, mapa, dificuldade, cidade)
    /// em NewGameSettings e navega para o Menu ou para o Loading.
    /// </summary>
    public class NewGameSetupManager : MonoBehaviour
    {
        [Header("Cenas")]
        [SerializeField] private string menuSceneName = "Menu";
        [SerializeField] private string gameSceneName = "Loading";

        [Header("Quantidade de opções")]
        [SerializeField, Min(1)] private int totalLeaders = 7;
        [SerializeField, Min(1)] private int totalMaps = 3;
        [SerializeField, Min(1)] private int totalDifficulties = 3;

        [Header("Padrões de uma partida nova")]
        [SerializeField, Min(0)] private int defaultLeaderIndex = 6;
        [SerializeField, Min(0)] private int defaultDifficultyIndex = 1;
        [SerializeField, Min(0)] private int defaultMapIndex = 1;

        public int SelectedLeaderIndex => Clamp(NewGameSettings.GetLeaderIndex(defaultLeaderIndex), totalLeaders);
        public int SelectedDifficultyIndex => Clamp(NewGameSettings.GetDifficultyIndex(defaultDifficultyIndex), totalDifficulties);
        public int SelectedMapIndex => Clamp(NewGameSettings.GetMapIndex(defaultMapIndex), totalMaps);
        public string SelectedCityName => NewGameSettings.CityName;

        public void SetLeader(int leaderIndex) => NewGameSettings.SetLeaderIndex(Clamp(leaderIndex, totalLeaders));

        public void SetDifficulty(int difficultyIndex) => NewGameSettings.SetDifficultyIndex(Clamp(difficultyIndex, totalDifficulties));

        public void SetMap(int mapIndex) => NewGameSettings.SetMapIndex(Clamp(mapIndex, totalMaps));

        public void SetCityName(string cityName) => NewGameSettings.CityName = cityName;

        public void OnBackPressed()
        {
            SceneManager.LoadScene(menuSceneName);
        }

        public void OnContinuePressed()
        {
            NewGameSettings.Save();
            SceneManager.LoadScene(gameSceneName);
        }

        private static int Clamp(int value, int totalOptions)
        {
            return Mathf.Clamp(value, 0, Mathf.Max(0, totalOptions - 1));
        }
    }
}
