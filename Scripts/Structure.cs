using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    public Rigidbody body;
    public List<Block> blocks;

    public void CalculateBodyMass()
    {
        float mass = 0;
        for (int i = 0; i < blocks.Count; i++)
        {
            mass += blocks[i].weight;
        }
        body.mass = mass;
        body.ResetCenterOfMass();
    }
    private void FixedUpdate()
    {
        body.WakeUp();
    }

}
