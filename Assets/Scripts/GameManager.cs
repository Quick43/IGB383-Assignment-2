using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager Instance;

	public float mouseSensitivity = 5.0f;

	public bool yInvert = false;


    //Camera variables
    public GameObject mainCamera;
    private int normalFov = 60;

    public GameObject playerDreadnaught;
    public GameObject alienMothership;

	public GameObject[] enemyList;

    //Gamestate variables
    public bool gameStarted = false;

	public bool gameOver = false;

    public GameObject[] asteroids;


	// Use this for initialization
	void Start () {
		//Hide Cursor = false
		Cursor.visible = true; 
        asteroids = GameObject.FindGameObjectsWithTag("Environment");
	}
	
	// Update is called once per frame
	void Update () {
	
		enemyList = GameObject.FindGameObjectsWithTag ("Enemy");


        //Check to see if Game Started
        if(Input.GetKeyDown("space") && gameStarted == false) {
            gameStarted = true;
            playerDreadnaught.SetActive(true);
            mainCamera.transform.position = playerDreadnaught.transform.position;
            mainCamera.GetComponent<ThirdPersonCamera>().enabled = true;
            mainCamera.GetComponent<Orbit>().enabled = false;
            mainCamera.GetComponent<Camera>().fieldOfView = 179;
        }

        //FOV warping effect
        if (mainCamera.GetComponent<Camera>().fieldOfView >= normalFov)
            mainCamera.GetComponent<Camera>().fieldOfView -= Time.deltaTime * 100;

        //Game Over conditions met
        if (enemyList.Length == 0 && !alienMothership)
            gameOver = true;
    }
}
