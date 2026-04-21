using UnityEngine;

public class Movement : MonoBehaviour
{
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public float turnSpeed = 10f;
    public float gravity = -20f;
    public float jumpForce = 6f;

    private CharacterController _controller;
    private ThirdPersonCamera _cameraRig;
    private Animator _animator;

    private Vector3 _velocity;
    private bool _isGrounded;
    private float _currentSpeed;

    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int HashJump = Animator.StringToHash("Jump");

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _cameraRig = FindObjectOfType<ThirdPersonCamera>();
    }

    private void Update()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        UpdateAnimator();
    }

    private void CheckGround()
    {
        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward;
        Vector3 right;

        if (_cameraRig != null)
        {
            forward = _cameraRig.GetCameraForward();
            right = _cameraRig.GetCameraRight();
        }
        else
        {
            forward = transform.forward;
            right = transform.right;
        }

        // sicurezza: garantisci che siano normalizzati e piatti
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * v + right * h;
        if (move.magnitude > 1f) move.Normalize();

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = move.magnitude > 0.1f ? (isRunning ? runSpeed : walkSpeed) : 0f;
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, 10f * Time.deltaTime);

        // ROTAZIONE PLAYER verso direzione movimento
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        _controller.Move(move * _currentSpeed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _velocity.y = jumpForce;
            if (_animator != null)
                _animator.SetTrigger(HashJump);
        }
    }

    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        if (_animator == null) return;
        _animator.SetFloat(HashSpeed, _currentSpeed, 0.1f, Time.deltaTime);
        _animator.SetBool(HashGrounded, _isGrounded);
    }
}