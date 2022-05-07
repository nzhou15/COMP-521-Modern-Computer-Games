using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Failure : MonoBehaviour
{
    public MazeGenerator maze;
    private bool goalSide = false;

    // Update is called once per frame
    void Update()
    {
        float x = GameObject.Find("FPSController").transform.position.x;
        float y = GameObject.Find("FPSController").transform.position.y;
        float z = GameObject.Find("FPSController").transform.position.z;
        // print("x = " + x);
        // print("z = " + z);

        // if the player falls into the canyon itself
        if(y < -10 && (x < 39 || x > 76))
        {
            print("LOSE! YOU FELL IN THE CAYON.");
            Application.Quit();
        }

        // if the player jumps onto an incomplete bridge-maze
        if(maze.isComplete == false && y < -10 && x > 39 && x < 77)
        {
            print("LOSE! YOU JUMPED ONTO AN INCOMPLETE MAZE.");
            Application.Quit();
        }

        if(z > 151)
        {
            print("Goal Side");
            goalSide = true;
        }

        // if the player falls back into the canyon from the goal side
        if(goalSide == true && y < -10)
        {
            print("LOSE! YOU FELL BACK INTO THE CANYON.");
            Application.Quit();
        }   
    }
}
