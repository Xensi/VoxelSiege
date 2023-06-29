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
    private int verticalStabilityPenalty = 5;
    private int maxStability = 200;
    [SerializeField] private bool supportedFromBelow = false;

    public enum BlockType //different block types don't join together automatically.
    {
        Ground,
        Wood,
        Stone
    }
    [SerializeField] private BlockType type = BlockType.Wood;
    [SerializeField] private Block closestRoot; //update as built
    public List<Block> connectionsToClosestRoot; //root, then any blocks that lead there 
    private void Awake()
    {
        blockLayer = LayerMask.GetMask("Block");
        if (!gameObject.isStatic)
        {
            FindSupports(); //check down, and all sides 
            //when placed, we are either a root, next to one, or not
            //get hovering block
            if (PlayerPlaceBlocks.Instance.hoveringOverBlock != null)
            { 
                Block hover = PlayerPlaceBlocks.Instance.hoveringOverBlock;
                if (hover != null)
                {
                    closestRoot = hover.closestRoot;
                    connectionsToClosestRoot = new List<Block>(hover.connectionsToClosestRoot); //shallow copy
                    connectionsToClosestRoot.Add(this); 

                }
            } 

            //GetClosestRoot();
            //UpdateNearbyBlocks(); //see if any block stability changes as a result
        }
        else
        {
            closestRoot = this;
        }
    }
    private void Update()
    {
        FindSupports();
    }
    private void OnCollisionEnter(Collision collision)
    {   //if we hit the ground, we can become a root
        if (!supportedFromBelow)
        { 
            CheckSupportFromBelow();
        }
    }
    private void OnDestroy()
    {
        //markedForDestruction = true;
        //RootDisconnectCheck();
    }
    private bool markedForDestruction = false;
    private void RootDisconnectCheck()
    { 
        float length = 1;
        DirCheckDisconnect(Vector3.forward, length);
        DirCheckDisconnect(Vector3.back, length);
        DirCheckDisconnect(Vector3.left, length);
        DirCheckDisconnect(Vector3.right, length);
        DirCheckDisconnect(Vector3.up, length);
    }
    private void DirCheckDisconnect(Vector3 dir, float length)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, length, blockLayer))
        {
            Block block = hit.collider.GetComponent<Block>();
            if (block != null && block.stability > 0)
            {
                bool disconnect = false;
                if (dir == Vector3.up) //broken block is directly below this one
                {
                    disconnect = true;
                }
                else
                { 
                    if (block.closestRoot == null || block.markedForDestruction)
                    {
                        disconnect = true;
                    }
                    else
                    {
                        for (int i = 0; i < block.connectionsToClosestRoot.Count; i++) //before destroyed, marked for destruction.
                        {
                            Block b = block.connectionsToClosestRoot[i];
                            if (b == null || b.markedForDestruction)
                            {
                                disconnect = true;
                            }
                        }
                    }
                } 
                if (disconnect)
                {
                    block.SetStability(0);
                    block.RootDisconnectCheck();
                }
            }
        }
    }
    private void FindSupports()
    {
        if (gameObject.isStatic) return;
        stability = 0;
        float length = 1;

        CheckSupportFromBelow(); 
        CheckSideSupport(Vector3.forward, length);
        CheckSideSupport(Vector3.back, length);
        CheckSideSupport(Vector3.left, length);
        CheckSideSupport(Vector3.right, length);
        CheckStability();
    }
    private void CheckSupportFromBelow()
    {
        float length = .75f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, length, blockLayer))
        {
            Block block = hit.collider.GetComponent<Block>();
            if (block != null)
            {
                BecomeRoot();
                SetStability(block.stability - verticalStabilityPenalty); //inherit stability. taller structures are less stable.
            }
        }
    }
    private void BecomeRoot()
    { 
        supportedFromBelow = true;
        closestRoot = this;
        connectionsToClosestRoot.Clear();
        if (!connectionsToClosestRoot.Contains(this))
        {
            connectionsToClosestRoot.Add(this);
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
                if (supportedFromBelow) //wider base is more stable
                {
                    AddStability(block.stability / 8);
                }
                else
                {
                    AddStability(block.stability - weight);
                }

                if (block.supportedFromBelow)
                {
                    closestRoot = block;
                }
            }
        }
    } 
    private void AddStability(int set)
    {
        stability = Mathf.Clamp(stability + set, 0, maxStability); 
    }
    private void SetStability(int set)
    {
        stability = Mathf.Clamp(set, 0, maxStability); 
    }
    private void CheckStability()
    {
        if (stability > 0 && closestRoot != null)
        {
            body.useGravity = false;
            body.isKinematic = true;
            CleanUpPosition();
        }
        else
        {
            body.useGravity = true;
            body.isKinematic = false;
            ClearRoot();
        }
    }
    private void ClearRoot()
    {
        supportedFromBelow = false;
        closestRoot = null;
        connectionsToClosestRoot.Clear(); 
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
    /* 
    private void UpdateNearbyBlocks()
    {
        float length = 1;
        UpdateSupport(Vector3.up, length);
        UpdateSupport(Vector3.forward, length);
        UpdateSupport(Vector3.back, length);
        UpdateSupport(Vector3.left, length);
        UpdateSupport(Vector3.right, length);
    } 
    private void UpdateSupport(Vector3 dir, float length)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, length, blockLayer))
        {
            Block block = hit.collider.GetComponent<Block>();
            if (block != null)
            {
                block.FindSupports();
                block.UpdateNearbyBlocks();
            }
        }
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
    } */
}
