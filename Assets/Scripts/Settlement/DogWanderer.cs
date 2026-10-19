using System.Collections;
using UnityEngine;

namespace Reclaim.Settlement
{
    /// <summary>Cachorro figurativo: circula perto do acampamento e descansa. Não tem função no jogo.</summary>
    public class DogWanderer : MonoBehaviour
    {
        private const float RoamRadius = 22f;

        private SettlementContext _context;
        private float _speed;

        public void Initialize(SettlementContext context)
        {
            _context = context;
            _speed = context.Settings.DogSpeed * Random.Range(0.85f, 1.15f);
            StartCoroutine(Wander());
        }

        private IEnumerator Wander()
        {
            yield return new WaitForSeconds(Random.Range(0f, 3f));

            while (true)
            {
                Vector2 offset = Random.insideUnitCircle * RoamRadius;
                Vector3 target = _context.CampCenter + new Vector3(offset.x, 0f, offset.y);
                if (!_context.IsBlocked(target))
                {
                    yield return Walk(target);
                }

                yield return new WaitForSeconds(Random.Range(3f, 10f));
            }
        }

        private IEnumerator Walk(Vector3 target)
        {
            float timeout = Mathf.Max(8f, Vector3.Distance(transform.position, target) / _speed * 3f);
            float elapsed = 0f;

            while (elapsed < timeout)
            {
                Vector3 toTarget = target - transform.position;
                toTarget.y = 0f;
                float distance = toTarget.magnitude;
                if (distance < 0.5f)
                {
                    yield break;
                }

                Vector3 direction = toTarget / distance;
                Vector3 next = transform.position + direction * Mathf.Min(distance, _speed * UnityEngine.Time.deltaTime);
                if (_context.IsBlocked(next))
                {
                    yield break;
                }

                next.y = WorldGround.HeightAt(next.x, next.z, next.y);
                transform.position = next;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 8f * UnityEngine.Time.deltaTime);

                elapsed += UnityEngine.Time.deltaTime;
                yield return null;
            }
        }
    }
}
