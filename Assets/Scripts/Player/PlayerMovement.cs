using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 80f;
    [SerializeField] private float deceleration = 100f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private Vector3 currentVelocity;
    private CharacterController _charController;
    void Start()
    {
        _charController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (InputManager.Instance == null) return;
        // Vector2 input = InputManager.Instance.MoveInput;
        // float deltaX = input.x * speed;
        // float deltaZ = input.y * speed;
        // Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        // movement = Vector3.ClampMagnitude(movement, speed);
        // movement.y = gravity;
        // movement *= Time.deltaTime;
        // movement = transform.TransformDirection(movement);
        // _charController.Move(movement);
        Vector2 input = InputManager.Instance.MoveInput;

        // local input direction
        Vector3 inputDir = new Vector3(input.x, 0, input.y);
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        Vector3 targetVelocity = inputDir * speed;

        // acceleration / deceleration on whether there are input
        float accel = inputDir.magnitude > 0.01f ? acceleration : deceleration;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            accel * Time.unscaledDeltaTime
        );

    currentVelocity.y = gravity;

        Vector3 move = transform.TransformDirection(currentVelocity);
    _charController.Move(move * Time.unscaledDeltaTime);
    }
}
