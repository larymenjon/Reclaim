using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reclaim.UI.Menus
{
    /// <summary>
    /// Ações do menu principal. "Carregar" fica desabilitado enquanto não houver save:
    /// o sistema de save em arquivo ainda não existe no alpha.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string newGameSceneName = "NewGameSetup";
        [SerializeField] private string loadGameSceneName = "Loading";

        [Header("Load Game")]
        [SerializeField] private string saveExistsPlayerPrefsKey = "reclaim.save.exists";
        [SerializeField] private Button loadGameButton;

        [Header("Options")]
        [SerializeField] private GameObject optionsPanel;

        public bool HasSaveData => PlayerPrefs.GetInt(saveExistsPlayerPrefsKey, 0) == 1;

        private void Start()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            if (loadGameButton != null)
            {
                loadGameButton.interactable = HasSaveData;
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
                Debug.LogWarning("[MenuManager] Nenhum save encontrado.", this);
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
