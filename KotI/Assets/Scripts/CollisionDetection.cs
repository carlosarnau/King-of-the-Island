using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    CharacterMovement characterController;
    public float force;

    private void Start()
    {
        characterController = GameObject.Find("Player").GetComponent<CharacterMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy" && characterController.playerState == CharacterMovement.PlayerState.Attacking)
        {
            Vector3 bounceDirection = new Vector3(other.gameObject.GetComponent<Transform>().transform.position.x - transform.position.x, 0, other.gameObject.GetComponent<Transform>().transform.position.z - transform.position.z);
            Debug.Log("hitting enemy");
            other.gameObject.GetComponent<EnemyController>().BounceBack(bounceDirection.x, bounceDirection.z, force);
        }
    }
}
