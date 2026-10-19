using Reclaim.Calendar;
using Reclaim.Construction;
using Reclaim.Economy;
using Reclaim.Economy.Forestry;
using Reclaim.Grid;
using Reclaim.Work;
using UnityEngine;

namespace Reclaim.Settlement
{
    /// <summary>Serviços compartilhados pelos moradores: relógio, estoque, grid e regras de horário.</summary>
    public class SettlementContext
    {
        public SettlementSettings Settings;
        public SimulationClock Clock;
        public ResourceStock Stock;
        public TreeForestrySystem Forestry;
        public GridManager Grid;
        public WorkSystem Work;
        public Vector3 CampCenter;

        public float Hour => Clock != null ? Clock.CurrentDate.DayProgress01 * 24f : 12f;

        public bool IsWorkTime => Hour >= Settings.WorkStartHour && Hour < Settings.WorkEndHour;

        /// <summary>Noite: da hora de dormir até uma hora antes do trabalho começar.</summary>
        public bool IsSleepTime => Hour >= Settings.SleepStartHour || Hour < Settings.WorkStartHour - 1f;

        /// <summary>Ponto dentro de uma construção (pedestres contornam). Estradas e terreno livre passam.</summary>
        public bool IsBlocked(Vector3 worldPoint)
        {
            if (Grid == null || !Grid.TryWorldToGrid(worldPoint, out GridCoordinate cell) || !Grid.TryGetCell(cell, out GridCellData data))
            {
                return false;
            }

            return data.OccupancyType == OccupancyType.Building;
        }
    }

    /// <summary>Onde uma família mora: a barraca do começo ou a casa quando fica pronta.</summary>
    public class FamilyHome
    {
        public Vector3 Position;
        public GameObject Tent;
        public Building House;

        public bool IsHouse => House != null;
    }
}
