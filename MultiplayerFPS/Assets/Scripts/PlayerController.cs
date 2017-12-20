using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour {

	[SerializeField]
	private float speed = 5f;
	[SerializeField]
	private float lookSensitivity = 3f;

    private float jump_force = 500f;

    private float VerticalVelocity;
    [SerializeField]
    private float gravity = 14.0f;

    [SerializeField]
    private LayerMask enviromentMask;

	// Component caching
	private PlayerMotor motor;
    private new BoxCollider collider;
    private Animator animator;
    private CharacterController controller;

	void Start ()
	{
		motor = GetComponent<PlayerMotor>();
		animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        collider = GetComponent<BoxCollider>();
	}

    void Update()
    {
        if (PauseMenu.IsOn)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }

            motor.Move(Vector3.zero);
            motor.Rotate(Vector3.zero);
            motor.RotateCamera(0f);

            return;

        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        //Calculate movement velocity as a 3D vector
        float _xMov = Input.GetAxis("Horizontal");
        float _zMov = Input.GetAxis("Vertical");

        Vector3 _movHorizontal = transform.right * _xMov;
        Vector3 _movVertical = transform.forward * _zMov;

        // Final movement vector
        Vector3 _velocity = (_movHorizontal + _movVertical) * speed;

        // Animate movement
        animator.SetFloat("ForwardVelocity", _zMov);

        //Apply movement
        motor.Move(_velocity);

        //Calculate rotation as a 3D vector (turning around)
        float _yRot = Input.GetAxisRaw("Mouse X");

        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * lookSensitivity;

        //Apply rotation
        motor.Rotate(_rotation);

        //Calculate camera rotation as a 3D vector (turning around)
        float _xRot = Input.GetAxisRaw("Mouse Y");

        float _cameraRotationX = _xRot * lookSensitivity;

        //Apply camera rotation
        motor.RotateCamera(_cameraRotationX);

        //Jump
        if (Input.GetButtonDown("Jump") && is_grounded())
        {
            motor.handle_jump(jump_force);
        }
    }


    private bool is_grounded()
    {
        float distance_to_ground = collider.bounds.extents.y;
        return Physics.Raycast(transform.position, -Vector3.up, distance_to_ground + 0.5f);
    }

}
