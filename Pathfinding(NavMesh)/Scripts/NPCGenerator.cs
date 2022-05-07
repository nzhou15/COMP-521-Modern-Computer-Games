using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGenerator : MonoBehaviour
{
    public GameObject NPCPrefab;
    public int num_NPC;

    public Transform plane1;
    public Transform plane2;
    public Transform plane3;
    public Transform bridge1;
    public Transform bridge2;
    public Transform bridge3;

    GameObject all_NPCs;
    GameObject all_obstalces;
    List<Transform> environment = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        num_NPC = 4;
        all_NPCs = new GameObject();
        all_NPCs.name = "NPCs";

        environment.Add(plane1);
        environment.Add(plane2);
        environment.Add(plane3);
        environment.Add(bridge1);
        environment.Add(bridge2);
        environment.Add(bridge3);

        InitiateNPCs();
    }

    // selects a random plane or bridge
    public Transform ChooseRandomRegion()
    {
        int i = Random.Range(0, environment.Count);
        return environment[i];
    }

    // chooses a random position on the given transform
    public Vector3 ChooseRandomPosition(Transform region)
    {
        float rand1 = Random.Range(-1f + NPCPrefab.transform.localScale.x / 2, 1f - NPCPrefab.transform.localScale.x / 2);
        float rand2 = Random.Range(-4.5f + NPCPrefab.transform.localScale.x / 2, 4.5f - NPCPrefab.transform.localScale.x / 2);

        if(region.transform.name == "Bridge (1)" || region.transform.name == "Bridge (2)" || region.transform.name == "Bridge (3)")
        {
            rand1 = Random.Range(-4f + NPCPrefab.transform.localScale.x / 2, 4f - NPCPrefab.transform.localScale.x / 2);;
            rand2 = 0f;
        }
        return region.transform.position + new Vector3(rand1, 0.1f, rand2);
    }

    // chooses a random destination in any non-obstacle location other than the waiting areas
    public Vector3 SetDestination(Transform region)
    {
        Vector3 position = ChooseRandomPosition(region);
        
        bool isValid = true;
        Collider[] colliders = Physics.OverlapSphere(position, NPCPrefab.transform.localScale.x / 2);

        for(int i = 0; i < colliders.Length; i++)
        {
            // ignores other NPCs
            if(colliders[i].transform.parent.name == "Obstacles" || colliders[i].transform.parent.name == "Transporters")
            {
                isValid = false;
            }
        }

        // if it is a valid position then returns the position, otherwise, choose again
        if(isValid)
        {   
            return position;
        }
        return SetDestination(region);
    }

    void InitiateNPCs()
    {
        List<Color> all_colors = new List<Color>(){ Color.white, Color.red, Color.green, Color.yellow, Color.magenta, 
            Color.blue};

        while(all_NPCs.transform.childCount < num_NPC)
        {
            // spawns NPCs at random non-obstacle space
            Vector3 position = ChooseRandomPosition(ChooseRandomRegion());

            // checks the validity of position 
            bool isValid = true;
            Collider[] colliders = Physics.OverlapSphere(position, NPCPrefab.transform.localScale.y + 1f);
            
            for(int j = 0; j < colliders.Length; j++)
            {
                // no overlapping other NPCs, obstacles or on the waiting areas
                if(colliders[j].transform.parent)
                {
                    if(colliders[j].transform.parent.name == "Obstacles" || colliders[j].transform.parent.name == "Transporters" ||
                    colliders[j].transform.parent.name == "NPCs")
                    {
                        isValid = false;
                    }
                }
            }

            if(isValid)
            {
                GameObject temp = Instantiate(NPCPrefab, position, Quaternion.identity);
                // print("position: " + position);
                
                // assigns different colors 
                temp.GetComponent<Renderer>().material.color = all_colors[ all_NPCs.transform.childCount % all_colors.Count ];
                temp.transform.parent = all_NPCs.transform;
            }
        }     
    }

 
}
