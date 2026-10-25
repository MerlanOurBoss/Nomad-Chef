using NomadChef.Counters;
using NomadChef.KitchenObjects;
using UnityEngine;

namespace NomadChef.Player
{
    /// <summary>
    /// Procedural chef animation: walk bob and leg/arm swing, arms forward while carrying,
    /// chopping and scrubbing at work stations, a lean while dashing and gentle breathing at rest.
    /// Works on plain joint transforms, so no Animator or rig is needed.
    /// In the rest pose the arms hang down; rotating a shoulder by a negative X angle raises the arm forward.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class ChefAnimator : MonoBehaviour
    {
        [Header("Joints")]
        [SerializeField] private Transform torso;
        [SerializeField] private Transform head;
        [SerializeField] private Transform shoulderLeft;
        [SerializeField] private Transform shoulderRight;
        [SerializeField] private Transform hipLeft;
        [SerializeField] private Transform hipRight;

        [Header("Walk")]
        [Tooltip("Walk cycles (a left and a right step) per metre walked.")]
        [SerializeField] private float stepsPerMetre = 0.85f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float bobHeight = 0.05f;
        [SerializeField] private float swayAngle = 4f;
        [SerializeField] private float legSwing = 32f;
        [SerializeField] private float armSwing = 26f;
        [SerializeField] private float dashLean = 18f;

        [Header("Poses")]
        [SerializeField] private float carryAngle = 68f;
        [Tooltip("Keep in step with the cutting board knife.")]
        [SerializeField] private float chopsPerSecond = 6f;
        [SerializeField] private float chopAngle = 35f;
        [SerializeField] private float scrubsPerSecond = 3f;
        [SerializeField] private float poseSharpness = 14f;

        private PlayerController player;
        private Vector3 torsoRest;
        private Vector3 lastPosition;
        private float walkPhase;
        private float walkBlend;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            torsoRest = torso.localPosition;
            lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
                return;

            Vector3 delta = transform.position - lastPosition;
            delta.y = 0f;
            lastPosition = transform.position;
            float speed = delta.magnitude / dt;

            walkBlend = Mathf.MoveTowards(walkBlend, Mathf.Clamp01(speed / runSpeed * 1.5f), dt * 6f);
            walkPhase += delta.magnitude * stepsPerMetre * Mathf.PI * 2f;
            float stride = Mathf.Sin(walkPhase);
            float breath = Mathf.Sin(Time.time * 2f) * 0.006f;

            float blend = 1f - Mathf.Exp(-poseSharpness * dt);

            // Torso: bob on every footfall, sway side to side, lean into a dash.
            torso.localPosition = torsoRest + Vector3.up * (Mathf.Abs(stride) * bobHeight * walkBlend + breath);
            float lean = player.IsDashing ? dashLean : walkBlend * 6f;
            torso.localRotation = Quaternion.Slerp(torso.localRotation,
                Quaternion.Euler(lean, 0f, stride * swayAngle * walkBlend), blend);

            hipLeft.localRotation = Quaternion.Euler(stride * legSwing * walkBlend, 0f, 0f);
            hipRight.localRotation = Quaternion.Euler(-stride * legSwing * walkBlend, 0f, 0f);

            ArmTargets(stride, out Quaternion left, out Quaternion right, out float headTilt);
            shoulderLeft.localRotation = Quaternion.Slerp(shoulderLeft.localRotation, left, blend);
            shoulderRight.localRotation = Quaternion.Slerp(shoulderRight.localRotation, right, blend);
            head.localRotation = Quaternion.Slerp(head.localRotation, Quaternion.Euler(headTilt, 0f, -stride * 2f * walkBlend), blend);
        }

        private void ArmTargets(float stride, out Quaternion left, out Quaternion right, out float headTilt)
        {
            headTilt = 0f;

            if (player.SelectedCounter is IWorkStation station && station.IsWorkedBy(player))
            {
                headTilt = 12f; // look down at the work
                if (station.Motion == WorkMotion.Chop)
                {
                    // Same rhythm as the knife: down on the board when the sine crosses zero.
                    float raise = Mathf.Abs(Mathf.Sin(Time.time * chopsPerSecond * Mathf.PI));
                    right = Quaternion.Euler(-(55f + chopAngle * raise), 0f, 0f);
                    left = Quaternion.Euler(-50f, 0f, 12f);
                }
                else
                {
                    float a = Time.time * scrubsPerSecond * Mathf.PI * 2f;
                    left = Quaternion.Euler(-55f + Mathf.Sin(a) * 12f, 0f, Mathf.Cos(a) * 10f);
                    right = Quaternion.Euler(-55f - Mathf.Sin(a) * 12f, 0f, -Mathf.Cos(a) * 10f);
                }
                return;
            }

            if (player.KitchenObject != null)
            {
                float bounce = stride * 3f * walkBlend;
                bool spraying = player.KitchenObject is Extinguisher extinguisher && extinguisher.IsSpraying;
                float shake = spraying ? Mathf.Sin(Time.time * 40f) * 1.5f : 0f;
                left = Quaternion.Euler(-carryAngle + bounce + shake, 0f, 0f);
                right = Quaternion.Euler(-carryAngle - bounce + shake, 0f, 0f);
                return;
            }

            // Free hands swing opposite to the legs.
            left = Quaternion.Euler(-stride * armSwing * walkBlend, 0f, -4f);
            right = Quaternion.Euler(stride * armSwing * walkBlend, 0f, 4f);
        }
    }
}
