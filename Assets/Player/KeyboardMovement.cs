using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class KeyboardMovement : MonoBehaviour
{
    /*
        This script handles the keyboard movement functionality for a first-person character.
        It moves the character based on keyboard input along the horizontal and vertical axes.

        It also applies gravity to the character to simulate falling.
        */
    public float speed = 10.0f;
    public float gravity = -9.8f;
    private CharacterController _charController;
    void Start()
    {
        _charController = GetComponent<CharacterController>();
    }

    void Update()
    {
        float deltaX = Input.GetAxis("Horizontal") * speed;
        float deltaZ = Input.GetAxis("Vertical") * speed;
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = Vector3.ClampMagnitude(movement, speed);
        movement.y = gravity;
        movement *= Time.deltaTime;
        movement = transform.TransformDirection(movement);
        _charController.Move(movement);
    }
}
