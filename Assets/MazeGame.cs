/* MazeCraze Game 
 * Created by Cole Fennel
 * 4-17-2013
 * 
 * Description: This game simply generates a maze using a version of Prim's Algorithm, 
 * and gives the user input using WSAD to solve the maze. The game records the users time for solving
 * each maze, and keeps track of levels. 
 * 
 * MazeGame.cs contains the bulk of the code. This file generates the gameBoard, displays stats, 
 * and handles user input. 
 * 
 * For questions or comments please email fennel.1@osu.edu
 * 
 * */

using UnityEngine; 
using System.Collections;
using System.Collections.Generic;

public class MazeGame : MonoBehaviour {
	
	public Transform material; //cell type
	public Vector3 size; //size of maze to generate, set in explorer
	private Transform[,] gameBoard; //actual grid containing maze
	private List<Transform> generatedCells; //will be used in Prim's algorithm to record cells which have been sorted
	private List<List<Transform>> remainingCells; //10 lists inside remainingCells, each one corresponding to a value between 1 and 10
	private int xPos; //current x position of player square
	private int zPos; //current z position of player square
	private int level; //current level of game
	private float startTime; //time maze was generated
	private float endTime; //time maze was solved

	
	// Use this for initialization	
	/* Start will set level variable, set start time, and create gameboard */
	void Start () {
		if(PlayerPrefs.HasKey("level")) //if level exists in PlayerPrefs, load level
		{
			level = PlayerPrefs.GetInt("level");
		}
			else //create level property in PlayerPrefs to level 1
		{
			PlayerPrefs.SetInt ("level",1);
			level = 1;
		}
		CreateMaze();
		startTime = Time.time;
	}
	
	/*CreateMaze calls all the other functions responsible for creating the maze */
	
	void CreateMaze(){
		InitializeGrid();
	    AdjustCamera();
		Sort();
		SetStart();
		IterateMaze();
	}
	
	/*Adjust Camera will simply put the camera in a place that can easily view
	 the maze */
	void AdjustCamera()
	{
		//Center the camera
		Camera.mainCamera.transform.position = gameBoard[(int)(size.x/4f),(int)(size.z/2f)].position + Vector3.up;
		//Adjust size
		Camera.mainCamera.orthographicSize = Mathf.Max(size.x * 3/5, size.z * 3/5); //max of x or z (with constant)
	}
	
	/* InitializeGrid simply makes cells to fill the Game Board 
	  and assigns random values to each cell which are used later in maze generation */
	
	void InitializeGrid()
	{
		gameBoard = new Transform[(int)size.x,(int)size.z];
	
		for(int x = 0; x < size.x; x++){ //for each column
			for(int z = 0; z < size.z; z++){ //for each row
				
				//Create new cell at correct position
				Transform cell = (Transform)Instantiate(material, new Vector3(x, 0, z), Quaternion.identity);
			    cell.name = "(" + x + "," + z + ")";
				cell.parent = transform; //parent is Maze Grid Object
				cell.GetComponent<SingleCell>().pos = new Vector3(x, 0, z); //set pos in script
				int value = Random.Range(0,10); //assign a random number
			    cell.GetComponent<SingleCell>().randomNumber = value; //store random number
				gameBoard[x,z] = cell; //add to gameBoard variable to keep track of 
			}
		}
	}
	
	
	/*Sort will add each adjacent cell to the surroundingCells list
	  * and will sort surroundingCells using the randomNumber property */
	
	void Sort(){
		foreach (Transform cell in gameBoard) //for all cells
		{
				SingleCell cellScript = cell.GetComponent<SingleCell>(); //script object of cell
				int x = (int)cell.position.x;
				int z = (int)cell.position.z;

				if(x - 1 >= 0){ //Add left cell
					cellScript.surroundingCells.Add(gameBoard[x-1,z]);
				}
				if(x + 1 < size.x){ //Add right cell
					cellScript.surroundingCells.Add(gameBoard[x+1,z]);
				}
				if(z - 1 >= 0){ //Add below cell
					cellScript.surroundingCells.Add(gameBoard[x,z-1]);
				}
				if(z + 1 < size.z){ //Add above cell
					cellScript.surroundingCells.Add(gameBoard[x,z+1]);
				}
				cellScript.surroundingCells.Sort(SortByRandomValues); //sort by least random value
		}
	}
	
	/* SortByRandomValues is used to return the lowest randomNumber
	 * when passed two Transforms with a randomNumber property */
	
	int SortByRandomValues(Transform inputA, Transform inputB){
		int a = inputA.GetComponent<SingleCell>().randomNumber; 
		int b = inputB.GetComponent<SingleCell>().randomNumber;
		return a.CompareTo(b); //return lowest
	}
	
