/* MazeCraze Game 
 * Created by Cole Fennel
 * 4-17-2013
 * 
 * Description: This game simply generates a maze using a version of Prim's Algorithm, 
 * and gives the user input using WSAD to solve the maze. The game records the users time for solving
 * each maze, and keeps track of levels. 
 * 
 * Menu.cs is the script that controls the user interface of the Menu Scene
 * 
 * For questions or comments please email fennel.1@osu.edu
 * 
 * */

using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
	
	public Texture BackgroundTexture;
	protected int number = 0;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI()
	{
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), BackgroundTexture); //draw background image
		
		//Add new Game Button
        if (GUI.Button(new Rect(Screen.width / 2 - 65, Screen.height / 2 - 100, 100, 40), "New Game"))
        {
			PlayerPrefs.DeleteAll(); //Refresh all preferences
			PlayerPrefs.SetInt ("level", 1);
			Application.LoadLevel("MazeGame");
        }
        
		//Add Resume Game Button
        if (GUI.Button(new Rect(Screen.width / 2 - 65, Screen.height / 2 - 50, 100, 40), "Resume Game"))
        {
            Application.LoadLevel("MazeGame");
        }	 
		
		//Add Quit Button
		if (GUI.Button(new Rect(Screen.width / 2 - 65, Screen.height / 2, 100, 40), "Quit"))
        {
            Application.Quit();
        }	 
	}
}
