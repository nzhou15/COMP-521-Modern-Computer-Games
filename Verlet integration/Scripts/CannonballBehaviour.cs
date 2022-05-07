using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonballBehaviour : MonoBehaviour
{   
    CannonBehaviour cannon;
    GameObject invisibleWall;
    GameObject wall;
    TerrainBehaviour terrain;

    float vx, vy;
    float radius; // the left/right ends and radius of the gameObject
    float gravity = -0.00098f;
    bool isCollided = false;    // a boolean variable indicates the invisible wall is collided once
    float timer = 10f;      // allow 10f stationary period
    float bounce_timer = 0.5f;    // bouncing time

    // Start is called before the first frame update
    void Start()
    {
        cannon = GameObject.Find("Cannon").GetComponent<CannonBehaviour>();
        float theta = cannon.theta; // barrel angle 
        float initial_v = cannon.velocity;
        
        // set the horizontal and vertical velocities of cannonballs
        vx = -initial_v * Mathf.Cos(theta * Mathf.Deg2Rad); // convert degrees to radians
        vy = initial_v * Mathf.Sin(theta * Mathf.Deg2Rad);

        radius = gameObject.transform.localScale.x/2;

        invisibleWall = GameObject.Find("Invisible Wall");
        
        wall = GameObject.Find("Wall");
        terrain = GameObject.Find("Terrain").GetComponent<TerrainBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {   
        // update the current velocities, v(t) = v(0) + at 
        vy = vy + gravity * 1f;
        gameObject.transform.position = new Vector3(transform.position.x + vx * 1f, transform.position.y + vy * 1f, 0);

        // destroy the gameObject if it is offscreen, or contacts invisible wall from the left, or ends up stationary
        if(IsOffScreen() || IsContactFromLeft() || IsStationary())
        {
            Destroy(gameObject);
        }
        
        // collision detections
        if(WallCollision() || OtherCannonballsCollision())
        {   
            CollisionHandler();
        }

        if(MoundCollision())
        {
            // print("Collides with the central mound.");
            // call MoundCollisionHandler() implicitly in MoundCollision()
        }

        if(GroundCollision())
        {
            GroundCollisionHandler();
        }
    }

    // collision detections  
    bool MoundCollision()
    {   
        List<Vector3> points = terrain.points;
        float width = terrain.width;    // mound width, height
        float height = terrain.height;

         // if gameObject does not reach the mound area, then return false
        if(gameObject.transform.position.x > width/2 || gameObject.transform.position.x < -width/2 ||
        gameObject.transform.position.y > height)
            return false;
        
        // extract the list of mound from the terrain list base on the implementation of class TerrainBehaviour
        List<Vector3> mound = new List<Vector3>();          // (points.Count - 20) points

        int length = points.Count - 20;
        for(int i = 0; i < points.Count; i++)
        {
            if(i < 10 + length && i >= 10)
            {
                mound.Add(points[i]);
            }
        }
        // print("mound.Count: " + mound.Count);
        // print(mound[0].x + ", " + mound[length-1].x);

        float span = width / (mound.Count - 1); // the length between two consecutive points

        // compute the left/right endpoints of the gameObject
        float left_endpoint = gameObject.transform.position.x - radius;
        float right_endpoint = gameObject.transform.position.x + radius;

        // find the curent position of gameObject is on which interval of list mound
        float left_index = Mathf.Floor((left_endpoint + width/2) / span);
        float right_index = Mathf.Ceil((right_endpoint + width/2) / span);
        
        if(left_index <= 0)    // edge cases
        {
            left_index = 0;
            right_index = 1;
        }
        if(right_index >= mound.Count - 1)
        {
            left_index = mound.Count - 2;
            right_index = mound.Count - 1;
        }

        Vector3 a = new Vector3(left_endpoint, transform.position.y, 0f);
        Vector3 b = new Vector3(right_endpoint, transform.position.y, 0f);

        if(IsIntersect(a, b, mound[(int)left_index], mound[(int)right_index]))
        {
            MoundCollisionHandler(mound, (int)left_index);
            return true;
        }
        return false;
    }

    void MoundCollisionHandler(List<Vector3> mound, int i)
    {
        // compute the intersection angle between the line segment and horizontal line
        float alpha = Mathf.Atan2(mound[i].y - mound[i + 1].y,
            mound[i].x - mound[i + 1].x == 0 ? 0.0001f : mound[i].x - mound[i + 1].x);
        
        float rand = Random.Range(0.5f, 0.95f);
        float v0 = Mathf.Sqrt(vx * vx + vy * vy);
        float v1 = v0 * rand;

        // compute the intersection angle between the current velocity and horizontal line
        float beta = Mathf.Atan2(vy, vx == 0 ? 0.0001f : vx);
        vx = v1 * Mathf.Cos(alpha * 2 + beta);
        vy = v1 * Mathf.Sin(alpha * 2 + beta);
    }

    // a function that determines if three points are listed in a counterclockwise order.
    bool IsCounterclockwise(Vector3 A, Vector3 B, Vector3 C)
    {   
        // if the slope of the line AB is less than the slope of the line AC then it is in counterclockwise order.
        return (C.y-A.y) * (B.x-A.x) > (B.y-A.y) * (C.x-A.x);
    }
    
    // a function that checks if line AB and line CD intersects
    bool IsIntersect(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        return IsCounterclockwise(A, C, D) != IsCounterclockwise(B, C, D) && IsCounterclockwise(A, B, C) != IsCounterclockwise(A, B, D);
    }   

    bool GroundCollision()
    {
        // extract the list of left/right grounds from the terrain list
        List<Vector3> points = terrain.points;
        List<Vector3> left_ground = new List<Vector3>();    // 10 points
        List<Vector3> right_ground = new List<Vector3>();   // 10 points

        int length = points.Count - 20;
        for(int i = 0; i < points.Count; i++)
        {
            if(i < 10)
            {
                left_ground.Add(points[i]);
            }
            
            if(i >= 10 + length)
            {
                right_ground.Add(points[i]);
            }    
        }
        // print("right_ground.Count: " + right_ground.Count);
        // print("left_ground.Count: " + left_ground.Count);

        for(int i = 0; i < right_ground.Count; i++)
        {
            if(transform.position.y < right_ground[i].y)
            {
                // print("Collides with ground.");
                return true;
            }        
        }

        for(int i = 0; i < left_ground.Count; i++)
        {
            if(transform.position.y < left_ground[i].y)
            {
                // print("Collides with ground.");
                return true;    
            }
        }
        return false;
    }

    bool WallCollision()
    {
        // the left endpoint of cannonball encounters the right side of wall
        float left_endpoint = gameObject.transform.position.x - radius;
        float right_side = wall.transform.position.x + wall.transform.localScale.x/2;

        if(left_endpoint < right_side)
        {
            // print("Collides with the left wall.");
            return true;
        }
        return false;
    }

    bool OtherCannonballsCollision()
    {
        GameObject allCannonballs = GameObject.Find("Cannonballs");
        for(int i = 0; i < allCannonballs.transform.childCount; i++)
        {   
            // iterate through all the cannonballs and detect collisions between circles
            GameObject other = allCannonballs.transform.GetChild(i).gameObject;
            if(other != gameObject && Mathf.Pow(other.transform.position.x - gameObject.transform.position.x, 2) + Mathf.Pow(other.transform.position.y - gameObject.transform.position.y, 2) <= Mathf.Pow(radius, 2))
            {
                // print("Collides with other cannonballs.");
                return true;
            }    
        }
        return false;
    }

    // collision handlers
    void CollisionHandler()
    {           
        // a coefficient of restitution of between 50% and 95%
        float rand = Random.Range(0.5f, 0.95f);
        vx = -vx * rand;
        vy = vy * rand;
    }

    public bool InsectCollision()
    {
        GameObject allInsects = GameObject.Find("All Insects");

        Vector3 a = new Vector3(transform.position.x - 0.1f, transform.position.y, 0f);
        Vector3 b = new Vector3(transform.position.x + 0.1f, transform.position.y, 0f);

        // iterate through all insects and detect collisions
        for(int i = 0; i < allInsects.transform.childCount; i++)
        {    
            InsectBehaviour aInsect = allInsects.transform.GetChild(i).GetComponent<InsectBehaviour>();
            
            for (int j = 0; j < aInsect.insect.Count - 1; j++)
            {
                if(IsIntersect(aInsect.insect[j] + aInsect.initial_pos, aInsect.insect[j + 1] + aInsect.initial_pos, a, b))
                {
                    // print("Collides with insects.");
                    return true;                    
                }
            }
        }
        return false;
    }

  

   
    // a function that lets cannonballs rebounce upwards after collisions with ground
    void GroundCollisionHandler()
    {
        float rand = Random.Range(0.5f, 0.95f);        
        while(bounce_timer > 0){
            // print("bounce_time: " + bounce_timer);
            gameObject.transform.position = new Vector3(transform.position.x, transform.position.y + 0.02f * rand, 0);
            bounce_timer -= Time.deltaTime;
        }
        vx = 0;
        vy = 0;
    }

    // a function that checks if cannonballs go offscreen
    bool IsOffScreen()
    {
        if(gameObject.transform.position.x > 10.5f || gameObject.transform.position.x < -10.5f 
         || gameObject.transform.position.y > 10f || gameObject.transform.position.y < 0f)
        {
            // print("Cannonball goes offscreen.");
            return true;
        }
        return false;
    }

    bool IsContactFromLeft()
    {    
        // print(6.4f - gameObject.transform.position.x);
        // invisible wall position: (6.5, 10, 0), scale: (0.2, 10, 1)
         if(invisibleWall.transform.position.x - invisibleWall.transform.localScale.x/2 >= gameObject.transform.position.x)
        {
            isCollided = true;
        }

        if(isCollided && invisibleWall.transform.position.x - invisibleWall.transform.localScale.x/2 < gameObject.transform.position.x )
        {
            // print("Cannonball contacts invisible wall from left.");
            return true;
        }

       
        return false;
    }

    bool IsStationary()
    {
        if(vx == 0 || vy == 0)
        {   
            timer -= Time.deltaTime;  
            if(timer < 0f)  // count down the period that cannonballs stop moving
            {
                // print("Cannonball ends up stationary for an extended period.");
                return true;
            }         
        }    
        return false;
    }
}
