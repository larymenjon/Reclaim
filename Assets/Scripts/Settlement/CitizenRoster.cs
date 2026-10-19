using System.Collections.Generic;
using Reclaim.Population;
using UnityEngine;

namespace Reclaim.Settlement
{
    /// <summary>Lista dos moradores vivos no mundo, por família. A quantidade acompanha o FamilyManager.</summary>
    public class CitizenRoster
    {
        private readonly List<Citizen> _all = new List<Citizen>();
        private readonly Dictionary<Family, List<Citizen>> _byFamily = new Dictionary<Family, List<Citizen>>();

        public IReadOnlyList<Citizen> All => _all;

        public IReadOnlyList<Citizen> OfFamily(Family family)
        {
            return _byFamily.TryGetValue(family, out List<Citizen> members) ? members : (IReadOnlyList<Citizen>)System.Array.Empty<Citizen>();
        }

        public int CountOf(Family family) => _byFamily.TryGetValue(family, out List<Citizen> members) ? members.Count : 0;

        public int CountAdultsOf(Family family)
        {
            int adults = 0;
            if (_byFamily.TryGetValue(family, out List<Citizen> members))
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].IsAdult)
                    {
                        adults++;
                    }
                }
            }

            return adults;
        }

        public void Add(Family family, Citizen citizen)
        {
            if (!_byFamily.TryGetValue(family, out List<Citizen> members))
            {
                members = new List<Citizen>();
                _byFamily[family] = members;
            }

            members.Add(citizen);
            _all.Add(citizen);
        }

        /// <summary>Remove um morador da família. Crianças saem primeiro.</summary>
        public Citizen RemoveOne(Family family)
        {
            if (!_byFamily.TryGetValue(family, out List<Citizen> members) || members.Count == 0)
            {
                return null;
            }

            int index = members.FindLastIndex(c => !c.IsAdult);
            if (index < 0)
            {
                index = members.Count - 1;
            }

            Citizen removed = members[index];
            members.RemoveAt(index);
            _all.Remove(removed);
            return removed;
        }

        public List<Citizen> RemoveFamily(Family family)
        {
            List<Citizen> removed = new List<Citizen>();
            if (_byFamily.TryGetValue(family, out List<Citizen> members))
            {
                removed.AddRange(members);
                _all.RemoveAll(c => members.Contains(c));
                _byFamily.Remove(family);
            }

            return removed;
        }

        public IEnumerable<Family> Families => _byFamily.Keys;

        /// <summary>Adultos sem ofício.</summary>
        public int CountFreeAdults()
        {
            int free = 0;
            for (int i = 0; i < _all.Count; i++)
            {
                if (_all[i] != null && _all[i].IsAdult && _all[i].Job == Work.JobType.None)
                {
                    free++;
                }
            }

            return free;
        }

        /// <summary>Adulto sem ofício mais perto do ponto, ou nulo se todos já trabalham.</summary>
        public Citizen FindFreeAdultNear(Vector3 point)
        {
            Citizen best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < _all.Count; i++)
            {
                Citizen candidate = _all[i];
                if (candidate == null || !candidate.IsAdult || candidate.Job != Work.JobType.None)
                {
                    continue;
                }

                float sqr = (candidate.transform.position - point).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = candidate;
                }
            }

            return best;
        }
    }
}
