using UnityEngine;

namespace Reclaim.Flow
{
    /// <summary>
    /// Escolhas do Novo Jogo que precisam atravessar cenas (líder, mapa, dificuldade, cidade, bandeira).
    /// São poucos valores simples, por isso ficam em PlayerPrefs. Todas as chaves vivem aqui.
    /// O progresso da partida, quando existir, terá um sistema de save próprio em arquivo.
    /// </summary>
    public static class NewGameSettings
    {
        public const string DefaultCityName = "Nova Cidade";

        private const string Prefix = "reclaim.newgame.";
        private const string LeaderIndexKey = Prefix + "leader_index";
        private const string MapIndexKey = Prefix + "map_index";
        private const string DifficultyIndexKey = Prefix + "difficulty_index";
        private const string CityNameKey = Prefix + "city_name";
        private const string FlagBackgroundIndexKey = Prefix + "flag_background";
        private const string FlagSymbolColorIndexKey = Prefix + "flag_symbol_color";
        private const string FlagSymbolIndexKey = Prefix + "flag_symbol";

        public static int GetLeaderIndex(int fallback) => PlayerPrefs.GetInt(LeaderIndexKey, fallback);
        public static void SetLeaderIndex(int index) => PlayerPrefs.SetInt(LeaderIndexKey, Mathf.Max(0, index));

        public static int GetMapIndex(int fallback) => PlayerPrefs.GetInt(MapIndexKey, fallback);
        public static void SetMapIndex(int index) => PlayerPrefs.SetInt(MapIndexKey, Mathf.Max(0, index));

        public static int GetDifficultyIndex(int fallback) => PlayerPrefs.GetInt(DifficultyIndexKey, fallback);
        public static void SetDifficultyIndex(int index) => PlayerPrefs.SetInt(DifficultyIndexKey, Mathf.Max(0, index));

        public static string CityName
        {
            get
            {
                string value = PlayerPrefs.GetString(CityNameKey, DefaultCityName);
                return string.IsNullOrWhiteSpace(value) ? DefaultCityName : value;
            }
            set => PlayerPrefs.SetString(CityNameKey, string.IsNullOrWhiteSpace(value) ? DefaultCityName : value.Trim());
        }

        public static int FlagBackgroundIndex
        {
            get => PlayerPrefs.GetInt(FlagBackgroundIndexKey, 0);
            set => PlayerPrefs.SetInt(FlagBackgroundIndexKey, Mathf.Max(0, value));
        }

        public static int FlagSymbolColorIndex
        {
            get => PlayerPrefs.GetInt(FlagSymbolColorIndexKey, 0);
            set => PlayerPrefs.SetInt(FlagSymbolColorIndexKey, Mathf.Max(0, value));
        }

        public static int FlagSymbolIndex
        {
            get => PlayerPrefs.GetInt(FlagSymbolIndexKey, 0);
            set => PlayerPrefs.SetInt(FlagSymbolIndexKey, Mathf.Max(0, value));
        }

        public static void Save() => PlayerPrefs.Save();
    }
}
