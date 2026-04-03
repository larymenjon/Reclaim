using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reclaim.UI
{
    /// <summary>
    /// Controls main menu actions and scene navigation.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string newGameSceneName = "NewGameSetup";
        [SerializeField] private string loadGameSceneName = "Game";

        [Header("Load Game")]
        [SerializeField] private string saveExistsPlayerPrefsKey = "reclaim.save.exists";
        [SerializeField] private bool logWarningWhenNoSave = true;

        [Header("Options")]
        [SerializeField] private GameObject optionsPanel;

        public bool HasSaveData => PlayerPrefs.GetInt(saveExistsPlayerPrefsKey, 0) == 1;

        private void Start()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }
        }

        public void NewGame()
        {
            SceneManager.LoadScene(newGameSceneName);
        }

        public void LoadGame()
        {
            if (!HasSaveData)
            {
                if (logWarningWhenNoSave)
                {
                    Debug.LogWarning("MenuManager: no save data found. Create a save first.");
                }

                return;
            }

            SceneManager.LoadScene(loadGameSceneName);
        }

        public void OpenOptions()
        {
            SetOptionsVisible(true);
        }

        public void CloseOptions()
        {
            SetOptionsVisible(false);
        }

        public void ToggleOptions()
        {
            if (optionsPanel == null)
            {
                return;
            }

            optionsPanel.SetActive(!optionsPanel.activeSelf);
        }

        public void ExitGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void SetOptionsVisible(bool isVisible)
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(isVisible);
            }
        }
    }
}
