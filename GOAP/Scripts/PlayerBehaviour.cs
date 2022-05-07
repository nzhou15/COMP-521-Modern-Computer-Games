using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerBehaviour : MonoBehaviour
{
    NavMeshAgent agent;

    public GameObject nutPrefab;
    public GameObject garbagePrefab;

    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.transform.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // toggles the player in/out of a ghost-mode
        if(Input.GetKeyDown("space"))
        {
            // disable the collider so that squirrel cannot detect the player
            gameObject.transform.GetComponent<Collider>().enabled = !gameObject.transform.GetComponent<Collider>().enabled;
        }

        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = new RaycastHit();;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                GameObject o = hit.collider.gameObject;
                print(o.transform.name);
                
                // creates nuts by clicking on an empty spot on the ground
                if(o.transform.name == "Terrain")
                {
                    GameObject nut = Instantiate(nutPrefab, hit.point + new Vector3(0f, 0.25f, 0f), transform.rotation);
                    nut.transform.parent = GameObject.Find("Trees").transform.GetChild(0);
                }
                
                // destroys a nut by clicking on it
                if(o.transform.name == "Nut(Clone)")
                {
                    Destroy(o);
                }

                // toggle the state of a garbage can
                if(o.transform.name == "Garbage can(Clone)")
                {   
                    GarbageCanBehaviour garbage_can = o.GetComponent<GarbageCanBehaviour>();

                    if(garbage_can.state == GarbageCanBehaviour.State.Full)
                    {
                        garbage_can.state = GarbageCanBehaviour.State.Empty;
                        Destroy(garbage_can.transform.GetChild(0).gameObject);
                    }
                    else
                    {
                        garbage_can.state = GarbageCanBehaviour.State.Full;
                        GameObject garbage = Instantiate(garbagePrefab, o.transform.position + new Vector3(0f, 1.9f, 0f), garbagePrefab.transform.rotation);
                        garbage.transform.parent = o.transform;
                    }
                }
            }
        }

        // additional feature: pressing the return-bar to move the player towards a random point on the plane
        if(Input.GetKey(KeyCode.Return))
        {
            float randX = Random.Range(2f, 48f); 
            float randZ = Random.Range(2f, 28f);

            agent.SetDestination(new Vector3(randX, 0f, randZ));
        }

    }
}
