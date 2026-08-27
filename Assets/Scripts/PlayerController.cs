using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -20f; // mas malakas para hindi "float"
    [SerializeField] private float groundedYVelocity = -2f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask = ~0; // "Everything" by default

    private CharacterController _controller;
    private Animator _animator;
    private PlayerHealth _health;
    private Vector3 _velocity;
    private bool _isGrounded;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<PlayerHealth>();
    }

    public override void OnNetworkSpawn()
    {
        enabled = IsOwner;
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Dead players can't move.
        if (_health != null && _health.IsDead.Value)
        {
            _animator.SetFloat(SpeedHash, 0f);
            return;
        }

        // Mas reliable na ground check gamit ang manual raycast/CheckSphere
        // kaysa umasa lang sa _controller.isGrounded (na minsan flaky sa unang frames).
        _isGrounded = Physics.CheckSphere(
            transform.position + Vector3.up * 0.1f,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        // Fallback: gamitin din yung built-in isGrounded ng CharacterController
        if (!_isGrounded)
            _isGrounded = _controller.isGrounded;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v);
        Vector3 moveDir = inputDir.normalized;

        // Gravity handling
        if (_isGrounded && _velocity.y < 0f)
        {
            _velocity.y = groundedYVelocity;
        }
        else
        {
            _velocity.y += gravity * Time.deltaTime;
        }

        Vector3 horizontalMove = moveDir * moveSpeed;
        Vector3 finalMove = horizontalMove + new Vector3(0f, _velocity.y, 0f);
        _controller.Move(finalMove * Time.deltaTime);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        float speedValue = moveDir.magnitude;
        _animator.SetFloat(SpeedHash, speedValue);
    }

    // Optional: makikita mo sa Scene view ang ground check sphere para sa debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.1f, groundCheckDistance);
    }
}