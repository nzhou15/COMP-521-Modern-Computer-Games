using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinding : MonoBehaviour
{
    GridScript grid;

    void Start()
    {   
        grid = GetComponent<GridScript>();
    }

    public List<Node> FindPath(Vector3 start_pos, Vector3 target_pos)
    {   
        Node start_node = grid.GetNode(start_pos);
        Node target_node = grid.GetNode(target_pos);

        // the list of nodes for the open list
        List<Node> openlist = new List<Node>();

        // the hashset of nodes for the closed list
        HashSet<Node> closedlist = new HashSet<Node>();

        openlist.Add(start_node);

        while(openlist.Count > 0)
        {
            Node cur_node = openlist[0];

            // loops through the open list starting from the second object
            for(int i = 1; i < openlist.Count; i++)
            {
                // if the f cost of that object is less than or equal to the f cost of the current node
                if (openlist[i].f_cost < cur_node.f_cost || openlist[i].f_cost == cur_node.f_cost && openlist[i].h_cost < cur_node.h_cost)//If the f cost of that object is less than or equal to the f cost of the current node
                {
                    cur_node = openlist[i];
                }
            }
            openlist.Remove(cur_node);
            closedlist.Add(cur_node);

            if (cur_node == target_node)
            {
                // gets the final path
                GetPath(start_node, target_node);
            }

            // iterates through each neighbor of the current node
            foreach (Node n in grid.GetNeighboringNodes(cur_node))
            {
                if (!n.isObstacle || closedlist.Contains(n))
                {
                    continue;
                }
                // computes the f-cost of that neighbor
                int cost = cur_node.g_cost + GetManhattenDistance(cur_node, n);

                if (cost < n.g_cost || !openlist.Contains(n))
                {
                    n.g_cost = cost;
                    n.h_cost = GetManhattenDistance(n, target_node);
                    n.parent = cur_node;

                    if(!openlist.Contains(n))
                    {
                        openlist.Add(n);
                    }
                }
            }
        }
        return grid.path;
    }

    // a function returns the path from start node to target node
    void GetPath(Node start_node, Node end_node)
    {
        List<Node> path = new List<Node>();
        Node cur_node = end_node;

        while(cur_node != start_node)
        {
            path.Add(cur_node);
            cur_node = cur_node.parent;
        }

        path.Reverse();
        grid.path = path;
    }

    // a function computes the manhatten distance
    int GetManhattenDistance(Node a, Node b)
    {
        int x = Mathf.Abs(a.grid_x - b.grid_x);
        int y = Mathf.Abs(a.grid_y - b.grid_y);

        return x + y;
    }
}
