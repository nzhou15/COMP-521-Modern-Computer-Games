using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RRTPathFinding : MonoBehaviour
{
    List<Vector2> vertices = new List<Vector2>();
    List<List<Vector2>> edges = new List<List<Vector2>>();

    public float step_size = 0.5f;    
    Vector2 start_node;

    LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // random gaussian generation
    Vector2 GenPointGauss(float dest_x, float dest_z, float std_dev_x, float std_dev_z)
    {
        float rand1 = Random.Range(0.0f, 1.0f);
        float rand2 = Random.Range(0.0f, 1.0f);

        float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

        float x = dest_x + std_dev_x * n;
        float z = dest_z + std_dev_z * n;
                     
        return new Vector2(x, z);
    }

    // a function generates random points
    Vector2 GenPoint()
    {
        float rand1 = Random.Range(-10f, 10f);
        float rand2 = Random.Range(-24f, 24f);
        return new Vector2(rand1, rand2);
    }

    // a functions returns the closest vertex to point
    Vector2 ClosestPointToPoint(List<Vector2> vertices, Vector2 p1)
    {
        float min = float.MaxValue;
        Vector2 v_closest = new Vector2();

        for(int i = 0; i < vertices.Count; i++)
        {
            Vector2 p2 = new Vector2(vertices[i].x, vertices[i].y);
            float dist = Vector2.Distance(p1, p2); 

            if(dist < min)
            {
                min = dist;
                v_closest = vertices[i];
            }
        }
        return v_closest;
    }

    // a function returns the parent of v
    Vector2 GetParent(Vector2 v)
    {   
        foreach (List<Vector2> e in edges)
        {
            if(e[1] == v)
            {
                return e[0];
            }
        }
        return new Vector2();
    }

    // a function returns the path from start node to target node
    List<Vector2> GetPath(Vector2 v)
    {
        List<Vector2> path = new List<Vector2>();
        path.Add(v);

        while(v != start_node)
        {
            Vector2 parent = GetParent(v);
            v = parent;
            path.Add(parent);
        }
        
        List<Vector2> toReturn = new List<Vector2>();
        for(int i = path.Count - 1; i >= 0; i--)
        {
            toReturn.Add(path[i]);
        }
        return toReturn;
    }

    public List<Vector2> RRT_search(Vector2 start, Vector3 dest)
    {
        start_node = start;

        // Vector2 p = GenPoint();
        Vector2 p = GenPointGauss(dest.x, dest.z, 10f, 20f);
        Vector2 v_closest = ClosestPointToPoint(vertices, p);

        // steering
        if(Vector2.Distance(v_closest, p) > step_size)
        {
            float theta = Mathf.Atan2(p.y - v_closest.y, p.x - v_closest.x);
            p.x = v_closest.x + step_size * Mathf.Cos(theta);
            p.y = v_closest.y + step_size * Mathf.Sin(theta);
        }
        
        bool obstacle_free = true;
        if(p.y < transform.position.z - 5f || p.y > transform.position.z + 5f || p.x > transform.position.x +  2.5f
         || p.x < transform.position.x - 2.5f)
        {
            obstacle_free = false;
        }

        Collider[] colliders = Physics.OverlapSphere(new Vector3(p.x, 0.2f, p.y), step_size);
        if(obstacle_free)
        {
            foreach (Collider c in colliders)
            {
                if(c.transform.parent)
                {
                    if(c.transform.parent.name == "Obstacles" || c.transform.parent.name == "Transporter")
                    {     
                        obstacle_free = false;
                        break;
                    }
                }
            }
        }
    
        if(obstacle_free)
        {
            vertices.Add(p);
            edges.Add(new List<Vector2>{v_closest, p});
            
            // print("v_closet: " + v_closest);
            // print("p: " + p);

            DrawEdge(v_closest, p);
        }

        if(Vector2.Distance(p, new Vector2(dest.x, dest.z)) < step_size)
        {   
            print("path found!");
            
            edges.Add(new List<Vector2>{p, new Vector2(dest.x, dest.z)});
            List<Vector2> path = GetPath(p);

            return path;
        }
        return RRT_search(start, dest);
    }

    void DrawEdge(Vector2 p1, Vector2 p2)
    {
        lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;

        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        
        lineRenderer.SetPosition(0, new Vector3(p1.x, 0.1f, p1.y));
        lineRenderer.SetPosition(1, new Vector3(p2.x, 0.1f, p2.y));
    }
}
