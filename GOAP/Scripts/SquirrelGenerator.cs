using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquirrelGenerator : MonoBehaviour
{
    public GameObject squirrelPrefab;
    GameObject allSquirrels;
    GameObject allTrees;

    // Start is called before the first frame update
    void Start()
    {
        allSquirrels = new GameObject();
        allSquirrels.name = "Squirrels";

        allTrees = GameObject.Find("Trees");

        while(allSquirrels.transform.childCount < 5)
        {
            Transform homeTree = allTrees.transform.GetChild(Random.Range(0, allSquirrels.transform.childCount - 1));

            GameObject squirrel = Instantiate(squirrelPrefab, homeTree.position + new Vector3(0.5f, 0f, 0f), squirrelPrefab.transform.rotation);
            squirrel.transform.parent = homeTree;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
