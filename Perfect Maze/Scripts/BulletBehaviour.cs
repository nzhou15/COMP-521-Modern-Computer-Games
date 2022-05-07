using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    void Update()
    {
        if(gameObject){
            gameObject.name = "Bullet";
            
            // disappears when it is too high or in the canyon area
            if(gameObject.transform.position.z > 92 || GameObject.Find("Bullet").transform.position.y > 10)
            {
                print("Projectiles out of bounds");
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {   
        if(other.gameObject.transform.parent.name == "Trees")
        {
            // trees disappear after collisions
            print("Hit " + other.name);       
            Destroy(other.gameObject);    
        }
        Destroy(gameObject); // disappears on contact with other level objects or the boundarys
    }
}
