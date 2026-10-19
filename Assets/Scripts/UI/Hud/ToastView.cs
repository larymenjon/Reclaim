using Reclaim.Calendar;
using TMPro;
using UnityEngine;

namespace Reclaim.UI.Hud
{
    /// <summary>
    /// "Aviso rápido" do Figma: mensagem curta acima da doca por 2,2 s.
    /// Enquanto o jogo está pausado, e não há aviso na tela, mostra "Pausado" em amarelo.
    /// </summary>
    public class ToastView : MonoBehaviour
    {
        [SerializeField] private SimulationClock clock;
        [SerializeField] private GameObject infoRoot;
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private GameObject pausedRoot;
        [SerializeField, Min(0.1f)] private float durationSeconds = 2.2f;

        private float _hideAt = -1f;

        private void Awake()
        {
            infoRoot.SetActive(false);
            pausedRoot.SetActive(false);
        }

        public void Show(string message)
        {
            infoText.text = message;
            infoRoot.SetActive(true);
            _hideAt = Time.unscaledTime + durationSeconds;
            Refresh();
        }

        private void Update()
        {
            if (infoRoot.activeSelf && Time.unscaledTime >= _hideAt)
            {
                infoRoot.SetActive(false);
            }

            Refresh();
        }

        private void Refresh()
        {
            bool showPaused = clock != null && clock.IsPaused && !infoRoot.activeSelf;
            if (pausedRoot.activeSelf != showPaused)
            {
                pausedRoot.SetActive(showPaused);
            }
        }
    }
}
