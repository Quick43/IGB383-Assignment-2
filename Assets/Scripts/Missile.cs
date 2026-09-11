using UnityEngine;
using System.Collections;

public class Missile : Projectile {

	GameManager gameManager;

	private float launchTime = 0.5f;

	private bool launched = false;

	private float lockRange = 2500;

	private GameObject target;
	
	private Quaternion targetRotation;
	
	public float rotationSpeed = 1.0f;

	// Use this for initialization
	void Start () {

		gameManager = GameObject.FindGameObjectWithTag ("GameManager").GetComponent<GameManager> ();

		//Launch Audio
		GameObject launchSFX = Instantiate(launchSound, transform.position, transform.rotation) as GameObject;
		launchSFX.transform.parent = this.transform;

		launchTime = Time.time + launchTime;

		//Set object kill time
		Destroy (this.gameObject, lifeTime);
	}
	
	// Update is called once per frame
	void Update () {

		if (launched == false && Time.time > launchTime) {

			launched = true;

			//If enemyTarget is within forward vector line of sight, shoot at it
			RaycastHit hit;
			if (Physics.Raycast (transform.position, transform.forward, out hit, lockRange)) {

				//Check if hit is an enemy and attack it
				if (hit.transform.tag == "Enemy")
					target = hit.transform.gameObject;
			} else 
				target = closestEnemy (transform.position, gameManager.enemyList);
		} 
		else if (launched == true && target != null) {
			//Slerp Rotate towards target - Smooth Lock
			//Determine the target rotation. This is the rotation if the transform looks at the target point
			targetRotation = Quaternion.LookRotation (target.transform.position - transform.position);
			
			//Smoothly rotate towards the target point.
			transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

			//Increase rotation speed
			rotationSpeed += Time.deltaTime * 10;
		}

		transform.position += Time.deltaTime * projectileSpeed * transform.forward;
	}

	void OnTriggerEnter(Collider otherObject){
		if (otherObject.tag == "Enemy" || otherObject.tag == "Boid" || otherObject.tag == "Environment") {
			otherObject.SendMessage ("takeDamage", damage, SendMessageOptions.DontRequireReceiver);
			Instantiate(hitEffect, transform.position, transform.rotation);
			Instantiate(hitSound, transform.position, transform.rotation);
			Destroy (this.gameObject);
		} 
	}


	public GameObject closestEnemy(Vector3 towerPosition, GameObject[] enemyList){
		
		float distance = Mathf.Infinity;
		GameObject closestUnit = null;
		
		foreach (GameObject enemyUnit in enemyList) {
			
			if (enemyUnit != null) {
				Vector3 thisVector = enemyUnit.transform.position - towerPosition;	//
				float curDistance = thisVector.sqrMagnitude;
				
				if (curDistance < distance) {
					closestUnit = enemyUnit;
					distance = curDistance;
				}
			}
		}
		return closestUnit;
	}
}
