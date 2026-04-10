using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Stores road connectivity and applies simple transform-based visuals.
    /// </summary>
    public class Road : MonoBehaviour
    {
        public int ConnectionMask { get; private set; }
        public int CoinsSpentToPlace { get; private set; }

        public void SetPlacementCost(int coinsSpent)
        {
            CoinsSpentToPlace = Mathf.Max(0, coinsSpent);
        }

        public void SetConnections(bool north, bool east, bool south, bool west)
        {
            int mask = 0;
            if (north) mask |= 1;
            if (east) mask |= 2;
            if (south) mask |= 4;
            if (west) mask |= 8;

            ConnectionMask = mask;
            ApplySimpleVisual();
        }

        private void ApplySimpleVisual()
        {
            bool north = (ConnectionMask & 1) != 0;
            bool east = (ConnectionMask & 2) != 0;
            bool south = (ConnectionMask & 4) != 0;
            bool west = (ConnectionMask & 8) != 0;

            transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
            transform.localRotation = Quaternion.identity;

            // Only cardinal (90-degree) rotations to keep road orientation stable and grid-aligned.
            switch (ConnectionMask)
            {
                // no neighbors
                case 0:
                    transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
                    transform.localRotation = Quaternion.identity;
                    break;

                // dead-ends (N, E, S, W)
                case 1:
                    transform.localScale = new Vector3(0.4f, 0.1f, 0.9f);
                    transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    break;
                case 2:
                    transform.localScale = new Vector3(0.4f, 0.1f, 0.9f);
                    transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    break;
                case 4:
                    transform.localScale = new Vector3(0.4f, 0.1f, 0.9f);
                    transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    break;
                case 8:
                    transform.localScale = new Vector3(0.4f, 0.1f, 0.9f);
                    transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
                    break;

                // straights
                case 5: // N + S
                    transform.localScale = new Vector3(0.4f, 0.1f, 1.2f);
                    transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    break;
                case 10: // E + W
                    transform.localScale = new Vector3(0.4f, 0.1f, 1.2f);
                    transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    break;

                // corners (NE, ES, SW, WN)
                case 3:
                    transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
                    transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    break;
                case 6:
                    transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
                    transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    break;
                case 12:
                    transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
                    transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    break;
                case 9:
                    transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
                    transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
                    break;

                // T-junctions (missing N, E, S, W respectively)
                case 14:
                    transform.localScale = new Vector3(1f, 0.1f, 1f);
                    transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    break;
                case 13:
                    transform.localScale = new Vector3(1f, 0.1f, 1f);
                    transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
                    break;
                case 11:
                    transform.localScale = new Vector3(1f, 0.1f, 1f);
                    transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    break;
                case 7:
                    transform.localScale = new Vector3(1f, 0.1f, 1f);
                    transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    break;

                // 4-way
                case 15:
                    transform.localScale = new Vector3(1f, 0.1f, 1f);
                    transform.localRotation = Quaternion.identity;
                    break;

                default:
                    transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
                    transform.localRotation = Quaternion.identity;
                    break;
            }
        }
    }
}
