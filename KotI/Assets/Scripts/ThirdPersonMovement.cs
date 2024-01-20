using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    private Animator _animator;
    public GameObject camera;
    public PlayerState state;


    public float speed = 6f;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    private void Start()
    {
        camera = GameObject.Find("Main Camera");   
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        switch(state)
        {
            case PlayerState.Idle:
                speed = 6f;
                _animator.SetInteger("AnimationType", 0);
                break;
            case PlayerState.Walking:
                speed = 6f;
                _animator.SetInteger("AnimationType", 1);
                break;
            case PlayerState.Running:
                speed = 15f;
                _animator.SetInteger("AnimationType", 2);
                break;
            default:
                break;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                state = PlayerState.Running;
            }
            else
            {
                state = PlayerState.Walking;
            }

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            state = PlayerState.Idle;
            
        }

        camera.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    public enum PlayerState
    {
        Idle,
        Walking,
        Running
    }
}
