using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [field: SerializeField]
    public float Horizontal { get; private set; }
    [field: SerializeField]
    public float Vertical { get; private set; }
    [field: SerializeField]
    public float MoveAmount { get; private set; }
    [field: SerializeField]
    public float MouseX { get; private set; }
    [field: SerializeField]
    public float MouseY { get; private set; }
    [field: SerializeField]
    public bool RollInput { get; private set; }
    [field: SerializeField]
    public bool RollFlag { get; set; }
    [field: SerializeField]
    public bool SprintFlag { get; set; }
    public float rollInputTimer;

    private InputSystem_Actions _inputActions;
    Vector2 movementInput;
    Vector2 cameraInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void OnEnable()
    {
        if(_inputActions == null)
        {
            _inputActions = new InputSystem_Actions();

            _inputActions.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Look.performed += ctx => cameraInput = ctx.ReadValue<Vector2>();
        }

        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    public void TickInput(float delta)
    {
        MoveInput(delta);
        CameraInput(delta);
    }

    private void MoveInput(float delta)
    {
        Horizontal = movementInput.x;
        Vertical = movementInput.y;
        MoveAmount = Mathf.Clamp01(Mathf.Abs(Horizontal) + Mathf.Abs(Vertical));
    }

    private void CameraInput(float delta)
    {
        MouseX = cameraInput.x;
        MouseY = cameraInput.y;
    }

}
