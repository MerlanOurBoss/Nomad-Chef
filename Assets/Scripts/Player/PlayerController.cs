using NomadChef.Counters;
using NomadChef.KitchenObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NomadChef.Player
{
    /// <summary>
    /// One chef. Reads its own PlayerInput (keyboard half or gamepad), moves, dashes,
    /// selects the counter in front of it and carries a single item.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour, IKitchenObjectParent
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotateSpeed = 18f;
        [SerializeField] private float gravity = -20f;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 16f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 0.5f;

        [Header("Interaction")]
        [SerializeField] private float interactDistance = 0.9f;
        [SerializeField] private float interactRadius = 0.25f;
        [SerializeField] private float interactHeight = 0.5f;
        [SerializeField] private LayerMask counterLayerMask;
        [SerializeField] private Transform holdPoint;

        [Header("Throw")]
        [SerializeField] private float throwSpeed = 8f;
        [SerializeField] private float throwLift = 3f;
        [Tooltip("What a thrown ingredient can land on: counters and chefs (not walls, so it flies over the caravan gap).")]
        [SerializeField] private LayerMask throwHitMask;

        [Header("Visuals")]
        [SerializeField] private Renderer[] tintRenderers;

        private CharacterController controller;
        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction interactAction;
        private InputAction interactAlternateAction;
        private InputAction dashAction;

        private float verticalVelocity;
        private float dashTimeLeft;
        private float dashCooldownLeft;
        private Vector3 dashDirection;
        private BaseCounter selectedCounter;

        public Transform KitchenObjectFollowPoint => holdPoint;
        public KitchenObject KitchenObject { get; set; }
        public int PlayerIndex => playerInput.playerIndex;
        public bool IsWalking { get; private set; }
        public bool IsDashing => dashTimeLeft > 0f;
        public BaseCounter SelectedCounter => selectedCounter;
        public bool InteractPressedThisFrame => interactAction.WasPressedThisFrame();
        public bool DashPressedThisFrame => dashAction.WasPressedThisFrame();

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();

            moveAction = playerInput.actions["Move"];
            interactAction = playerInput.actions["Interact"];
            interactAlternateAction = playerInput.actions["InteractAlternate"];
            dashAction = playerInput.actions["Dash"];
        }

        private void OnDisable()
        {
            SetSelectedCounter(null);
        }

        private void Update()
        {
            // Frozen during the countdown and after time is up.
            if (GameManager.Instance != null && !GameManager.Instance.CanPlayersAct)
            {
                IsWalking = false;
                SetSelectedCounter(null);
                if (KitchenObject is Extinguisher extinguisher)
                    extinguisher.SetSpraying(false);
                return;
            }

            HandleMovement();
            HandleSelection();
            HandleInteractions();
        }

        public void SetColor(Color color)
        {
            var block = new MaterialPropertyBlock();
            foreach (Renderer tintRenderer in tintRenderers)
            {
                tintRenderer.GetPropertyBlock(block);
                block.SetColor(BaseColorId, color);
                tintRenderer.SetPropertyBlock(block);
            }
        }

        /// <summary>Moves the chef without the CharacterController fighting the new position.</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = true;
        }

        private void HandleMovement()
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            Vector3 moveDir = new Vector3(input.x, 0f, input.y);
            if (moveDir.sqrMagnitude > 1f)
                moveDir.Normalize();

            dashCooldownLeft -= Time.deltaTime;
            if (dashAction.WasPressedThisFrame() && dashCooldownLeft <= 0f)
            {
                dashDirection = moveDir.sqrMagnitude > 0.01f ? moveDir.normalized : transform.forward;
                dashTimeLeft = dashDuration;
                dashCooldownLeft = dashCooldown;
            }

            Vector3 horizontal;
            if (dashTimeLeft > 0f)
            {
                dashTimeLeft -= Time.deltaTime;
                horizontal = dashDirection * dashSpeed;
            }
            else
            {
                horizontal = moveDir * moveSpeed;
            }

            verticalVelocity = controller.isGrounded ? -1f : verticalVelocity + gravity * Time.deltaTime;
            controller.Move((horizontal + Vector3.up * verticalVelocity) * Time.deltaTime);

            IsWalking = moveDir.sqrMagnitude > 0.01f;

            Vector3 faceDir = IsDashing ? dashDirection : moveDir;
            if (faceDir.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(faceDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, rotateSpeed * Time.deltaTime);
            }
        }

        private void HandleSelection()
        {
            BaseCounter counter = null;
            Vector3 origin = transform.position + Vector3.up * interactHeight;

            if (Physics.SphereCast(origin, interactRadius, transform.forward, out RaycastHit hit,
                    interactDistance, counterLayerMask, QueryTriggerInteraction.Ignore))
            {
                counter = hit.collider.GetComponentInParent<BaseCounter>();
            }

            SetSelectedCounter(counter);
        }

        private void HandleInteractions()
        {
            // Holding the extinguisher turns "use" into spraying.
            var extinguisher = KitchenObject as Extinguisher;
            if (extinguisher != null)
                extinguisher.SetSpraying(interactAlternateAction.IsPressed());

            bool usePressed = interactAlternateAction.WasPressedThisFrame();

            // Holding a throwable ingredient turns "use" into a throw.
            if (usePressed && KitchenObject != null && KitchenObject.Data.CanBeThrown)
            {
                ThrownItem.Launch(KitchenObject, this, transform.forward * throwSpeed + Vector3.up * throwLift, throwHitMask);
                return;
            }

            if (selectedCounter == null || selectedCounter.IsOnFire)
                return;

            if (interactAction.WasPressedThisFrame())
                selectedCounter.Interact(this);

            if (extinguisher == null && usePressed)
                selectedCounter.InteractAlternate(this);
        }

        private void SetSelectedCounter(BaseCounter counter)
        {
            if (counter == selectedCounter)
                return;

            if (selectedCounter != null)
                selectedCounter.RemoveSelector();

            selectedCounter = counter;

            if (selectedCounter != null)
                selectedCounter.AddSelector();
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + Vector3.up * interactHeight;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + transform.forward * interactDistance);
            Gizmos.DrawWireSphere(origin + transform.forward * interactDistance, interactRadius);
        }
    }
}
