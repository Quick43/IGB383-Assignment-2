using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	public GameObject mainMenu;

	public GameObject instructions;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void showInstructions(){
		mainMenu.SetActive (false);
		instructions.SetActive (true);
	}

	public void showMain(){
		mainMenu.SetActive (true);
		instructions.SetActive (false);
	}

	public void StartGame(){
		SceneManager.LoadScene ("Assignment 2");
	}
}
