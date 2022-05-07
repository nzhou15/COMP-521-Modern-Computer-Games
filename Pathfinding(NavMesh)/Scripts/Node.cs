using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int grid_x; // x position in the grid
    public int grid_y;  // y position in the grid
    
    public Vector3 node_position;  // the position of node 
    public bool isObstacle; // if the node contains an obstacle

    public Node parent;     // the parent node of this node

    public int g_cost;  // the path cost between the start node and the current node
    public int h_cost;  // the cost of the cheapest path
    public int f_cost{ get { return g_cost + h_cost; }  }

    // constructor
    public Node(bool b, Vector3 pos, int x, int y)
    {
        isObstacle = b;    
        node_position = pos; 
        grid_x = x; 
        grid_y = y; 
    }
}
