using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlaceBlocks : MonoBehaviour
{
    [SerializeField] private GameObject selectedBlock;
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private float armLength = 1;
    LayerMask blockLayer;
    public Block hoveringOverBlock;
    [SerializeField] private BoxCollider playerCollisionCheck;
    [SerializeField] private MeshRenderer collisionCheckRenderer;
    [SerializeField] private Material valid;
    [SerializeField] private Material invalid;
    [SerializeField] private CharacterController player;
    [SerializeField] private bool canPlace = true;

    public static PlayerPlaceBlocks Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        blockLayer = LayerMask.GetMask("Block");
    }
    void Update()
    {
        GetTargetPosition();
        UpdateCollisionChecker();
        if (Input.GetMouseButtonDown(0))
        { //on left click destroy
            DestroyBlock();
        }
        if (Input.GetMouseButtonDown(1))
        { //on right click place
            if (canPlace)
            { 
                PlaceBlock();
            }
        }
    }
    private void UpdateCollisionChecker()
    {
        if (canPlace)
        {
            collisionCheckRenderer.material = valid;
        }
        else
        {
            collisionCheckRenderer.material = invalid;
        }
    }
    private void GetTargetPosition()
    { 
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, armLength, blockLayer))
        { 
            hoveringOverBlock = hit.collider.gameObject.GetComponent<Block>(); 
            Vector3 dir = hit.point - hoveringOverBlock.transform.position; 
            float a = Mathf.Abs(dir.x);
            float b = Mathf.Abs(dir.y);
            float c = Mathf.Abs(dir.z);

            bool aG = false;
            bool bG = false; 
            if (a > b)
            {
                if (a > c)
                {
                    aG = true;
                } 
            }
            else
            {
                if (b > c)
                {
                    bG = true;
                } 
            }
            
            if (aG)
            {
                dir.y = 0;
                dir.z = 0;
            }
            else if (bG)
            {
                dir.x = 0;
                dir.z = 0;
            }
            else
            {
                dir.x = 0;
                dir.y = 0;

            }
            dir.Normalize();
            Vector3 hover = RoundVector(hoveringOverBlock.transform.position);
            playerCollisionCheck.transform.position = hover + dir;
            targetPosition = playerCollisionCheck.transform.position;
            if (playerCollisionCheck.bounds.Intersects(player.bounds))
            {
                canPlace = false;
            }
            else
            {
                canPlace = true;
            }
        }
        else
        {
            canPlace = true;
            Vector3 pos = transform.position + transform.TransformDirection(Vector3.forward) * armLength;
            Vector3 newPos = RoundVector(pos);
            targetPosition = newPos;
            hoveringOverBlock = null;
            playerCollisionCheck.transform.position = newPos;
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * armLength, Color.white);
        }
    }
    private Vector3 RoundVector(Vector3 pos)
    { 
        return new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
    }
    private void DestroyBlock()
    {
        if (hoveringOverBlock != null)
        {
            //hoveringOverBlock.TakeDamage(100);
            /*Rigidbody body = hoveringOverBlock.GetComponent<Rigidbody>();
            if (body != null)
            {
                Vector3 dir = hoveringOverBlock.transform.position - transform.position;
                dir.y = 0;
                dir.Normalize();
                body.AddForceAtPosition(dir * 1000, transform.position, ForceMode.Impulse);
            }*/
            Destroy(hoveringOverBlock.gameObject);
            hoveringOverBlock = null;
        }
    }
    private int num = 0;
    private void PlaceBlock()
    {
        if (hoveringOverBlock != null)
        {
            hoveringOverBlock.transform.position = RoundVector(hoveringOverBlock.transform.position);
            hoveringOverBlock.transform.rotation = Quaternion.identity; //reset rotation of whatever we're looking at
        }
        GameObject block = Instantiate(selectedBlock, targetPosition, Quaternion.identity);
        block.name = num.ToString();
        num++;
        //check for adjacent blocks to join with

    }
}
