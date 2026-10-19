using Reclaim.Economy;
using Reclaim.Economy.Forestry;
using Reclaim.Settlement;
using UnityEngine;

namespace Reclaim.Work
{
    /// <summary>
    /// Entrega aos trabalhadores o que fazer: a árvore mais próxima para cortar, o monte de sucata para
    /// recolher, o toco para replantar. Cada alvo é reservado para um morador só, e o que é produzido
    /// entra no estoque da colônia.
    /// </summary>
    public class WorkSystem
    {
        private readonly SettlementContext _context;

        public CitizenRoster Roster { get; }

        public WorkSystem(SettlementContext context, CitizenRoster roster)
        {
            _context = context;
            Roster = roster;
        }

        public TreeResourceNode ReserveTree(Vector3 origin, Object by)
        {
            TreeResourceNode best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < TreeResourceNode.All.Count; i++)
            {
                TreeResourceNode node = TreeResourceNode.All[i];
                if (node == null || !node.IsHarvestable)
                {
                    continue;
                }

                float sqr = (node.transform.position - origin).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = node;
                }
            }

            return best != null && best.TryReserve(by) ? best : null;
        }

        public TreeResourceNode ReserveStump(Vector3 origin, Object by)
        {
            TreeResourceNode best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < TreeResourceNode.All.Count; i++)
            {
                TreeResourceNode node = TreeResourceNode.All[i];
                if (node == null || !node.NeedsReplant)
                {
                    continue;
                }

                float sqr = (node.transform.position - origin).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = node;
                }
            }

            return best != null && best.TryReserve(by) ? best : null;
        }

        public ScrapPile ReservePile(Vector3 origin, Object by)
        {
            ScrapPile best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < ScrapPile.All.Count; i++)
            {
                ScrapPile pile = ScrapPile.All[i];
                if (pile == null || !pile.IsAvailable)
                {
                    continue;
                }

                float sqr = (pile.transform.position - origin).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = pile;
                }
            }

            return best != null && best.TryReserve(by) ? best : null;
        }

        public void DepositWood(int amount, Vector3 worldPosition)
        {
            if (_context.Forestry != null)
            {
                _context.Forestry.DepositWood(amount, worldPosition);
            }
            else if (_context.Stock != null)
            {
                _context.Stock.Add(ResourceType.Wood, amount);
            }
        }

        public void DepositScrap(int amount)
        {
            if (_context.Stock != null)
            {
                _context.Stock.Add(ResourceType.Scrap, amount);
            }
        }
    }
}
