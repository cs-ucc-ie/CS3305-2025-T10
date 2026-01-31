using UnityEngine;

public class MouseLook : MonoBehaviour
{
    /*
        This script handles the mouse look functionality for a first-person camera.
        It rotates the camera based on mouse movement.
        Remember: X axis is bind on player, Y axis is bind on camera.

        It also locks the cursor to the center of the screen and makes it invisible.
        */
public enum RotationAxes {MouseXAndY = 0, MouseX = 1, MouseY = 2}

    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityHor = 9.0f;
    public float sensitivityVert = 9.0f;
    public float maximumVert = 45.0f;
    public float minimumVert = -45.0f;
    private float _rotationX = 0;
    private Camera _camera;
    void Update()
    {
        if (axes == RotationAxes.MouseX){
            // horizontal
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityHor, 0);
        } else if (axes == RotationAxes.MouseY){
            // vertical
            _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
            _rotationX = Mathf.Clamp(_rotationX, minimumVert, maximumVert);

            float rotationY = transform.localEulerAngles.y;
            transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
        } else {
            // both horizontal and vertical
            _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
            _rotationX = Mathf.Clamp(_rotationX, minimumVert, maximumVert);

            float delta = Input.GetAxis("Mouse X") * sensitivityHor;
            float rotationY = transform.localEulerAngles.y + delta;
            transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
        }
    }
    void Start()
    {
        _camera = GetComponent<Camera>();
        // lock cursor is moved to UI controller
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }
}
