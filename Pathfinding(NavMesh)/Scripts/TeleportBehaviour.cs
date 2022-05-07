using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBehaviour : MonoBehaviour
{
    List<GameObject> collided_objects = new List<GameObject>();
    List<GameObject> teleported_objects = new List<GameObject>();

    public GameObject NPCPrefab;
    float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int num = 0;

        // selects at most 3 waiting NPCs from the waiting area
        if(collided_objects.Count >= 3)
        {
            num = 3;
        }
        else
        {
            num = collided_objects.Count;
        }

        for(int i = 0; i < num; i++)
        {
            GameObject head = collided_objects[0];
            head.transform.position = gameObject.transform.position;
            collided_objects.Remove(head);

            Teleport(head);
        }

        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            for(int i = 0; i < teleported_objects.Count; i++)
            {
                GameObject o = teleported_objects[i];
                o.SetActive(true);

                // exits the transporter immediately
                o.transform.position = o.transform.position + new Vector3(1f, 0f, 0f);
                o.GetComponent<Collider>().isTrigger = false;
                teleported_objects.Remove(o);
            }
            timer = 0f;
        }
    }

    void Teleport(GameObject o)
    {
        Vector3 position = new Vector3();

        // from upper level to lower level
        if(o.transform.position.y > 2f) 
        {
            // respawns NPCs in the waiting area of the lower level or vise versa
            position = o.transform.position + new Vector3(2.5f, -2f, 0f);
        }
        else    // from lower level to upper level
        {
            position = o.transform.position + new Vector3(-1.5f, 2.05f, 0f);
        }

        o.SetActive(false);
        o.transform.position = position;
        teleported_objects.Add(o);
        o.GetComponent<Collider>().isTrigger = true;

    }

    void OnCollisionEnter(Collision other)
    {
        // waiting area holds at least 3 NPCs
        if(other.transform.parent.name == "NPCs")
        {   
            if(!teleported_objects.Contains(other.gameObject))
            {
                collided_objects.Add(other.gameObject);
                print("teleporting..." + other.transform.position);
            }
        }
    }
}
