using System.Collections.Generic;
using Reclaim.Construction;
using Reclaim.Settlement;
using UnityEngine;

namespace Reclaim.Work
{
    /// <summary>
    /// Construção onde moradores trabalham. Criado quando a obra termina, a partir dos
    /// ofícios do BuildingData. O jogador designa quantos moradores fazem cada ofício.
    /// </summary>
    public class Workplace : MonoBehaviour
    {
        private const float DoorDistance = 1.6f;

        private readonly Dictionary<JobType, List<Citizen>> _workers = new Dictionary<JobType, List<Citizen>>();
        private WorkSystem _workSystem;
        private SettlementContext _context;
        private Vector3 _entryPoint;

        public Building Building { get; private set; }

        public string Title => Building != null ? Building.Data.DisplayName : "Local de trabalho";
        public bool IsOperational => Building != null && Building.IsConstructionComplete;
        public IReadOnlyList<JobSlot> Slots => Building.Data.JobSlots;

        /// <summary>Ponto de chegada dos trabalhadores, do lado de fora da construção.</summary>
        public Vector3 EntryPoint => _entryPoint;

        public void Initialize(Building building, WorkSystem workSystem, SettlementContext context)
        {
            Building = building;
            _workSystem = workSystem;
            _context = context;
            _entryPoint = ComputeEntryPoint();
        }

        public int CapacityOf(JobType job)
        {
            IReadOnlyList<JobSlot> slots = Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].job == job)
                {
                    return slots[i].capacity;
                }
            }

            return 0;
        }

        public int AssignedTo(JobType job)
        {
            return _workers.TryGetValue(job, out List<Citizen> list) ? list.Count : 0;
        }

        public bool TryAssign(JobType job)
        {
            if (!IsOperational || AssignedTo(job) >= CapacityOf(job))
            {
                return false;
            }

            Citizen worker = _workSystem.Roster.FindFreeAdultNear(_entryPoint);
            if (worker == null)
            {
                return false;
            }

            worker.AssignJob(job, this);
            if (!_workers.TryGetValue(job, out List<Citizen> list))
            {
                list = new List<Citizen>();
                _workers[job] = list;
            }

            list.Add(worker);
            return true;
        }

        public bool Unassign(JobType job)
        {
            if (!_workers.TryGetValue(job, out List<Citizen> list) || list.Count == 0)
            {
                return false;
            }

            Citizen worker = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            if (worker != null)
            {
                worker.ClearJob();
            }

            return true;
        }

        /// <summary>Um trabalhador morreu ou foi removido: libera a vaga.</summary>
        public void NotifyWorkerLost(Citizen worker)
        {
            foreach (List<Citizen> list in _workers.Values)
            {
                list.Remove(worker);
            }
        }

        private void OnDestroy()
        {
            foreach (List<Citizen> list in _workers.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                    {
                        list[i].ClearJob();
                    }
                }
            }
        }

        private Vector3 ComputeEntryPoint()
        {
            float cell = _context.Grid != null ? _context.Grid.CellSize : 1f;
            return BuildingEntry.PointFor(Building, cell, DoorDistance);
        }
    }
}
