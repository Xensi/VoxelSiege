using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    public Rigidbody body;
    public List<Block> blocks;
    LayerMask blockLayer;
    public void Awake()
    {
        blockLayer = LayerMask.GetMask("Block");
    }
    private void FixedUpdate()
    {
        body.WakeUp();
    }
    public void CalculateBodyMass()
    {
        float mass = 0;
        for (int i = 0; i < blocks.Count; i++)
        {
            mass += blocks[i].mass;
        }
        body.mass = mass;
        body.ResetCenterOfMass();
    } 
    bool allowedCollision = true;
    private void AllowCollisions()
    {
        allowedCollision = true;
    }
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(" " + name + " " + collision.collider.name);
        //List<Block> blockList = new List<Block>();
        if (allowedCollision)
        {
            //Debug.Log("one col");
            allowedCollision = false;
            Invoke("AllowCollisions", .1f);

            foreach (ContactPoint contact in collision.contacts)
            {
                //Debug.Log(contact.thisCollider + " " + contact.otherCollider);
                Collider thisCollider = contact.thisCollider;
                Collider otherCollider = contact.otherCollider;

                Block thisBlock = contact.thisCollider.GetComponent<Block>();
                Block otherBlock = contact.otherCollider.GetComponent<Block>();
                float total = 0;
                if (thisBlock != null && otherBlock != null)
                {
                    float velocity = collision.relativeVelocity.magnitude;
                    float newMass = thisBlock.mass;
                    if (thisBlock.structure != null)
                    { 
                        newMass = thisBlock.structure.body.mass;
                    } 
                    float momentum = (newMass * velocity) / 20;
                    float damage = momentum / collision.contactCount; //more contacts less damage
                    total += damage;
                    if (damage >= 1)
                    {
                        thisBlock.TakeDamage(damage);
                        otherBlock.TakeDamage(damage);
                    }
                }
                Debug.Log(total);
            } 

        } 
    }

}
