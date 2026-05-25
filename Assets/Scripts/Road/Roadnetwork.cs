using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Manages the road graph and instantiates segment and junction prefabs.
    /// </summary>
    public class RoadNetwork : MonoBehaviour
    {
        [SerializeField] private RoadPrefabLibrary _prefabLibrary;
        [SerializeField] private Transform _segmentsParent;
        [SerializeField] private Transform _junctionsParent;

        private readonly Dictionary<int, RoadNode> _nodes = new Dictionary<int, RoadNode>();
        private readonly Dictionary<int, RoadSegmentData> _segments = new Dictionary<int, RoadSegmentData>();

        private int _nextNodeId;
        private int _nextSegmentId;

        public void Initialize(RoadPrefabLibrary library)
        {
            _prefabLibrary = library;
        }

        public void BuildRoad(IReadOnlyList<Vector3> points)
        {
            if (points == null || points.Count < 2 || _prefabLibrary == null)
            {
                return;
            }

            List<RoadNode> pathNodes = BuildOrReusePathNodes(points);
            CreateSegments(pathNodes);
            RefreshJunctions(pathNodes);
        }

        public RoadNode FindNearestNode(Vector3 worldPosition, float radius)
        {
            RoadNode nearestNode = null;
            float bestDistance = radius;

            foreach (KeyValuePair<int, RoadNode> pair in _nodes)
            {
                float distance = Vector3.Distance(pair.Value.Position, worldPosition);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearestNode = pair.Value;
                }
            }

            return nearestNode;
        }

        private List<RoadNode> BuildOrReusePathNodes(IReadOnlyList<Vector3> points)
        {
            List<RoadNode> pathNodes = new List<RoadNode>(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                pathNodes.Add(GetOrCreateNode(points[i]));
            }

            return pathNodes;
        }

        private void CreateSegments(List<RoadNode> pathNodes)
        {
            for (int i = 0; i < pathNodes.Count - 1; i++)
            {
                RoadNode fromNode = pathNodes[i];
                RoadNode toNode = pathNodes[i + 1];

                if (SegmentExists(fromNode.Id, toNode.Id))
                {
                    continue;
                }

                fromNode.AddConnection(toNode.Id);
                toNode.AddConnection(fromNode.Id);

                RoadSegmentData segment = new RoadSegmentData(_nextSegmentId++, fromNode.Id, toNode.Id);
                _segments[segment.Id] = segment;

                PlaceSegmentObjects(segment, fromNode.Position, toNode.Position);
            }
        }

        private void RefreshJunctions(List<RoadNode> pathNodes)
        {
            for (int i = 0; i < pathNodes.Count; i++)
            {
                RefreshJunction(pathNodes[i]);
            }
        }

        private RoadNode GetOrCreateNode(Vector3 position)
        {
            RoadNode existingNode = FindNearestNode(position, _prefabLibrary.snapRadius);
            if (existingNode != null)
            {
                return existingNode;
            }

            RoadNode newNode = new RoadNode(_nextNodeId++, position);
            _nodes[newNode.Id] = newNode;
            return newNode;
        }

        private bool SegmentExists(int firstNodeId, int secondNodeId)
        {
            foreach (KeyValuePair<int, RoadSegmentData> pair in _segments)
            {
                RoadSegmentData segment = pair.Value;
                bool directMatch = segment.NodeAId == firstNodeId && segment.NodeBId == secondNodeId;
                bool reverseMatch = segment.NodeAId == secondNodeId && segment.NodeBId == firstNodeId;

                if (directMatch || reverseMatch)
                {
                    return true;
                }
            }

            return false;
        }

        private void PlaceSegmentObjects(RoadSegmentData segment, Vector3 start, Vector3 end)
        {
            if (_prefabLibrary.straightPrefab == null)
            {
                return;
            }

            float totalDistance = Vector3.Distance(start, end);
            if (totalDistance < 0.01f)
            {
                return;
            }

            Vector3 direction = (end - start).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            float segmentLength = _prefabLibrary.segmentLength;
            int fullSegments = Mathf.FloorToInt(totalDistance / segmentLength);
            float remainingDistance = totalDistance - fullSegments * segmentLength;

            for (int i = 0; i < fullSegments; i++)
            {
                Vector3 center = start + direction * (i * segmentLength + segmentLength * 0.5f);
                GameObject piece = Instantiate(_prefabLibrary.straightPrefab, center, rotation, _segmentsParent);
                segment.SegmentObjects.Add(piece);
            }

            if (remainingDistance <= 0.05f)
            {
                return;
            }

            Vector3 remainderCenter = start + direction * (fullSegments * segmentLength + remainingDistance * 0.5f);
            GameObject remainderPiece = Instantiate(_prefabLibrary.straightPrefab, remainderCenter, rotation, _segmentsParent);
            Vector3 localScale = remainderPiece.transform.localScale;
            localScale.z *= remainingDistance / segmentLength;
            remainderPiece.transform.localScale = localScale;
            segment.SegmentObjects.Add(remainderPiece);
        }

        private void RefreshJunction(RoadNode node)
        {
            if (node.JunctionObject != null)
            {
                Destroy(node.JunctionObject);
            }

            RoadJunctionType junctionType = node.GetJunctionType();
            GameObject prefab = PickJunctionPrefab(node, junctionType);
            if (prefab == null)
            {
                return;
            }

            Quaternion rotation = ComputeJunctionRotation(node, junctionType);
            node.JunctionObject = Instantiate(prefab, node.Position, rotation, _junctionsParent);
        }

        private GameObject PickJunctionPrefab(RoadNode node, RoadJunctionType junctionType)
        {
            switch (junctionType)
            {
                case RoadJunctionType.EndCap:
                    return _prefabLibrary.endCapPrefab;
                case RoadJunctionType.Through:
                    return PickThroughPrefab(node);
                case RoadJunctionType.TJunction:
                    return _prefabLibrary.tJunctionPrefab;
                case RoadJunctionType.XJunction:
                    return _prefabLibrary.xJunctionPrefab;
                default:
                    return null;
            }
        }

        private GameObject PickThroughPrefab(RoadNode node)
        {
            if (node.ConnectedNodeIds.Count != 2)
            {
                return _prefabLibrary.straightPrefab;
            }

            Vector3 directionA = (_nodes[node.ConnectedNodeIds[0]].Position - node.Position).normalized;
            Vector3 directionB = (_nodes[node.ConnectedNodeIds[1]].Position - node.Position).normalized;
            float deviationFromLine = Mathf.Abs(180f - Vector3.Angle(directionA, directionB));

            return deviationFromLine <= _prefabLibrary.straightAngleThreshold
                ? null
                : _prefabLibrary.curvePrefab;
        }

        private Quaternion ComputeJunctionRotation(RoadNode node, RoadJunctionType junctionType)
        {
            if (node.ConnectedNodeIds.Count == 0)
            {
                return Quaternion.identity;
            }

            Vector3 firstDirection = (_nodes[node.ConnectedNodeIds[0]].Position - node.Position).normalized;
            firstDirection.y = 0f;

            if (junctionType == RoadJunctionType.Through && node.ConnectedNodeIds.Count == 2)
            {
                Vector3 secondDirection = (_nodes[node.ConnectedNodeIds[1]].Position - node.Position).normalized;
                secondDirection.y = 0f;

                Vector3 bisector = (firstDirection + secondDirection).normalized;
                if (bisector == Vector3.zero)
                {
                    bisector = firstDirection;
                }

                return Quaternion.LookRotation(bisector, Vector3.up);
            }

            if (firstDirection == Vector3.zero)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(firstDirection, Vector3.up);
        }
    }
}
