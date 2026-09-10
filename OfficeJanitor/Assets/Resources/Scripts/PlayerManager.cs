using UnityEngine;
using UnityEngine.Events;

namespace OGS
{
    public class PlayerManager : MonoBehaviour
    {
        PlayerMovement inputHandler;
        Animator anim;
        private PlayerLocomotion playerLocomotion;

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


        private void Awake()
        {
            RollEvent = new UnityEvent();
        }

        // Start is called before the first frame update
        void Start()
        {
            inputHandler = GetComponent<PlayerMovement>();
            anim = GetComponentInChildren<Animator>();

            playerLocomotion = GetComponent<PlayerLocomotion>();

        }

        // Update is called once per frame
        void Update()
        {
            float delta = Time.deltaTime;

            inputHandler.TickInput(delta);
            playerLocomotion?.HandleMovement(delta);
        }

        private void FixedUpdate()
        {
            
        }

        private void LateUpdate()
        {
            IsSprinting = inputHandler.SprintFlag;

            if (IsAirborne)
            {
                playerLocomotion.airborneTimer += Time.deltaTime;
            }
        }
    }
}
