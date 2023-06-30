using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushRigidbodies : MonoBehaviour
{
    public float pushForce = 1;
    void OnControllerColliderHit()
    {

    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    { 
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body != null)
        {
            if (hit.moveDirection.y < -0.3f)
            { 
            }
            else
            { 
                Vector3 dir = hit.gameObject.transform.position - transform.position;
                dir.y = 0;
                dir.Normalize();
                body.AddForceAtPosition(dir * pushForce, transform.position, ForceMode.Impulse);
            }
        }
    }
}
