using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Resources.Scripts.Player
{
    public class PlayerManager : NetworkBehaviour
    {
        private const string TAG_THIRD_PERSON_CAM = "CinemachineCamera";
        PlayerMovement inputHandler;
        Animator anim;
        private PlayerLocomotion playerLocomotion;

        public CinemachineCamera ThirdPersonCam { get; private set; }

        [field: SerializeField]
        public bool IsInteracting { get; set; }
        [field: SerializeField]
        public bool IsSprinting { get; set; }
        [field: SerializeField]
        public bool IsAirborne { get; set; }
        [field: SerializeField]
        public bool IsGrounded { get; set; }

        [field: SerializeField]
        public bool IsActionable { get { return !IsInteracting && !IsAirborne; } }

        public UnityEvent RollEvent { get; private set; }

        private bool _isNetworkSpawned = false;
        private bool _isWorldReady = false;
        private bool _isInitialized = false;

        private void Awake()
        {
            RollEvent = new UnityEvent();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Only the owning client needs a camera reference; remote players don't drive the local camera.
            if (!IsOwner) return;

            GameObject camObject = GameObject.FindWithTag(TAG_THIRD_PERSON_CAM);
            if (camObject == null || !camObject.TryGetComponent(out CinemachineCamera cam))
            {
                Debug.LogWarning("PlayerManager: Could not find ThirdPersonCam CinemachineCamera in the scene.");
                return;
            }

            ThirdPersonCam = cam;
            ThirdPersonCam.Follow = transform;
            _isNetworkSpawned = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            inputHandler = GetComponent<PlayerMovement>();
            anim = GetComponentInChildren<Animator>();

            playerLocomotion = GetComponent<PlayerLocomotion>();
            _isWorldReady = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner) return;
            InitNetworkedPlayer();
            float delta = Time.deltaTime;

            inputHandler.TickInput(delta);
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            // Rigidbody movement must be applied on the fixed timestep to avoid jitter
            playerLocomotion?.HandleMovement(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            IsSprinting = inputHandler.SprintFlag;

            if (IsAirborne)
            {
                playerLocomotion.airborneTimer += Time.deltaTime;
            }
        }

        private void InitNetworkedPlayer()
        {
            if (_isInitialized) return;
            if (!_isNetworkSpawned || !_isWorldReady) return;

            playerLocomotion.SetOrbitalFollow(ThirdPersonCam.GetComponentInChildren<CinemachineOrbitalFollow>());
            _isInitialized = true;
        }
    }
}
