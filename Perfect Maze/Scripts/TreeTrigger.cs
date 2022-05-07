using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeTrigger : MonoBehaviour
{
    public GameObject treePrefab;
    private bool isTriggered = false;
    private GameObject allTrees;

    // a function that triggers by the Player 
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "FPSController" && isTriggered == false)
        {
            allTrees = new GameObject();
            allTrees.name = "Trees";

            GameObject tree; 
            for(int i = 0; i < 10; i++)
            {   
                // instantiates trees at random location
                float randX = Random.Range(5f, 110f); 
                float randZ = Random.Range(10f, 30f);
                tree = Instantiate(treePrefab, new Vector3(randX, -5, transform.position.z + randZ), transform.rotation);   
                tree.transform.parent = allTrees.transform;
            }
            isTriggered = true; 
        }
    }
}
