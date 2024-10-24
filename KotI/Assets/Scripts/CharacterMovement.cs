using TMPro;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Attacking,
        Bouncing,
        Win
    }

    public Animator animator;
    public PlayerState playerState;

    public CharacterController controller;
    public Transform cam;
    public Transform groundCheck;
    public Vector3 moveDirection;

    public float speed = 6f;
    public float gravity = -15f;
    public float groundDistance = 0.1f;
    public float jumpForce = 10;

    public LayerMask groundMask;

    bool isGrounded;
    bool isAttacking;
    bool canStop;

    public Vector3 velocity;
    public Vector3 dir;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVel;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Transform>();
        groundCheck = GameObject.Find("GroundCheck").GetComponent<Transform>();
        animator = GameObject.Find("NinjaGFX").GetComponent<Animator>();
    }

    void Update()
    {

        switch (playerState)
        {
            case PlayerState.Idle:
                animator.SetInteger("AnimationType", 0);
                break;

            case PlayerState.Walking:
                animator.SetInteger("AnimationType", 1);
                speed = 6f;
                break;

            case PlayerState.Running:
                animator.SetInteger("AnimationType", 2);
                speed = 12f;
                break;

            case PlayerState.Jumping:
                break;

            case PlayerState.Bouncing:
                break;

            case PlayerState.Attacking:
                animator.SetInteger("AnimationType", 3);
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f)
                {
                    isAttacking = false;
                    playerState = PlayerState.Idle;
                    animator.speed = 1f;
                }
                break;

            default:
                break;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }


        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        dir = direction;

        if (Input.GetMouseButton(0) && !isAttacking)
        {
            isAttacking = true;
            animator.speed = 2f;
            playerState = PlayerState.Attacking;
        }

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVel, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDirection.normalized * speed * Time.deltaTime);

            if (Input.GetKey(KeyCode.LeftShift) && !isAttacking)
            {
                playerState = PlayerState.Running;
            }
            else if (!isAttacking)
            {
                playerState = PlayerState.Walking;
            }
        }
        else if (isGrounded && !isAttacking)
        {
            playerState = PlayerState.Idle;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (!isAttacking)
                playerState = PlayerState.Jumping;
            Jump();
        }

        if (isGrounded && canStop)
        {
            velocity.x = 0;
            velocity.z = 0;

            canStop = false;
        }

        if (velocity.y < -2)
        {
            canStop = true;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);


        TextMeshPro text = GetComponentInChildren<TextMeshPro>();
        if (text.gameObject != null)
        {
            Camera mainCamera = Camera.main;
            Quaternion lookRotation = Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up);
            text.gameObject.transform.rotation = lookRotation;
            text.text = PlayerPrefs.GetString("username");
        }

    }

    public void UpdateWin()
    {
        int randomAnim = Random.Range(1, 3) + 9;
        playerState = PlayerState.Win;

        animator.SetInteger("AnimationType", randomAnim);
    }

    private void Jump()
    {
        velocity.y = jumpForce;
    }

    public void BounceBack(float x, float z, float force)
    {
        Jump();
        velocity.x = x * force;
        velocity.z = z * force;
    }
}
