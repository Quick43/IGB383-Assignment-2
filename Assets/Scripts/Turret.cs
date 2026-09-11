using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour {

	GameManager gameManager;
	Dreadnaught playerDreadnaught;

	public GameObject mouseFocus;

	public bool AITurret = false;
	public float changeTargetTime = 5.0f;
	private float changeTargetTimer;

	private Quaternion targetRotation;

	public float rotationSpeed = 1.0f;

	public GameObject turretObject;
	public GameObject turretBarrel;
	public GameObject[] turretMuzzles;
	
	private GameObject target;
	public float turretFireSpeed = 0.2f;
	private float turretFireTime;

	public GameObject turretProjectile;

	//Effects
	public GameObject turretFireEffect;

	// Use this for initialization
	void Start () {
		gameManager = GameObject.FindGameObjectWithTag ("GameManager").GetComponent<GameManager> ();
		playerDreadnaught = GameObject.FindGameObjectWithTag ("Player").GetComponent<Dreadnaught> ();
	}
	
	// Update is called once per frame
	void Update () {

		if (playerDreadnaught.laserAmmo > 0)
			TurretFiring ();

	}

	void TurretFiring(){

		//AI or Human Turret Controls
		if (AITurret == true && Time.time > changeTargetTimer && gameManager.enemyList.Length > 0) {

			int newEnemyIndex = Random.Range (0, gameManager.enemyList.Length);

			target = gameManager.enemyList[newEnemyIndex];

			changeTargetTimer = Time.time + changeTargetTime;

		} else if (AITurret == false){
			target = mouseFocus;
		}

		//If we have a target to shoot at
		if (target != null) {
			//Slerp Rotate towards target - Smooth Lock
			//Determine the target rotation. This is the rotation if the transform looks at the target point
			targetRotation = Quaternion.LookRotation (target.transform.position - turretObject.transform.position);

			//Smoothly rotate towards the target point.
			turretObject.transform.rotation = Quaternion.Slerp (turretObject.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

			//Fire Animation
			if (Time.time > turretFireTime) {

				if (Input.GetMouseButton (0) || AITurret == true) {
					turretBarrel.GetComponent<Animation> ().Play ("turret fire");

					//Decrease ammo
					playerDreadnaught.laserAmmo-=2;

					//Fire Projectiles
					Instantiate (turretProjectile, turretMuzzles [0].transform.position, turretMuzzles [0].transform.rotation);
					Instantiate (turretProjectile, turretMuzzles [1].transform.position, turretMuzzles [1].transform.rotation);

					//Spawn Visual Effects
					Instantiate (turretFireEffect, turretMuzzles [0].transform.position, turretMuzzles [0].transform.rotation);
					Instantiate (turretFireEffect, turretMuzzles [1].transform.position, turretMuzzles [1].transform.rotation);

					turretFireTime = Time.time + turretFireSpeed;
				}
			}
		}
	}
}
