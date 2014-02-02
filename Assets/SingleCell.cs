/* MazeCraze Game 
 * Created by Cole Fennel
 * 4-17-2013
 * 
 * Description: This game simply generates a maze using a version of Prim's Algorithm, 
 * and gives the user input using WSAD to solve the maze. The game records the users time for solving
 * each maze, and keeps track of levels. 
 * 
 * SingleCell.cs contains the code for keeping track of each individual cell. Each cell Transform
 * will be linked to a class of type SingleCell.
 * 
 * For questions or comments please email fennel.1@osu.edu
 * 
 * */


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SingleCell : MonoBehaviour {
	
	public List<Transform> surroundingCells; //surroundingCells will contain all cells adjacent
	public Vector3 pos; //coordinates
	public int randomNumber; //randomNumber will be used to randomly generate maze
	public int surroundingCellsOpened; //number of surroundingCells accounted for
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
}
