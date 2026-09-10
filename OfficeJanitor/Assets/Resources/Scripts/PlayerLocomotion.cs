using UnityEngine;

namespace OGS
{
    public class PlayerLocomotion : MonoBehaviour
    {
        Transform cameraObject;
        PlayerMovement inputHandler;
        public Vector3 moveDirection;

        [HideInInspector]
        public Transform myTransform;

        public Rigidbody Rigidbody { get; private set; }
        public GameObject normalCamera;

        private PlayerManager playerManager;

        [Header("Ground & Air Detection Stats")]
        [SerializeField]
        private float groundDetectionRayStartPoint = 0.5f;
        [SerializeField]
        private float minDistanceNeededToBeginFall = 1f;
        [SerializeField]
        private float groundDirectionRayDistance = 0.2f;
        private LayerMask ignoreForGroundCheck;
        public float airborneTimer;

        [Header("Movement Stats")]
        [SerializeField]
        private float movementSpeed = 5;
        [SerializeField]
        private float sprintSpeed = 7;
        [SerializeField]
        private float rotationSpeed = 10;
        [SerializeField]
        private float fallingSpeed = 45;
        [SerializeField]
        private float maxFallingSpeed = 45;
        [SerializeField]
        private float bumpOffLedgeSpeed = 10f;

        void Awake()
        {
            playerManager = GetComponent<PlayerManager>();
            Rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<PlayerMovement>();

            cameraObject = Camera.main.transform;
            myTransform = transform;
            playerManager.IsGrounded = true;
            ignoreForGroundCheck = ~(1 << 8 | 1 << 11);
        }

        #region Movement
        Vector3 normalVector;
        Vector3 targetPosition;

        public void HandleMovement(float delta)
        {
            if (playerManager.IsInteracting)
            {
                return;
            }
            HandleWalking();
        }

        private void HandleWalking()
        {
            Debug.Log($"dir:{moveDirection} cam:{cameraObject} ver{inputHandler.Vertical} hor{inputHandler.Horizontal}");
        
            moveDirection = cameraObject.forward * inputHandler.Vertical;
            moveDirection += cameraObject.right * inputHandler.Horizontal;
            moveDirection.Normalize();
            moveDirection.y = 0;

            float speed = movementSpeed;

            if (inputHandler.SprintFlag && inputHandler.MoveAmount > 0.5)
            {
                speed = sprintSpeed;
                playerManager.IsSprinting = true;
            }
            else
            {
                playerManager.IsSprinting = false;
            }
            moveDirection *= speed;

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
            Rigidbody.linearVelocity = projectedVelocity;
        }

        private void HandleRotation(float delta)
        {
            Vector3 targetDir;
            float moveOverride = inputHandler.MoveAmount;

            targetDir = cameraObject.forward * inputHandler.Vertical;
            targetDir += cameraObject.right * inputHandler.Horizontal;

            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
            {
                targetDir = myTransform.forward;
            }

            float rs = rotationSpeed;
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

            myTransform.rotation = targetRotation;
        }

        #endregion

    }
}
