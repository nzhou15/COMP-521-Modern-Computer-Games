using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsectBehaviour : MonoBehaviour
{
    LineRenderer lineRenderer;
    public GameObject insectPrefab;
    GameObject invisibleWall;
    GameObject allInsects;
    InsectGenerator insectGenerator;
    List<WindBehaviour> air_columns = new List<WindBehaviour>();

    public List<Vector3> insect = new List<Vector3>();
    List<Vector3> prev_pos = new List<Vector3>();   // keep track of the previous positions
    List<Vector3> left_wing = new List<Vector3>();
    List<Vector3> right_wing = new List<Vector3>();

    List<Vector3> antennaes = new List<Vector3>();

    public Vector3 initial_pos = new Vector3();
    Vector3 origin = new Vector3(0f, 0f, 0f);
    Vector3 accelaration = new Vector3();  // ax, ay

    // Start is called before the first frame update
    void Start()
    {        
        lineRenderer = GetComponent<LineRenderer>();
        invisibleWall = GameObject.Find("Invisible Wall");
        allInsects = GameObject.Find("All Insects");
        insectGenerator = GameObject.Find("All Insects").GetComponent<InsectGenerator>();
        
        GameObject rising_air = GameObject.Find("Rising Air");
        for(int i = 0; i < rising_air.transform.childCount; i++)
        {
            air_columns.Add(rising_air.transform.GetChild(i).GetComponent<WindBehaviour>());
        }

        // insects are spawned at random locations left of the mound: [-9, -6], [1, 4]
        float rand_x = Random.Range(-9f, -5.5f);
        float rand_y = Random.Range(2f, 4.5f);
        gameObject.transform.position = new Vector3(rand_x, rand_y, 0);
        initial_pos = gameObject.transform.position;

        // apply an initial random accelerations to gameObject 
        rand_x = Random.Range(-0.05f, 0.05f);
        rand_y = Random.Range(-0.05f, 0.05f);
        accelaration.x = rand_x;
        accelaration.y = rand_y;    // ay is affected by air force

        CreateInsect();
    }

    // Update is called once per frame
    void Update()
    {   
        DrawInsect();
        ApplyAirForce();
        Simulate();

        if(IsOffScreen() || HitsInvisibleWall() || HitsLeftWall() || GroundCollision() || CannonballCollision())
        {
            insectGenerator.CreateNewInsect();
            lineRenderer = new LineRenderer();
            Destroy(gameObject);
        }
    }

    void CreateInsect()
    {
        insect.Add(new Vector3(1, 1, 0));       // body [0]
        insect.Add(new Vector3(0.75f, 1.5f, 0));    // wing [1]
        insect.Add(new Vector3(0.5f, 2, 0));    // wing [2]
        insect.Add(new Vector3(0.25f, 1.5f, 0)); // wing [3]
        insect.Add(new Vector3(0, 1, 0));       // wing [4]
        insect.Add(new Vector3(1, 1, 0));
        insect.Add(new Vector3(1, 0, 0));       // wing [6]
        insect.Add(new Vector3(1.5f, 0.25f, 0)); // wing [7]
        insect.Add(new Vector3(2, 0.5f, 0));    // wing [8]
        insect.Add(new Vector3(1.5f, 0.75f, 0)); // wing [9]
        insect.Add(new Vector3(1, 1, 0));       
        insect.Add(new Vector3(1.5f, 1.25f, 0)); // antennae [11] 
        insect.Add(new Vector3(1, 1, 0));       
        insect.Add(new Vector3(1.25f, 1.5f, 0)); // antennae [13]
        insect.Add(new Vector3(1, 1, 0));

        left_wing.Add(insect[1]);
        left_wing.Add(insect[2]);
        left_wing.Add(insect[3]);
        left_wing.Add(insect[4]);

        right_wing.Add(insect[9]);
        right_wing.Add(insect[8]);
        right_wing.Add(insect[7]);
        right_wing.Add(insect[6]);

        antennaes.Add(insect[11]);
        antennaes.Add(insect[13]);

        // store the original shape of the gameObject
        for (int i = 0; i < insect.Count; i++)
        {
            prev_pos.Add(new Vector3(insect[i].x, insect[i].y, 0));
        }
    }

    void DrawInsect()
    {
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        lineRenderer.positionCount = insect.Count;
        lineRenderer.useWorldSpace = false;
        for (int i = 0; i < insect.Count; i++)
        {
            lineRenderer.SetPosition(i, insect[i]);
        }
    }

    void ApplyAirForce()
    {
        for(int i = 0; i < air_columns.Count; i++)
        {
            // print("air_columns[i].transform.position.x: " + air_columns[i].transform.position.x);
            // print("transform.position.x: " + transform.position.x);
            if(insect[0].x + initial_pos.x <= air_columns[i].transform.position.x + 0.1f && 
            insect[0].x + initial_pos.x > air_columns[i].transform.position.x - 0.1f)
            {   
                accelaration.y += air_columns[i].air_speed;
            }
        }
        origin = new Vector3(accelaration.x + origin.x, accelaration.y + origin.y, 0f);
    }
    
    // verlets integration
    void Simulate()
    {   
        // SIMULATION
        for (int i = 0; i < insect.Count; i++)
        {
            Vector3 v = insect[i] - prev_pos[i];
            prev_pos[i] = insect[i];
            insect[i] += v;
            insect[i] += accelaration * Time.deltaTime;
        }

        // CONSTRAINTS
        for (int i = 0; i < 50; i++)
        {
            ApplyConstraint();
        }
    }

    // a separate .pdf file is included in the submission folder
    void ApplyConstraint()
    {  
        float rand = Random.Range(-0.01f, 0.01f);
        
        // ANTENNAES
        // insect can move its antennaes
        antennaes[0] = new Vector3(antennaes[0].x, antennaes[0].y - rand, 0f);
        antennaes[1] = new Vector3(antennaes[1].x - rand, antennaes[1].y, 0f);
       
        insect[11] = antennaes[0];
        insect[13] = antennaes[1];

        // if the distance between insect body and antennaes is larger that some distance, then return to original shape
        if(Vector3.Distance(insect[11], insect[0]) > 0.6f || Vector3.Distance(insect[13], insect[0]) > 0.6f)
        {
            prev_pos[11] = insect[11];
            insect[11] = insect[0] + new Vector3(0.5f, 0.25f, 0f);
            antennaes[0] = insect[11];
            
            prev_pos[13] = insect[13];
            insect[13] = insect[0] + new Vector3(0.25f, 0.5f, 0f);
            antennaes[1] = insect[13];
        }

        // WINGS
        for(int i = 0; i < 4; i++)
        {
            left_wing[i] = new Vector3(left_wing[i].x - rand, left_wing[i].y, 0f);
            right_wing[i] = new Vector3(right_wing[i].x, right_wing[i].y - rand, 0f);
        }

        for(int i = 1; i < 5; i++)
        {
            insect[i] = left_wing[i - 1];
        }
         for(int i = 6; i < 10; i++)
        {
            insect[i] = right_wing[i - 6];
        }

        if(Vector3.Distance(left_wing[2], insect[0]) > 1.2f || Vector3.Distance(right_wing[2], insect[0]) > 1.2f)
        {
            insect[1] = insect[0] + new Vector3(-0.25f, 0.5f, 0f);
            insect[2] = insect[0] + new Vector3(-0.5f, 1f, 0f);
            insect[3] = insect[0] + new Vector3(-0.75f, 0.5f, 0f);
            insect[4] = insect[0] + new Vector3(-1f, 0f, 0f);

            insect[6] = insect[0] + new Vector3(0f, -1f, 0f);
            insect[7] = insect[0] + new Vector3(0.5f, -0.75f, 0f);
            insect[8] = insect[0] + new Vector3(1f, -0.5f, 0f);
            insect[9] = insect[0] + new Vector3(0.5f, -0.25f, 0f);

             for(int i = 0; i < 4; i++)
            {
                left_wing[i] = insect[i + 1];
                right_wing[i] = insect[i + 6];
            }
        }
        
    }

    // no collision response is required for collisions with ground or cannonballs
    bool GroundCollision()
    {
        for(int i = 0; i < insect.Count; i++)
        {
            if(insect[i].y + initial_pos.y < 0.1f)
                return true;
        }
        return false;
    }

    // a function that calls InsectCollision() from class CannonballBehaviour
    // returns true if there is a collision between cannonballs and insects
    bool CannonballCollision()
    {
        GameObject c = GameObject.Find("Cannonballs");
        for(int i = 0; i < c.transform.childCount; i++){
            CannonballBehaviour temp = c.transform.GetChild(i).GetComponent<CannonballBehaviour>();
            if(temp.InsectCollision())
                return true;
        }
        return false;
    }

    bool IsOffScreen()
    {   
        // insect[1]: the top endpoint
        if(insect[1].y + initial_pos.y > 10f)
        {
            // print("Insect goes offscreen at the top.");
            return true;
        }
        return false;
    }

    bool HitsLeftWall()
    {   
        // insect[2]: the left endpoint
        if(insect[2].x + initial_pos.x < -10f)
        {
            // print("Insect hits the left wall.");
            return true;
        }
        return false;
    }

    bool HitsInvisibleWall()
    {   
        // insect[5]: the right endpoint 
        if(insect[5].x + initial_pos.x > invisibleWall.transform.position.x - 0.1f)
        {
            // print("Insect hits invisible wall.");
            return true;
        }
        return false;
    }
}
