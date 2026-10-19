namespace Reclaim.Flow
{
    /// <summary>
    /// Recado entre cenas: a tela de Loading pede o efeito de lente ao entrar no jogo.
    /// </summary>
    public static class LoadingTransitionState
    {
        private static bool lensZoomRequested;
        private static string targetSceneName = string.Empty;

        public static void RequestLensZoomTransition(string sceneName)
        {
            lensZoomRequested = true;
            targetSceneName = sceneName ?? string.Empty;
        }

        public static bool ConsumeLensZoomRequestForScene(string sceneName)
        {
            if (!lensZoomRequested)
            {
                return false;
            }

            bool matchesTarget = string.IsNullOrWhiteSpace(targetSceneName) ||
                                 string.Equals(targetSceneName, sceneName, System.StringComparison.Ordinal);

            lensZoomRequested = false;
            targetSceneName = string.Empty;
            return matchesTarget;
        }
    }
}
