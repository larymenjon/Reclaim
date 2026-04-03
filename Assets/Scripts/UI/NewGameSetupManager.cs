using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reclaim.UI
{
    /// <summary>
    /// Handles navigation actions for the New Game setup scene.
    /// </summary>
    public class NewGameSetupManager : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private string menuSceneName = "Menu";
        [SerializeField] private string gameSceneName = "Game";

        [Header("Options Count")]
        [SerializeField, Min(1)] private int totalLeaders = 7;
        [SerializeField, Min(1)] private int totalMaps = 3;
        [SerializeField, Min(1)] private int totalDifficulties = 3;

        [Header("Defaults")]
        [SerializeField] private string defaultLeaderName = "Aris";
        [SerializeField] private int defaultLeaderIndex = 6;
        [SerializeField] private int defaultDifficultyIndex = 1;
        [SerializeField] private int defaultMapIndex = 1;

        private const string LeaderNameKey = "reclaim.newgame.leader_name";
        private const string LeaderIndexKey = "reclaim.newgame.leader_index";
        private const string DifficultyIndexKey = "reclaim.newgame.difficulty_index";
        private const string MapIndexKey = "reclaim.newgame.map_index";
        private const string DefaultsAppliedKey = "reclaim.newgame.defaults_applied";

        public int SelectedLeaderIndex => ClampSelection(PlayerPrefs.GetInt(LeaderIndexKey, defaultLeaderIndex), totalLeaders);
        public int SelectedDifficultyIndex => ClampSelection(PlayerPrefs.GetInt(DifficultyIndexKey, defaultDifficultyIndex), totalDifficulties);
        public int SelectedMapIndex => ClampSelection(PlayerPrefs.GetInt(MapIndexKey, defaultMapIndex), totalMaps);
        public string SelectedLeaderName => PlayerPrefs.GetString(LeaderNameKey, defaultLeaderName);

        private void Start()
        {
            if (PlayerPrefs.GetInt(DefaultsAppliedKey, 0) != 1)
            {
                SetLeaderName(defaultLeaderName);
                SetLeader(defaultLeaderIndex);
                SetDifficulty(defaultDifficultyIndex);
                SetMap(defaultMapIndex);
                PlayerPrefs.SetInt(DefaultsAppliedKey, 1);
            }
        }

        public void SetLeaderName(string leaderName)
        {
            if (string.IsNullOrWhiteSpace(leaderName))
            {
                return;
            }

            PlayerPrefs.SetString(LeaderNameKey, leaderName.Trim());
        }

        public void SetLeader(int leaderIndex)
        {
            PlayerPrefs.SetInt(LeaderIndexKey, ClampSelection(leaderIndex, totalLeaders));
        }

        public void SetDifficulty(int difficultyIndex)
        {
            PlayerPrefs.SetInt(DifficultyIndexKey, ClampSelection(difficultyIndex, totalDifficulties));
        }

        public void SetMap(int mapIndex)
        {
            PlayerPrefs.SetInt(MapIndexKey, ClampSelection(mapIndex, totalMaps));
        }

        public void OnBackPressed()
        {
            SceneManager.LoadScene(menuSceneName);
        }

        public void OnContinuePressed()
        {
            PlayerPrefs.Save();
            SceneManager.LoadScene(gameSceneName);
        }

        private static int ClampSelection(int value, int totalOptions)
        {
            int maxIndex = Mathf.Max(0, totalOptions - 1);
            return Mathf.Clamp(value, 0, maxIndex);
        }
    }
}
