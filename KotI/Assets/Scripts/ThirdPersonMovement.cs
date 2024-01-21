using UnityEngine;
using Cinemachine;

public class ThirdPersonMovement : MonoBehaviour
{
    //public CharacterController controller;
    private Animator _animator;
    public GameObject camera;
    public PlayerState state;

    public float speed = 10f;

    private void Start()
    {
        camera = GameObject.Find("Client Camera");
        camera.GetComponent<CinemachineFreeLook>().Follow = gameObject.transform;
        camera.GetComponent<CinemachineFreeLook>().LookAt = gameObject.transform;

        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        switch(state)
        {
            case PlayerState.Idle:
                speed = 45f;
                _animator.SetInteger("AnimationType", 0);
                break;
            case PlayerState.Walking:
                speed = 45f;
                _animator.SetInteger("AnimationType", 1);
                break;
            case PlayerState.Running:
                speed = 70f;
                _animator.SetInteger("AnimationType", 2);
                break;
            case PlayerState.Attacking:
                speed = 4f;
                _animator.SetInteger("AnimationType", 3);
                break;
            default:
                break;
        }
    }

    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Attacking
    }
}
