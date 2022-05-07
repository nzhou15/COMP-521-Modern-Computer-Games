using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScript : MonoBehaviour
{
    public LayerMask obstacle_layer;    // the layer of obstacles
    public Vector2 size;    // the size of whole grid
    public float node_radius;   // the radius of each node
    public float gap;   // the distance between nodes

    Node[,] node_array;     // a 2d array of nodes 
    public List<Node> path; // the path on this grid
    
    float node_diameter;
    int unit_grid_x, unit_grid_y;   // the unit size of the grid in array.
    bool created = false;   

    void Start()
    {   
        // computes the size of the graph in array units
        node_diameter = node_radius * 2;
        unit_grid_x = Mathf.RoundToInt(size.x / node_diameter); 
        unit_grid_y = Mathf.RoundToInt(size.y / node_diameter);
    }

    void Update()
    {
        if(!created)
        {
            CreateGrid();
        }
    }

    void CreateGrid()
    {
        node_array = new Node[unit_grid_x, unit_grid_y];

        // gets the position of the bottom left cornor of the grid
        Vector3 bottomLeft = transform.position - Vector3.right * size.x / 2 - Vector3.forward * size.y / 2;
        
        for (int x = 0; x < unit_grid_x; x++)
        {
            for (int y = 0; y < unit_grid_y; y++)
            {
                Vector3 pos = bottomLeft + Vector3.right * (x * node_diameter + node_radius) + Vector3.forward * (y * node_diameter + node_radius);
                bool isObstacle = true;
                
                if (Physics.CheckSphere(pos, node_radius, obstacle_layer))
                {
                    isObstacle = false; // object is not an obstacle
                }

                node_array[x, y] = new Node(isObstacle, pos, x, y);
            }
        }
        created = true;
    }

    // a function gets the neighboring nodes of the given node.
    public List<Node> GetNeighboringNodes(Node n)
    {
        List<Node> list = new List<Node>();
        int x_dir, y_dir;

        // checks the right side of the current node.
        x_dir = n.grid_x + 1;
        y_dir = n.grid_y;
        if (x_dir >= 0 && x_dir < unit_grid_x)
        {
            if (y_dir >= 0 && y_dir < unit_grid_y)
            {
                list.Add(node_array[x_dir, y_dir]);
            }
        }

        // checks the left side of the current node.
        x_dir = n.grid_x - 1;
        y_dir = n.grid_y;
        if (x_dir >= 0 && x_dir < unit_grid_x)
        {
            if (y_dir >= 0 && y_dir < unit_grid_y)
            {
                list.Add(node_array[x_dir, y_dir]);
            }
        }

        // checks the top side of the current node.
        x_dir = n.grid_x;
        y_dir = n.grid_y + 1;
        if (x_dir >= 0 && x_dir < unit_grid_x)
        {
            if (y_dir >= 0 && y_dir < unit_grid_y)
            {
                list.Add(node_array[x_dir, y_dir]);
            }
        }

        // checks the bottom side of the current node.
        x_dir = n.grid_x;
        y_dir = n.grid_y - 1;
        if (x_dir >= 0 && x_dir < unit_grid_x)
        {
            if (y_dir >= 0 && y_dir < unit_grid_y)
            {
                list.Add(node_array[x_dir, y_dir]);
            }
        }
        return list;
    }

    // a function gets the closest node to the given position
    public Node GetNode(Vector3 position)
    {
        float x = ((position.x + size.x / 2) / size.x);
        float y = ((position.z + size.y / 2) / size.y);

        x = Mathf.Clamp01(x);
        y = Mathf.Clamp01(y);

        int i = Mathf.RoundToInt((unit_grid_x - 1) * x);
        int j = Mathf.RoundToInt((unit_grid_y - 1) * y);

        return node_array[i, j];
    }


    // a function draws the wireframe
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 1, size.y));

        if (node_array != null) 
        {
            foreach (Node n in node_array)
            {
                if (n.isObstacle)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.yellow;
                }

                if (path != null)
                {
                    if (path.Contains(n))
                    {
                        Gizmos.color = Color.red;
                    }
                }
                Gizmos.DrawCube(n.node_position, Vector3.one * (node_diameter - gap));
            }
        }
    }
}
