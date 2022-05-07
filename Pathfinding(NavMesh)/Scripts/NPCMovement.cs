using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{   
    NPCGenerator generator;
    
    Vector3 dest;   // the position of destination
    Transform dest_plane;   // the plane that destination locates

    Transform cur_plane;    // the current plane of NPC
    int cur_plane_index;
    
    bool use_mid_dest = false;  // if we use a middle destionation

    List<Vector3> path = new List<Vector3>();   // the path from start to dest
    List<AStarPathFinding> all_astar_paths = new List<AStarPathFinding>();
    List<RRTPathFinding> all_rrt_paths = new List<RRTPathFinding>();

    int i = 0;  // the index of vector of path
    float timer = 1f;   // 1000 ms
    int num_path = 0;

    public float speed = 1f;   // the speed of NPC agent
    bool reach_goal = true;

    // two different pathfinding algorithm
    public enum Algorithm { RRT, A_STAR };
    public Algorithm algorithm; 

    // Start is called before the first frame update
    void Start()
    {
        generator = GameObject.Find("Environment").GetComponent<NPCGenerator>();

        GameObject environment = GameObject.Find("Environment");
        for(int i = 0; i < 3; i++)
        {
            all_astar_paths.Add(environment.transform.GetChild(i).GetComponent<AStarPathFinding>());
            all_rrt_paths.Add(environment.transform.GetChild(i).GetComponent<RRTPathFinding>());
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {   
        // sets a random destination if it reaches the goal or at the beginning 
        if(reach_goal)
        {
            dest_plane = generator.ChooseRandomRegion();
            dest = generator.SetDestination(dest_plane);

            reach_goal = false;
        }

        // if NPC is currently on the bridge
        if(OnBridge())
        {   
            Vector3 temp = dest_plane.transform.position; 

            if(DestOnBridge() && !use_mid_dest && CheckUseBridge())
            {
                temp = GameObject.Find("Plane (1)").transform.position;
            }

            // walks in the middle of bridge to prevent falling
            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y,
             cur_plane.transform.position.z);
            
            // moves towards dest_plane
            Vector3 target_pos = new Vector3(temp.x, gameObject.transform.position.y, gameObject.transform.position.z);
            gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, target_pos, speed * Time.deltaTime);
        }
        else    // if NPC is on the plane
        {
            if(DestOnBridge())
            {
                use_mid_dest = true;
            }

            Vector3 temp = dest; 

            // different cases handling
            if(CheckUseBridge() && !DestOnBridge())
            {
                if(dest_plane.name == "Plane (1)")
                {
                    if(cur_plane)
                    {
                        if(cur_plane.name == "Plane (2)")
                            temp = GameObject.Find("Bridge (1)").transform.position;
                        
                        if(cur_plane.name == "Plane (3)")
                        {
                            if(gameObject.transform.localPosition.z > 0f)
                                temp = GameObject.Find("Bridge (2)").transform.position;
                            else
                                temp = GameObject.Find("Bridge (3)").transform.position;
                        }
                    }
                }

                if(dest_plane.name == "Plane (2)")
                {
                    if(cur_plane)
                    {
                        if(cur_plane.name == "Plane (1)")
                            temp = GameObject.Find("Bridge (1)").transform.position;
                        
                        if(cur_plane.name == "Plane (3)")
                        {
                            if(gameObject.transform.localPosition.z < 0f)
                                temp = GameObject.Find("Waiting Area (1)").transform.position;
                            else
                                temp = GameObject.Find("Waiting Area (2)").transform.position;
                        }
                    }
                }
                
                if(dest_plane.name == "Plane (3)")
                {   
                    if(cur_plane)
                    {
                        if(cur_plane.name == "Plane (1)")
                        {
                            if(gameObject.transform.localPosition.z > 0f)
                                temp = GameObject.Find("Bridge (1)").transform.position;
                            else
                                temp = GameObject.Find("Bridge (2)").transform.position;
                        }
                        if(cur_plane.name == "Plane (2)")
                        {
                            if(gameObject.transform.localPosition.z < 0f)
                                temp = GameObject.Find("Waiting Area (3)").transform.position;
                            else
                                temp = GameObject.Find("Waiting Area (4)").transform.position;
                        }
                    }
                }
            }

            // if NPC moves towards transporters and gets close to it, places it in the waiting areas
            if(temp == GameObject.Find("Waiting Area (3)").transform.position || temp == GameObject.Find("Waiting Area (4)").transform.position
             || temp == GameObject.Find("Waiting Area (1)").transform.position || temp == GameObject.Find("Waiting Area (2)").transform.position)
            {
                if(Vector3.Distance(temp, gameObject.transform.localPosition) < 0.5f)
                    gameObject.transform.localPosition = new Vector3(temp.x, 0.2f, temp.z);
            }       

            // gets paths by two different algorithms
            if(algorithm == Algorithm.RRT)
            {
                foreach (Vector2 v in all_rrt_paths[cur_plane_index].RRT_search(gameObject.transform.localPosition, temp))
                {
                    path.Add(new Vector3(v.x, gameObject.transform.localPosition.y, v.y));
                }
            }

            if(algorithm == Algorithm.A_STAR)
            {   
                foreach (Node n in all_astar_paths[cur_plane_index].FindPath(gameObject.transform.localPosition, temp))
                {
                    path.Add(n.node_position);
                }
                
            }
            
            // follows the points on the path
            if(i < path.Count)
            {
                Vector3 target_pos = new Vector3(path[i].x, gameObject.transform.position.y, path[i].z);
                gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, target_pos, speed * Time.deltaTime);

                if(gameObject.transform.localPosition == target_pos)
                {
                    i++;
                }
            }

            if(i == path.Count)
            {
                if(Vector3.Distance(dest, gameObject.transform.localPosition) > 0.05f)
                {
                    gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, dest, speed /2 * Time.deltaTime);
                }
                else
                {                    
                    // pauses for 1000ms when NPC reaches the current destination
                    timer -= Time.deltaTime;
                    if(timer < 0)
                    {   
                        // sets a new random destination
                        reach_goal = true;
                        num_path ++;
                        // print(num_path);
                        i = 0;
                    }
                }
            }
        }
    }

    bool DestOnBridge()
    {
        return (dest_plane.name == "Bridge (1)" || dest_plane.name == "Bridge (2)" || dest_plane.name == "Bridge (3)");
    }

    bool CheckUseBridge()
    {
        if(dest_plane == cur_plane)
            return false; 
        return true;
    }

    bool OnBridge()
    {
        if(cur_plane)
            return (cur_plane.name == "Bridge (1)" || cur_plane.name == "Bridge (2)" || cur_plane.name == "Bridge (3)");
        else 
            return false;
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.transform.parent.name == "Environment")
        {
            cur_plane = other.transform;

            if(other.transform.name == "Plane (1)")
                cur_plane_index = 0;

            if(other.transform.name == "Plane (2)")
                cur_plane_index = 1;

            if(other.transform.name == "Plane (3)")
                cur_plane_index = 2;
        }
        
    }
}
