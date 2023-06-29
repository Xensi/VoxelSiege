using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{ 
    public Structure structure;
    public float weight = 20;
    public bool markedForDeath = false;
    public enum BlockType //different block types don't join together automatically.
    {
        Ground,
        Wood,
        Stone
    }
    [SerializeField] private BlockType type = BlockType.Wood; 
}
