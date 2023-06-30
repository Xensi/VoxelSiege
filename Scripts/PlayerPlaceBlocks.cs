using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlaceBlocks : MonoBehaviour
{
    [SerializeField] private GameObject selectedBlock;
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private float armLength = 1;
    public LayerMask blockLayer;
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
        Instance = this;
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
            DestroyBlockHovering();
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
            //Vector3 hover = RoundVector(hoveringOverBlock.transform.position);
            Vector3 hover = hoveringOverBlock.transform.position;
            playerCollisionCheck.transform.position = hover + playerCollisionCheck.transform.TransformDirection(dir);
            playerCollisionCheck.transform.rotation = hoveringOverBlock.transform.rotation;
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
            playerCollisionCheck.transform.rotation = Quaternion.identity;
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * armLength, Color.white);
        }
    }
    private Vector3 RoundVector(Vector3 pos)
    { 
        return new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
    }
    public List<Block> newList;
    public void DestroyTargetBlockUpdateStructures(Block target)
    {
        Structure structure = target.structure;
        if (structure != null)
        {
            int index = structure.blocks.IndexOf(target);
            //Debug.Log(index);
            target.markedForDeath = true;
            Destroy(target.gameObject); 
            if (index >= 0)
            {
                structure.blocks.RemoveAt(index); 
            }
            structure.transform.DetachChildren();

            if (structure.blocks.Count > 0)
            {
                newList = new List<Block>(structure.blocks); //blocks to check
                while (structure.blocks.Count > 1) //only first block will retain structure
                {
                    int end = structure.blocks.Count - 1;
                    Block block = structure.blocks[end];
                    block.structure = null;
                    structure.blocks.RemoveAt(end);
                }
                structure.blocks[0].transform.parent = structure.transform;
                //

                for (int i = 0; i < newList.Count; i++)
                {
                    Block block = newList[i];
                    //Debug.Log(block.gameObject.name);
                    //check in all directions for blocks with structures
                    isolatedCheck = true;
                    CheckBlockDir(Vector3.up, block);
                    CheckBlockDir(Vector3.down, block);
                    CheckBlockDir(Vector3.forward, block);
                    CheckBlockDir(Vector3.back, block);
                    CheckBlockDir(Vector3.left, block);
                    CheckBlockDir(Vector3.right, block);
                    if (isolatedCheck)
                    {
                        if (block.structure == null)
                        {
                            GameObject str = Instantiate(structurePrefab, block.transform.position, Quaternion.identity);
                            Structure strCmp = str.GetComponent<Structure>();
                            block.transform.parent = str.transform;
                            strCmp.blocks.Add(block);
                            block.structure = strCmp;
                            strCmp.CalculateBodyMass();
                            allStructures.Add(strCmp);
                            //Debug.Log("1");
                        }
                        else
                        {
                            block.structure.CalculateBodyMass();
                        }
                    }
                }
            }
            else
            {
                //Destroy(structure.gameObject);
            }
        }
        else
        {

            Destroy(target.gameObject);
            hoveringOverBlock = null;
        }
        ClearEmptyStructures();
        WakeUpAllStructures();
    }
    private void DestroyBlockHovering()
    {
        if (hoveringOverBlock != null)
        {
            DestroyTargetBlockUpdateStructures(hoveringOverBlock);
            hoveringOverBlock = null;
        }
    } 
    private bool isolatedCheck = false; 
    private void CheckBlockDir(Vector3 dir, Block origin) //if we hit a block that is in a structure, return the structure
    {
        float len = 1;
        RaycastHit hit; 
        if (Physics.Raycast(origin.transform.position, origin.transform.TransformDirection(dir), out hit, len, blockLayer))
        {
            Block block = hit.collider.GetComponent<Block>();
            if (block != null && !block.markedForDeath && !block.gameObject.isStatic)
            {
                //Debug.Log("here's " + block.gameObject.name);
                isolatedCheck = false;
                if (origin.structure != null) //join origin
                {
                    if (!origin.structure.blocks.Contains(block))
                        origin.structure.blocks.Add(block);
                    block.transform.parent = origin.structure.transform;
                    if (block.structure != null && block.structure != origin.structure)
                    {
                        //drain it 
                        while (block.structure.blocks.Count > 0)
                        {
                            origin.structure.blocks.Add(block.structure.blocks[0]);
                            block.structure.blocks[0].transform.parent = origin.structure.transform;
                            block.structure.blocks.RemoveAt(0);
                        }  

                        Debug.Log("1");
                        //if (block.structure.gameObject.transform.childCount <= 0) Destroy(block.structure.gameObject);
                    }
                    block.structure = origin.structure;
                    origin.structure.CalculateBodyMass(); 
                    //Debug.Log("2");
                }
                else if (block.structure != null)
                {
                    if (!block.structure.blocks.Contains(origin))
                        block.structure.blocks.Add(origin);
                    origin.transform.parent = block.structure.transform;
                    if (origin.structure != null && origin.structure != block.structure)
                    {
                        while (origin.structure.blocks.Count > 0)
                        {
                            block.structure.blocks.Add(origin.structure.blocks[0]);
                            origin.structure.blocks[0].transform.parent = block.structure.transform;
                            origin.structure.blocks.RemoveAt(0);
                        }

                        //if (block.structure.gameObject.transform.childCount <= 0) Destroy(origin.structure.gameObject);
                    }
                    origin.structure = block.structure;
                    block.structure.CalculateBodyMass(); 
                    //Debug.Log("3");
                }
                else //create structure
                {
                    GameObject structure = Instantiate(structurePrefab, targetPosition, Quaternion.identity);
                    Structure structureComp = structure.GetComponent<Structure>();
                    allStructures.Add(structureComp);
                    block.transform.parent = structure.transform;
                    origin.transform.parent = structure.transform;
                    structureComp.blocks.Add(block);
                    structureComp.blocks.Add(origin);
                    block.structure = structureComp;
                    origin.structure = structureComp;
                    structureComp.CalculateBodyMass(); 
                    //Debug.Log("4");
                }
            }
        }
        
    }
    private int num = 0;
    [SerializeField] private GameObject structurePrefab;
    private void PlaceBlock()
    {
        GameObject blockObj = Instantiate(selectedBlock, targetPosition, Quaternion.identity);
        if (hoveringOverBlock != null)
        {
            blockObj.transform.rotation = hoveringOverBlock.transform.rotation;
        }
        blockObj.name = num.ToString();
        num++;
        Block block = blockObj.GetComponent<Block>();
        //check for adjacent blocks to join with
        //using raycasts
        List<Structure> list = new List<Structure>();
        Structure check = CheckStructureDir(Vector3.forward, block);
        if (!list.Contains(check) && check != null) list.Add(check);
        check = CheckStructureDir(-Vector3.forward, block);
        if (!list.Contains(check) && check != null) list.Add(check);
        check = CheckStructureDir(Vector3.right, block);
        if (!list.Contains(check) && check != null) list.Add(check);
        check = CheckStructureDir(-Vector3.right, block);
        if (!list.Contains(check) && check != null) list.Add(check);
        check = CheckStructureDir(Vector3.up, block);
        if (!list.Contains(check) && check != null) list.Add(check);
        check = CheckStructureDir(-Vector3.up, block);
        if (!list.Contains(check) && check != null) list.Add(check);

        if (list.Count > 0) //at least one structure already exists
        {
            if (list.Count > 1) //if at least 2 structure, then those structures can be merged.
            {
                Structure king = list[0];
                while (list.Count > 1)
                {
                    //get last struct
                    Structure serf = list[list.Count - 1];
                    while (serf.blocks.Count > 0)
                    {
                        king.blocks.Add(serf.blocks[0]);
                        serf.blocks[0].transform.parent = king.transform;
                        serf.blocks.RemoveAt(0);
                    }
                    Destroy(serf.gameObject);
                    list.RemoveAt(list.Count - 1);

                } 
                Structure structure = list[0];
                block.transform.parent = structure.transform;
                structure.blocks.Add(block);
                block.structure = structure;
                structure.CalculateBodyMass();
            }
            else //add block to this structure
            {
                Structure structure = list[0];
                block.transform.parent = structure.transform;
                structure.blocks.Add(block);
                block.structure = structure;
                structure.CalculateBodyMass();
            }
        }
        else //no structure exists
        { 
            GameObject structure = Instantiate(structurePrefab, targetPosition, Quaternion.identity);
            Structure structureComp = structure.GetComponent<Structure>();
            block.transform.parent = structure.transform;
            structureComp.blocks.Add(block);
            block.structure = structureComp;
            structureComp.CalculateBodyMass();
            allStructures.Add(structureComp);
        }
        WakeUpAllStructures();
    }
    public List<Structure> allStructures;
    public void ClearEmptyStructures()
    {
        foreach (Structure item in allStructures)
        {
            if (item != null)
            {
                if (item.transform.childCount <= 0)
                {
                    Destroy(item.gameObject);
                }

            }
        }
    }
    public void WakeUpAllStructures()
    { 
        foreach (Structure item in allStructures)
        {
            if (item != null)
            {
                item.body.WakeUp();
            }
        }
    }
    private Structure CheckStructureDir(Vector3 dir, Block placed) //if we hit a block that is in a structure, return the structure
    { 
        float len = 1;
        RaycastHit hit;
        if (Physics.Raycast(targetPosition, dir, out hit, len, blockLayer))
        {
            Block block = hit.collider.GetComponent<Block>(); 
            return block.structure;
        }
        return null;
    }
}
