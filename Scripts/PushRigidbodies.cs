using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushRigidbodies : MonoBehaviour
{
    public float pushForce = 1;
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        /* Rigidbody body = hit.rigidbody;

         // no rigidbody
         if (body == null || body.isKinematic)
             return;

         // We dont want to push objects below us
         if (hit.moveDirection.y < -0.3f)
             return;
         // Calculate push direction from move direction,
         // we only push objects to the sides never up and down
         Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z); 

         // Apply the push
         body.velocity = pushDir * pushForce;*/
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body != null)
        {
            if (hit.moveDirection.y < -0.3f) return;
            Vector3 dir = hit.gameObject.transform.position - transform.position;
            dir.y = 0;
            dir.Normalize();
            body.AddForceAtPosition(dir * pushForce, transform.position, ForceMode.Impulse);
        }
    }
}
