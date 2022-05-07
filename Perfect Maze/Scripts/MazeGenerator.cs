using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell{
    public bool isVisited;
    public GameObject eastWall;
    public GameObject westWall;
    public GameObject southWall;
    public GameObject northWall;
}


public class MazeGenerator : MonoBehaviour
{
    public GameObject wall;     // wall prefab   
    public Transform bridge;
    
    public float wallLength = 6f;
    private int width = 6;
    private int length = 10;
    private Vector3 initialPos;
    private GameObject allWalls;

    private Cell[] cells;
    private int cur = 0;        // the position of current cell
    private int num_cells = 60; 
    private int num_visited_cells = 0; 

    private bool isBuilded = false; // whether or not the Maze starts building
    private int curNeighbour = 0;   // the position of current neighbour
    private List<int> lastCells = new List<int>();  // a list of visited cells
    private int backingUp = 0;

    private int wallToBreak = 0;
    
    private int num_trees = 10;
    private int unhiddenCells = 0;

    public bool isComplete = false; // whether or not the Maze is complete

    void Start()
    {   
        initialPos = new Vector3(bridge.position.x - 15f, bridge.position.y + 1.5f, bridge.position.z - 23.5f);      
        
        allWalls = new GameObject();
        allWalls.name = "Maze Walls"; 
        
        cells = new Cell[60];
        lastCells.Clear();  

        createWalls();
    }

    void Update()
    {
        if(GameObject.Find("Trees")){   
            // tracks the number of existing trees
            int num = GameObject.Find("Trees").transform.childCount;

            // each time a tree is destroyed then reveals one row of maze
            if(num != num_trees)
            {
                num_trees--;
                revealMaze();
            }
        }
    }

    private void revealMaze()
    {    
        for(int i = unhiddenCells; i < width + unhiddenCells; i++){
            if(cells[i].eastWall)
            {
                cells[i].eastWall.SetActive(true);
            }

            if(cells[i].westWall)
            {
                cells[i].westWall.SetActive(true);
            }

            if(cells[i].southWall)
            {
                cells[i].southWall.SetActive(true);
            }

             if(cells[i].northWall)
            {
                cells[i].northWall.SetActive(true);
            }
        }
        unhiddenCells += 6;

        if(unhiddenCells == 60)
        {
            isComplete = true;
        }
    }

    // a function that hides the maze at the beginning of the game
    private void hideMaze(){
        for(int i = 0; i < num_cells; i++){
            cells[i].eastWall.SetActive(false);
            cells[i].westWall.SetActive(false);
            cells[i].northWall.SetActive(false);
            cells[i].southWall.SetActive(false);
        }
    }

    // a function that creates a comoplete maze
    private void createWalls()
    {
        Vector3 pos = initialPos; 
        GameObject temp;

        // along x-axis
        for(int i = 0; i < length; i++)
        {
            for(int j = 0; j <= width; j++)
            {
                pos = new Vector3(initialPos.x + (j * wallLength) - wallLength/2, initialPos.y, initialPos.z + (i * wallLength) - wallLength/2);
                temp = Instantiate(wall, pos, Quaternion.identity);
                temp.transform.parent = allWalls.transform;   // stores all walls under the GameObject "Maze Walls"
            }
        }

        // along y-axis
        for(int i = 0; i <= length; i++)
        {
            for(int j = 0; j < width; j++)
            {
                pos = new Vector3(initialPos.x + (j * wallLength), initialPos.y, initialPos.z + (i * wallLength) - wallLength);
                temp = Instantiate(wall, pos, Quaternion.Euler(0, 90f, 0));
                temp.transform.parent = allWalls.transform;
            }
        }
        createCells();
    }

    // a function that creates maze cells, each contains four walls 
    private void createCells()
    {
        int num = allWalls.transform.childCount;
        GameObject[] arr = new GameObject[num];     // an array of walls

        for(int i = 0; i < num; i++)
        {
            arr[i] = allWalls.transform.GetChild(i).gameObject;
        }

        int eastWallCounter = 0; 
        int colCounter = 0;  // column counter 
        for(int i = 0; i < cells.Length; i++)
        {
            if(colCounter == width)
            {
                eastWallCounter++;
                colCounter = 0; // returns to the first coulumn
            }
            
            // assigns east/west/south/north walls to their maze cell
            cells[i] = new Cell();
            cells[i].eastWall = arr[eastWallCounter];
            cells[i].westWall = arr[eastWallCounter + 1];
            cells[i].southWall = arr[i + (width + 1) * length];
            cells[i].northWall = arr[i + (width + 1) * length + width];

            eastWallCounter++;
            colCounter++;
        }
        createMaze();
    }

    // DFS algorithm
    private void createMaze()
    {
        while (num_visited_cells < num_cells)
        {
            if(isBuilded == false)   // checks if this is the first cell
            {   
                cur = Random.Range(0, num_cells);   // chooses a random cell
                cells[cur].isVisited = true;
                num_visited_cells++;
                isBuilded = true;
            }
            else
            {
                findNeighbours();

                // check whether or not the current cell and neighbour is visited or not
                if(cells[curNeighbour].isVisited == false && cells[cur].isVisited == true)
                {
                    breakWall();

                    cells[curNeighbour].isVisited = true;
                    num_visited_cells++;

                    lastCells.Add(cur);
                    cur = curNeighbour;

                    if(lastCells.Count > 0) // updates the position in the list
                    {
                        backingUp = lastCells.Count - 1;
                    }
                }
            }
        }
        hideMaze();
    }

    // a function that find the neighbours of the current cell
    private void findNeighbours()
    {
        int[] neighbours = new int[4];
        int[] connectingWall = new int[4];

        // checks whether or not the current cell locates in the corner
        int check = (cur + 1) / width - 1;
        check *= width;
        check += width;

        int len = 0;

        // east 
        if(cur > 0 && cur != check)
        {
            if(cells[cur - 1].isVisited == false)
            {
                neighbours[len] = cur - 1;
                connectingWall[len] = 1;
                len++;
            }
        }

        // west
        if(cur < num_cells - 1 && (cur + 1) != check)
        {
            if(cells[cur + 1].isVisited == false)
            {
                neighbours[len] = cur + 1;
                connectingWall[len] = 2;
                len++;
            }
        }

        // south
        if(cur - width >= 0)
        {
            if(cells[cur - width].isVisited == false)
            {
                neighbours[len] = cur - width;
                connectingWall[len] = 3;
                len++;
            }
        }

        // north
        if(cur + width < num_cells)
        {
            if(cells[cur + width].isVisited == false)
            {
                neighbours[len] = cur + width;
                connectingWall[len] = 4;
                len++;
            }
        }

        if(len != 0)    // chooses a random neighbour
        {
            int rand = Random.Range(0, len);
            curNeighbour = neighbours[rand];
            wallToBreak = connectingWall[rand];
        }
        else
        {   
            // if there is no neighbour can be found then goes backwards
            if(backingUp > 0) 
            {
                // the current cell becomes the last cell it visited
                cur = lastCells[backingUp];
                backingUp--;
            }
        }
    }

    private void breakWall(){
        switch(wallToBreak)
        {
            case 1: 
                Destroy(cells[cur].eastWall);
                break;
            case 2: 
                Destroy(cells[cur].westWall);
                break;
            case 3: 
                Destroy(cells[cur].southWall);
                break;
            case 4: 
                Destroy(cells[cur].northWall);
                break;
        }
    }
}
