using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{   
    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Attacking
    }

    public Animator animator;
    public PlayerState playerState;

    public CharacterController controller;
    public Transform cam;
    public Transform groundCheck;

    public float speed = 6f;
    public float gravity = -15f;
    public float groundDistance = 0.1f;
    public float jumpForce = 10;

    public LayerMask groundMask;

    bool isGrounded;

    Vector3 velocity;

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
        switch(playerState)
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

            case PlayerState.Attacking:
                animator.SetInteger("AnimationType", 3);
                break;

            default:
                break;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3 (horizontal, 0f, vertical).normalized;

        if(direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVel, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            velocity.x = moveDirection.x;
            velocity.z = moveDirection.z;

            controller.Move(moveDirection.normalized * speed * Time.deltaTime);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                playerState = PlayerState.Running;
            }
            else
            {
                playerState = PlayerState.Walking;
            }
        }
        else if(isGrounded)
        {
            playerState= PlayerState.Idle;
            velocity.x = 0;
            velocity.z = 0;
        }
        else
        {
            velocity.x = 0;
            velocity.z = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            playerState = PlayerState.Jumping;
            Jump();
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Jump()
    {
        velocity.y = jumpForce;
    }
}
