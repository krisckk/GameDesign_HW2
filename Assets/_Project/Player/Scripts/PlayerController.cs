using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public float walkSpeed = 2.5f, runSpeed = 6f, turnSpeed = 720f, jumpHeight = 1.2f, gravity = -20f;
    [HideInInspector] public bool canMove = true;

    CharacterController cc;
    InputSystem_Actions input;
    Transform cam;
    float vy;

    void Awake() { cc = GetComponent<CharacterController>(); input = new InputSystem_Actions(); }
    void OnEnable() => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Start()
    {
        cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 m = canMove ? input.Player.Move.ReadValue<Vector2>() : Vector2.zero;
        Vector3 fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 dir = Vector3.ClampMagnitude(fwd * m.y + right * m.x, 1f);
        float speed = input.Player.Sprint.IsPressed() ? runSpeed : walkSpeed;

        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);

        if (cc.isGrounded && vy < 0) vy = -2f;
        if (canMove && cc.isGrounded && input.Player.Jump.WasPressedThisFrame())
        {
            vy = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }
        vy += gravity * Time.deltaTime;

        Vector3 velocity = dir * speed;
        velocity.y = vy;
        cc.Move(velocity * Time.deltaTime);
        animator.SetFloat("Speed", dir.magnitude * speed, 0.1f, Time.deltaTime);
    }
}