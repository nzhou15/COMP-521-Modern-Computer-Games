using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBehaviour : MonoBehaviour
{
    LineRenderer lineRenderer;

    public List<Vector3> points = new List<Vector3>();
    public float width = 12f;   // mound width
    public float height = 4.5f; // mound height
    
    public List<Vector3> ground = new List<Vector3>();
    public float ground_width = 12f;
    
    public int iteration = 1;   // the complexity of mound

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // set three initial points 
        points.Add(new Vector3(- width/2, 0.1f, 0));
        points.Add(new Vector3(0, height, 0));
        points.Add(new Vector3(width/2, 0.1f, 0));

        // implement using midpoint-bisection
        for (int i = 0; i < iteration; i++)
        {
            for (int j = points.Count - 1; j > 0; j--)  // iterate from the tail of list
            {
                Vector3 prev = points[j - 1];
                Vector3 next = points[j];

                // instert a new point base on the x, y values of previous and next points
                // add a random displacement to the y value
                double rand = ((double)(new System.Random(System.Guid.NewGuid().GetHashCode()).Next(0, 200) - 100) / 100d) * height * System.Math.Pow(0.5, i + 1);
                points.Insert(j, new Vector3((prev.x + next.x) / 2, (prev.y + next.y) / 2 + (float)rand, 0));
            }
        }
        drawGround();
        drawMound();
    }

    // a function adds random points of the rest of ground 
    void drawGround()
    {
        for(int i = 0; i < 10; i++)
        {
            float rand = Random.Range(0f, 0.1f); 
            points.Insert(i, new Vector3(- 16f + i, rand, 0));
            points.Add(new Vector3(width/2 + i + 1, rand, 0));
        }
    }

    // a function draws an uneven, but more-or-less flat terrain with a central mound
    void drawMound()
    {
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        lineRenderer.positionCount = points.Count;
        lineRenderer.useWorldSpace = true;

        // draw all the points in the list
        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }
    }
}
