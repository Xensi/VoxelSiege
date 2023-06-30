using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{ 
    public Structure structure;
    public float mass = 20;
    public bool markedForDeath = false;
    public enum BlockType //different block types don't join together automatically.
    {
        Ground,
        Wood,
        Stone
    }
    [SerializeField] private BlockType type = BlockType.Wood;

    public float HP = 100;

    [SerializeField] private BoxCollider boxCollider;

    public void TakeDamage(float damage)
    {
        HP -= damage;
        if (HP <= 0)
        { 
            PlayerPlaceBlocks.Instance.DestroyTargetBlockUpdateStructures(this);
        }
    }
}
