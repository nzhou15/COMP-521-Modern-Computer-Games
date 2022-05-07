using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Generator : MonoBehaviour
{
    public GameObject treePrefab;
    public GameObject garbageCanPrefab;
    public GameObject squirrelPrefab;
    GameObject allTrees;
    GameObject allCans;
    List<GameObject> allSquirrels = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Generator1();
        Generator2();
    }

    void Generator1()
    {
        allTrees = new GameObject();
        allTrees.name = "Trees";

        allCans = new GameObject();
        allCans.name = "Garbage cans";

        while(allCans.transform.childCount < 5)
        {             
            // instantiates 10 trees and 5 garbage cans at random location
            float randX = Random.Range(2f, 48f); 
            float randZ = Random.Range(2f, 28f);

            // relatively sparse distribution, non-overlapping
            Vector3 position = new Vector3(randX, 0f, randZ);
            Collider[] colliders = Physics.OverlapSphere(position, 5f);

            bool valid = true;
            foreach (Collider c in colliders)
            {   
                if(c.transform.name == "Tree(Clone)" || c.transform.name == "Garbage can(Clone)" || c.transform.name == "Player")
                {   
                    valid = false;
                }    
            }

            if(valid)
            { 
                if(allTrees.transform.childCount < 10)
                {
                    GameObject tree = Instantiate(treePrefab, position, transform.rotation);   
                    tree.transform.parent = allTrees.transform;
                }
                else
                {
                    GameObject can = Instantiate(garbageCanPrefab, position, Quaternion.Euler(new Vector3(-90f, 0f, 0f)));   
                    can.transform.parent = allCans.transform;
                }
            }
        }
    }

    void Generator2()
    {   
        List<int> list = new List<int>();
        while(list.Count < 5)
        {
            int rand = Random.Range(0, 10);
            if(!list.Contains(rand))
            {
                list.Add(rand);
            }
        }

        // instantiates 5 squirrels associated with their home tree
        for(int i = 0; i < 5; i++)
        {
            Transform homeTree = allTrees.transform.GetChild(list[i]);

            GameObject squirrel = Instantiate(squirrelPrefab, homeTree.position + new Vector3(1f, 0f, 0f), squirrelPrefab.transform.rotation);
            squirrel.transform.parent = homeTree;
            
            allSquirrels.Add(squirrel);
        }
    }
   
}
