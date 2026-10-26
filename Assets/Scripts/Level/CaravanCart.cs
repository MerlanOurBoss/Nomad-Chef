using UnityEngine;

namespace NomadChef.Level
{
    /// <summary>
    /// A kitchen cart that drifts away from the other cart and back on a fixed loop.
    /// Counters and items are children, so they ride along; chefs standing on the deck are
    /// carried by moving their CharacterController. While apart, a wall closes the open edge.
    /// Carts with the same timings stay in sync because they all start their loop together.
    /// </summary>
    public class CaravanCart : MonoBehaviour
    {
        [SerializeField] private Vector3 openOffset = new Vector3(-1.2f, 0f, 0f);
        [SerializeField, Min(0f)] private float closedTime = 12f;
        [SerializeField, Min(0.1f)] private float moveTime = 2f;
        [SerializeField, Min(0f)] private float openTime = 8f;
        [SerializeField] private Collider edgeWall;
        [Tooltip("Area above the deck (local space) where chefs count as riding.")]
        [SerializeField] private Vector3 riderAreaCenter = new Vector3(0f, 1f, 0f);
        [SerializeField] private Vector3 riderAreaSize = new Vector3(6f, 2f, 7f);
        [SerializeField] private LayerMask riderMask;

        private static readonly Collider[] Riders = new Collider[8];

        private Vector3 closedPosition;
        private float clock;

        private void Start()
        {
            closedPosition = transform.position;
        }

        private void Update()
        {
            clock += Time.deltaTime;
            float open = OpenAmount(clock);

            Vector3 delta = closedPosition + openOffset * open - transform.position;
            edgeWall.enabled = open > 0.01f;
            if (delta == Vector3.zero)
                return;

            transform.position += delta;
            Physics.SyncTransforms(); // riders must collide with the counters' new positions

            int count = Physics.OverlapBoxNonAlloc(transform.TransformPoint(riderAreaCenter), riderAreaSize * 0.5f, Riders,
                transform.rotation, riderMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (Riders[i] is CharacterController rider)
                    rider.Move(delta);
            }
        }

        /// <summary>0 = together, 1 = fully apart, eased in between.</summary>
        private float OpenAmount(float time)
        {
            float t = time % (closedTime + moveTime + openTime + moveTime);
            if (t < closedTime)
                return 0f;
            t -= closedTime;
            if (t < moveTime)
                return Mathf.SmoothStep(0f, 1f, t / moveTime);
            t -= moveTime;
            if (t < openTime)
                return 1f;
            return Mathf.SmoothStep(1f, 0f, (t - openTime) / moveTime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(riderAreaCenter, riderAreaSize);
        }
    }
}
