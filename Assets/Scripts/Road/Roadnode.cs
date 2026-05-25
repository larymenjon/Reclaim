using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Road
{
    public enum RoadJunctionType { Isolated, EndCap, Through, TJunction, XJunction }

    /// <summary>Ponto de conexão no grafo de estradas.</summary>
    public class RoadNode
    {
        public int      Id       { get; }
        public Vector3  Position { get; set; }
        public List<int> ConnectedNodeIds { get; } = new();
        public GameObject JunctionObject  { get; set; }

        public RoadNode(int id, Vector3 position) { Id = id; Position = position; }

        public void AddConnection(int otherId)    { if (!ConnectedNodeIds.Contains(otherId)) ConnectedNodeIds.Add(otherId); }
        public void RemoveConnection(int otherId) { ConnectedNodeIds.Remove(otherId); }

        public RoadJunctionType GetJunctionType() => ConnectedNodeIds.Count switch
        {
            0 => RoadJunctionType.Isolated,
            1 => RoadJunctionType.EndCap,
            2 => RoadJunctionType.Through,
            3 => RoadJunctionType.TJunction,
            _ => RoadJunctionType.XJunction
        };
    }

    /// <summary>Segmento de estrada entre dois nós.</summary>
    public class RoadSegmentData
    {
        public int Id      { get; }
        public int NodeAId { get; }
        public int NodeBId { get; }
        public List<GameObject> SegmentObjects { get; } = new();

        public RoadSegmentData(int id, int nodeAId, int nodeBId)
        {
            Id = id; NodeAId = nodeAId; NodeBId = nodeBId;
        }
    }
}