	/* SetStart will initialize generatedCells and remainingCells
	and set the starting point on the game board by coloring it cyan */
	
	void SetStart(){
		generatedCells = new List<Transform>(); //Initialize generatedCells
		
		remainingCells = new List<List<Transform>>(); //Initialize remainingCells
		for(int i = 0; i < 10; i++){
			remainingCells.Add(new List<Transform>());	//Add 10 empty Lists
		}
		gameBoard[0,0].renderer.material.color = Color.cyan; //set start cell as cyan
		AddToLists(gameBoard[0,0]); // add first cell
		
		//set global variables
		xPos = 0;
		zPos = 0;
	}
	
	/* AddToLists will add the passed cell to both global lists, generatedCells and remainingCells. It will 
	   check to make sure no cell is added twice to a list */
	
	void AddToLists(Transform cell){
		generatedCells.Add(cell); //Add to generatedCells
		
		//Add all adjacent cells
		foreach(Transform surroundingCell in cell.GetComponent<SingleCell>().surroundingCells){
			surroundingCell.GetComponent<SingleCell>().surroundingCellsOpened++; 
			if(!generatedCells.Contains(surroundingCell) && !(remainingCells[surroundingCell.GetComponent<SingleCell>().randomNumber].Contains(surroundingCell))){
				remainingCells[surroundingCell.GetComponent<SingleCell>().randomNumber].Add(surroundingCell); //Add to correct list depending on randomNumber
			}
		}
	}
	
	/* SetEnd will add the end point to the game board, by coloring the cell red */
	
	void SetEnd (int index)
	{
		generatedCells[index].renderer.material.color = Color.red;
	}
	
	/* IterateMaze implements Prim's Algorithm. It recursively will look for adjacent cells,
	   and change their color based on where they belong in the maze. For each cell, it will
	   mark at most two of them as a path. It also calls SetEnd to mark the ending cell. */
	
	void IterateMaze(){
		Transform cell;
		int i;
		
		do{			
			for(i = 0; i < 10; i++){//for all ten lists, starting with the least randomNumber (1)
				//find the first list that is not empty
				if(remainingCells[i].Count > 0){ 
					break;
				}
			}
			
			if (i == 10){ //if all lists are empty, done generating
				SetEnd(generatedCells.Count-1); //mark the ending cell
				return;
			}
			
			cell = remainingCells[i][0]; 
			remainingCells[i].Remove(cell); //Remove cell since it is accounted for
		}while(cell.GetComponent<SingleCell>().surroundingCellsOpened >= 2); 
		//2 is the max number of adjacent cells it should open to create a maze
		
		//Add cell to maze as general path
		cell.renderer.material.color = Color.white;
		AddToLists(cell);
		
		Invoke("IterateMaze", 0); //Recursion, use Invoke so that the maze creation is displayed
		//IterateMaze(); //Can be used to hide maze creation
	}
	
		// Update is called once per frame
	/* Update will handle all user input */
	
	void Update(){
		if(Input.GetKeyDown(KeyCode.N)){ //If N is input, refresh / get new board
			Application.LoadLevel ("MazeGame");	
		}
		else if(Input.GetKeyDown(KeyCode.M)){ //If M is input, return to menu
			Application.LoadLevel ("Menu");	
		}
		else if (Input.GetKeyUp(KeyCode.W)) //If W is input, move player up
        {
            if(zPos < size.z - 1) //handle array boundaries
			{
			 if(gameBoard[xPos,zPos+1].renderer.material.color == Color.white) //if not wall
				{
					gameBoard[xPos,zPos].renderer.material.color = Color.white; //make old space white
					zPos = zPos + 1;
					gameBoard[xPos,zPos].renderer.material.color = Color.cyan; //make current space cyan
				}
				else if(gameBoard[xPos,zPos+1].renderer.material.color == Color.red)//if reached finish
				{
					LevelWon();
				}
			}
        }
        else if (Input.GetKeyUp(KeyCode.A)) //If A is input, move player left
        {
          if(xPos > 0) //handle array boundaries
			{
			 if(gameBoard[xPos-1,zPos].renderer.material.color == Color.white) //if not wall
				{
					gameBoard[xPos,zPos].renderer.material.color = Color.white; //make old space white
					xPos = xPos - 1;
					gameBoard[xPos,zPos].renderer.material.color = Color.cyan; //make new space cyan
				}
				else if(gameBoard[xPos-1,zPos].renderer.material.color == Color.red)//if reached finish
				{
					LevelWon ();
				}
			}
        }
        else if (Input.GetKeyUp(KeyCode.S)) //if S is input, move player down
        {
          if(zPos >  0) //handle array boundaries
			{
			 if(gameBoard[xPos,zPos-1].renderer.material.color == Color.white) //if not wall
				{
					gameBoard[xPos,zPos].renderer.material.color = Color.white; //make old space white
					zPos = zPos - 1;
					gameBoard[xPos,zPos].renderer.material.color = Color.cyan; //make new space cyan
				}
				else if(gameBoard[xPos,zPos-1].renderer.material.color == Color.red)//if reached finish
				{
					LevelWon();
				}
			}
        }
        else if (Input.GetKeyUp(KeyCode.D)) //if D is input, move player right
        {
           if(xPos < size.x - 1) //handle array boundaries
			{
			 if(gameBoard[xPos+1,zPos].renderer.material.color == Color.white) //if not wall
				{
					gameBoard[xPos,zPos].renderer.material.color = Color.white; //make old space white
					xPos = xPos + 1;
					gameBoard[xPos,zPos].renderer.material.color = Color.cyan; //make new space cyan
				}
				else if(gameBoard[xPos+1,zPos].renderer.material.color == Color.red)//if reached finish
				{
					LevelWon ();
				}
			}
        }
	}
	
/* LevelWon() will calculate the time taken for solving the level,
   record the appropriate times, and load the new level */
	
