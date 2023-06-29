using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{ 
    [SerializeField] private int stability = 0;
    [SerializeField] private Rigidbody body; 
    [SerializeField] private int hitpoints = 100;
    [SerializeField] private bool alive = true;
    LayerMask blockLayer;
    [SerializeField] private List<Block> supportingBlocks;
    [SerializeField] private int weight = 20;
    private int sideStabilityPenalty = 10;
    private int maxStability = 200;
    private bool supportedFromBelow = false;

    public enum BlockType //different block types don't join together automatically.
    {
        Ground,
        Wood,
        Stone
    }
    [SerializeField] private BlockType type = BlockType.Wood;

    private void Awake()
    {
        blockLayer = LayerMask.GetMask("Block");
        if (!gameObject.isStatic)
        { 
            FindSupports(); //check down, and all sides
            //UpdateNearbyBlocks(); //see if any block stability changes as a result
        }
    }
    private void FindSupports()
    {
        stability = 0;
        float length = 1;
        CheckSupportFromBelow();
        CheckSideSupport(Vector3.forward, length);
        CheckSideSupport(Vector3.back, length);
        CheckSideSupport(Vector3.left, length);
        CheckSideSupport(Vector3.right, length);
    }
    private void OnCollisionEnter(Collision collision)
    {
        CheckSupportFromBelow();
    }
    private void CheckSupportFromBelow()
    { 
        float length = 1;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, length, blockLayer))
        {
            Block block = hit.collider.GetComponent<Block>();
            if (block != null)
            {
                supportedFromBelow = true;
                SetStability(block.stability); //inherit stability 
            }
        }
    }
    private void CheckSideSupport(Vector3 dir, float length)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, length, blockLayer))
        {
            Block block = hit.collider.GetComponent<Block>();
            if (block != null)
            { 
                //using side support is penalized by weight. heavier means less supported blocks
                AddStability(block.stability - weight);
            }
        }
    }
    private void UpdateNearbyBlocks()
    { 
        float length = 1; 
        GiveSideSupport(Vector3.forward, length);
        GiveSideSupport(Vector3.back, length);
        GiveSideSupport(Vector3.left, length);
        GiveSideSupport(Vector3.right, length);
    } 
    private void GiveSideSupport(Vector3 dir, float length)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(dir), out hit, length, blockLayer))
        {
            Block block = hit.collider.GetComponent<Block>();
            if (block != null)
            {
                block.FindSupports();
            }
        }
    }
    private void AddStability(int set)
    {
        stability = Mathf.Clamp(stability + set, 0, maxStability);
        CheckStability();
    }
    private void SetStability(int set)
    {
        stability = Mathf.Clamp(set, 0, maxStability);
        CheckStability();
    }
    private void CheckStability()
    { 
        if (stability > 0)
        {
            body.useGravity = false;
            body.isKinematic = true;
            CleanUpPosition();
        }
        else
        { 
            body.useGravity = true;
            body.isKinematic = false;
        }
    }
    private Vector3 RoundVector(Vector3 pos)
    {
        return new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
    }
    private void CleanUpPosition()
    {
        transform.position = RoundVector(transform.position);
        transform.rotation = Quaternion.identity;
    }
    private void AdjacencyChecks()
    { 
        float length = 1f; 
        RaycastHit hit;

        if (body.SweepTest(transform.forward, out hit, length))
        {
            Debug.Log("front");
            JoinBlocks(hit.collider);
        }

        if (body.SweepTest(-transform.right, out hit, length))
        {
            Debug.Log("left");
            JoinBlocks(hit.collider);
        }

        if (body.SweepTest(transform.right, out hit, length))
        {
            Debug.Log("right");
            JoinBlocks(hit.collider);
        }

        if (body.SweepTest(transform.up, out hit, length))
        {
            Debug.Log("up");
            JoinBlocks(hit.collider);
        }

        if (body.SweepTest(-transform.up, out hit, length))
        {
            Debug.Log("down");
            JoinBlocks(hit.collider);
        }
    }
    private void JoinBlocks(Collider collider)
    { 
        Block block = collider.GetComponent<Block>();
        if (block != null && block.type == type)
        {
            FixedJoint joint = gameObject.AddComponent<FixedJoint>();
            Rigidbody rigid = block.gameObject.GetComponent<Rigidbody>();
            joint.connectedBody = rigid;
        }
    } 
    public void TakeDamage(int damage)
    {
        hitpoints -= damage;
        if (hitpoints <= 0 && alive)
        {
            alive = false;
            Crumble();
        }
    }

    private void Crumble()
    {  
    } 
}
