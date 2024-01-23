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
        Attacking
    }

    //public Animator animator;
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

    private void Start()
    {
        groundCheck = GameObject.Find("EnemyGroundCheck").GetComponent<Transform>();
        //animator = GameObject.Find("NinjaGFX").GetComponent<Animator>();
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

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
        velocity.x = x * force;
        velocity.z = z * force;
    }
}
