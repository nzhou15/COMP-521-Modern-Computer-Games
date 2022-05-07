using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TreeBehaviour : MonoBehaviour
{
    public GameObject nutPrefab;
    
    float timer = 2f;
    int num_nuts = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // spawns one nut every 2s and up to a maximum of 5 nuts
        timer -= Time.deltaTime;
        if(timer < 0 && num_nuts < 5)
        {
            InstantiateNuts();
            timer = 2f;
        }
    }

    void InstantiateNuts()
    {
        // selects a random position around the tree
        Vector3 position = Random.insideUnitSphere * 2f;
        position.y = nutPrefab.transform.localScale.y / 2;
        Collider[] colliders = Physics.OverlapSphere(position, nutPrefab.transform.localScale.x / 2 + 0.1f);

        bool valid = true;
        foreach (Collider c in colliders)
        {   
            // non-overlapping
            if(c.transform.name == "Nut(Clone)")
            {   
                valid = false;
            }    
        }
        
        if(valid)
        {
            GameObject nut = Instantiate(nutPrefab, gameObject.transform.position + position, transform.rotation);
            nut.transform.parent = gameObject.transform;
            num_nuts++;
        }
        else
        {
            InstantiateNuts();
        }

    }

}