	void LevelWon()
	{
		float timeTaken;
		float totalTime;
		float fastestTime;
		
		endTime = Time.time;
		timeTaken = endTime - startTime; //calculate level time
		
		
		if(PlayerPrefs.HasKey("totalTime")) //if not level 1
		{
			totalTime = PlayerPrefs.GetFloat("totalTime");
			totalTime += timeTaken;
			PlayerPrefs.SetFloat ("totalTime",totalTime);
			PlayerPrefs.SetFloat ("averageTime",totalTime / level); //calculate new avg. time
		}
		else //if level 1
		{
			PlayerPrefs.SetFloat ("totalTime",timeTaken);
			PlayerPrefs.SetFloat ("averageTime",timeTaken);
		}
		if(PlayerPrefs.HasKey("fastestTime")) //if not level 1
		{
			fastestTime = PlayerPrefs.GetFloat("fastestTime");
			if(fastestTime > timeTaken) //if faster than fastest time
			{
				PlayerPrefs.SetFloat ("fastestTime",timeTaken); //new fastest time
			}
		}
		else //if level 1
		{
			PlayerPrefs.SetFloat ("fastestTime",timeTaken);
		}
		
		level += 1;
		PlayerPrefs.SetInt ("level",level); //set to new level
		Application.LoadLevel ("MazeGame"); //load new level
	}

	
	/* OnGUI will put helpful information on the display screen including
	  level number, statistics, records, and more. */
	
		void OnGUI()
	{
		
		//Display Level and Timer
	    GUI.Box(new Rect(25, 10, 150, 25), "LEVEL " + level);
		GUI.Box(new Rect(25, 35, 150, 25), "Level Time: " + (Time.time - startTime).ToString("f1"));
		
		//Display Controls
		GUI.Label(new Rect(25, 70, 250, 25), "CONTROLS:");
        GUI.Label(new Rect(25, 85, 250, 25), "WASD = Move");
		GUI.Label(new Rect(25, 100, 250, 25), "N = New Puzzle");
		GUI.Label(new Rect(25, 115, 250, 25), "M = Main Menu");
		
		//Display Colors / How to Play
        GUI.Label(new Rect(25, 155, 250, 25), "COLORS:");
		GUI.Label(new Rect(25, 170, 250, 25), "Cyan Box = Player");
		GUI.Label(new Rect(25, 185, 250, 25), "Red Box = Finish Line");
		
		//Display level stats
		GUI.Label(new Rect(25, 225, 250, 25), "RECORDS:");
		if(PlayerPrefs.HasKey("averageTime"))
			GUI.Label(new Rect(25, 240, 250, 25), "Average Time: " + PlayerPrefs.GetFloat("averageTime").ToString("f1"));
		else //only occurs on level 1
			GUI.Label(new Rect(25, 240, 250, 25), "N/A");
		if(PlayerPrefs.HasKey("fastestTime"))
			GUI.Label(new Rect(25, 255, 250, 25), "Fastest Time: " + PlayerPrefs.GetFloat("fastestTime").ToString ("f1"));
		else //only occurs on level 1
			GUI.Label(new Rect(25, 255, 250, 25), "N/A");
			
	}
}
