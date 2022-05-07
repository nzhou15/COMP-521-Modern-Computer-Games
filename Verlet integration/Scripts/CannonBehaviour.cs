using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CannonBehaviour : MonoBehaviour
{
    public GameObject cannonball;
    private GameObject allCannonballs;
    public Text text;

    public float theta = 45f;   // barrel angle
    public float velocity = 0.15f; // muzzle velocity of cannonballs

    void Start()
    {
        allCannonballs = new GameObject();
        allCannonballs.name = "Cannonballs";
    }

    void Update()
    {   
        // present the current muzzle velocity and barrel angle
        text.text = "muzzle velocity = " + velocity;
        text.text += "\nbarrel angle = " + theta;

        // pressing the spacebar fires the cannon
        if (Input.GetKeyDown(KeyCode.Space))
        {  
            GameObject c = Fire();
            c.transform.parent = allCannonballs.transform;
        }

        // increasing with the up-arrow
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (theta >= 90f) return;
            gameObject.transform.Rotate(0, 0, -1f);
            theta += 1f;
        }

        // decreasing by a down-arrow press
         if (Input.GetKey(KeyCode.DownArrow))
        {
            if (theta <= 0f) return;
            gameObject.transform.Rotate(0, 0, 1f);
            theta -= 1f;
        }

        // muzzle velocity is increased/decreased by left/right arrows
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if(velocity > 0.4f) return;
            velocity += 0.05f;
            // print("LeftArrow: v = " + velocity);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            if(velocity <= 0.1f) return;
            velocity -= 0.05f;
            // print("RightArrow: v = " + velocity);
        }
    }

    GameObject Fire()
    {   
        // instantiate a cannonball game object
        return Instantiate(cannonball, transform.position, transform.rotation);
    }
}
