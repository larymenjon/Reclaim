using System;

namespace Reclaim.Work
{
    /// <summary>
    /// Ofícios que o jogador designa nas construções de trabalho.
    /// Valores explícitos porque o enum é serializado em assets de construção.
    /// </summary>
    public enum JobType
    {
        None = 0,
        Lumberjack = 1,
        Scavenger = 2,
        Planter = 3
    }

    /// <summary>Quantos trabalhadores de um ofício cabem em uma construção.</summary>
    [Serializable]
    public struct JobSlot
    {
        public JobType job;
        public int capacity;
    }

    public static class JobTypeNames
    {
        public static string Display(JobType job)
        {
            switch (job)
            {
                case JobType.Lumberjack: return "Lenhador";
                case JobType.Scavenger: return "Catador de sucata";
                case JobType.Planter: return "Plantador";
                default: return "Sem ofício";
            }
        }
    }
}
