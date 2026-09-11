using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GUIManager : MonoBehaviour {

	GameManager gameManager;

	Dreadnaught playerDreadnaught;

    
    public Text crossHair;
	public Text missilesRemaining;
	public Text enemyTargets;
	public Text lasersRemaining;

    public GameObject missionText;
    public GameObject victoryScreen;

	// Use this for initialization
	void Start () {
		gameManager = GameObject.FindGameObjectWithTag ("GameManager").GetComponent<GameManager> ();
	}

	// Update is called once per frame
	void Update () {

        if (gameManager.gameStarted) {
            //Find player Dreadnaught
            if (!playerDreadnaught) {
                playerDreadnaught = GameObject.FindGameObjectWithTag("Player").GetComponent<Dreadnaught>();
                missionText.SetActive(false);
            }
            //Update GUI elements
            else {
                crossHair.text = "+";
                missilesRemaining.text = "Missiles: " + playerDreadnaught.missileAmount + "/" + playerDreadnaught.missileMaxAmount;
                lasersRemaining.text = "Lasers: " + playerDreadnaught.laserAmmo;
                enemyTargets.text = "Enemies: " + gameManager.enemyList.Length;
            }
        }

		//GameOver Condition Checks
		if (gameManager.gameOver == true)
			victoryScreen.SetActive (true);
	}


	public void restartScene(){
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
	}
}
