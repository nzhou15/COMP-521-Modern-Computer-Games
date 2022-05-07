using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesGenerator : MonoBehaviour
{
    public Transform plane1;
    public Transform plane2;
    public Transform plane3;
    public GameObject obstaclePrefab1;
    public GameObject obstaclePrefab2;
    public GameObject obstaclePrefab3;

    // Start is called before the first frame update
    void Start()
    {
        List<GameObject> list = new List<GameObject>();
        list.Add(obstaclePrefab1);
        list.Add(obstaclePrefab2);
        list.Add(obstaclePrefab3);

        List<Transform> all_planes = new List<Transform>();
        all_planes.Add(plane1);
        all_planes.Add(plane2);
        all_planes.Add(plane3);

        GameObject all_obstalces = new GameObject();
        all_obstalces.name = "Obstacles";

        // creates 10 small, randomly placed obstacles
        while(all_obstalces.transform.childCount < 10)
        {
            // selects a random shape
            int i = Random.Range(0, list.Count);
            GameObject obstaclePrefab = list[i];

            // selects a random plane 
            int j = Random.Range(0, all_planes.Count);
            Transform plane = all_planes[j];

            // selects a random increment on position
            float rand1 = Random.Range(-2.2f + obstaclePrefab.transform.localScale.x / 2, 2.2f - obstaclePrefab.transform.localScale.x / 2);
            float rand2 = Random.Range(-4.8f + obstaclePrefab.transform.localScale.x / 2, 4.8f - obstaclePrefab.transform.localScale.x / 2);

            Vector3 position = plane.transform.position + new Vector3(rand1, obstaclePrefab.transform.localScale.y / 2, rand2);

            // checks the validity of position 
            bool isValid = true;
            Collider[] colliders = Physics.OverlapSphere(position, obstaclePrefab.transform.localScale.y);
            
            for(int k = 0; k < colliders.Length; k++)
            {
                // prevents overlapping other obstacles and transporters
                if(colliders[k].transform.parent)
                {
                    if(colliders[k].transform.parent.name == "Obstacles" || colliders[k].transform.parent.name == "Transporters")
                    {
                        isValid = false;
                    }
                }
            }

            // if it is a valid position without overlapping then instantiates an obstacle
            if(isValid)
            {
                GameObject temp = Instantiate(obstaclePrefab, position, Quaternion.Euler(0f, Random.Range(0f, 90f), 0f));
                temp.transform.parent = all_obstalces.transform;
            }
        }
    }
}
