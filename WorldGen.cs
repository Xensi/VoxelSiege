using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class WorldGen : MonoBehaviour
{
    public int x = 100;
    public int y = 100;
    public int z = 100;
    public GameObject block;
    public List<GameObject> blocks;
    void Start()
    {
        //GenerateWorld();
    }
    private void OnEnable()
    {
        GenerateWorld();
    }
    private void ClearWorld()
    {
        while(blocks.Count > 0)
        {
            DestroyImmediate(blocks[0].gameObject);
            blocks.RemoveAt(0);
        }
    }
    private void GenerateWorld()
    {
        ClearWorld();
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    Vector3 pos = new Vector3(i, j, k);
                    GameObject a = Instantiate(block, pos, Quaternion.identity);
                    blocks.Add(a);
                }
            }
        }
    }
}
