using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Attacking,
        Bouncing
    }

    public Animator animator;
    public EnemyState state;

    public CharacterController controller;
    public Transform groundCheck;

    public float speed = 0;
    public float gravity = -15f;
    public float groundDistance = 0.1f;
    public float jumpForce = 15;

    public bool canStop;

    public LayerMask groundMask;

    public bool isGrounded;

    public Vector3 velocity;
    public Vector3 dir;
    public Vector3 moveDirection;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVel;

    private void Start()
    {
        groundCheck = gameObject.GetComponent<Transform>().Find("EnemyGroundCheck").GetComponent<Transform>();
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(groundCheck.position, groundDistance);
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        switch (state)
        {
            case EnemyState.Idle:
                animator.SetInteger("AnimationType", 0);
                break;

            case EnemyState.Walking:
                animator.SetInteger("AnimationType", 1);
                speed = 6f;
                break;

            case EnemyState.Running:
                animator.SetInteger("AnimationType", 2);
                speed = 12f;
                break;

            case EnemyState.Jumping:
                if(!isGrounded)
                {
                    Jump();
                }
                break;
            case EnemyState.Bouncing:
                break;

            case EnemyState.Attacking:
                animator.SetInteger("AnimationType", 3);
                break;

            default:
                break;
        }

        if(dir.magnitude >= 0.1f)
        {
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
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
    }

    public void Jump()
    {
        velocity.y = jumpForce;
    }

    public void BounceBack(float x, float z, float force)
    {
        GameObject.Find("Client").GetComponent<Client>().SendBouncePacket(gameObject.name, x, z, force);
        Jump();
        velocity.x = x * force;
        velocity.z = z * force;
    }
}
