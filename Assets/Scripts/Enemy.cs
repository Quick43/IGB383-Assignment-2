using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float health = 100;

    public GameObject deathEffect;
    public GameObject deathSound;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public virtual void takeDamage(float dmg) {

        health -= dmg;

        if (health <= 0) {
            Destroy(this.gameObject);

            Instantiate(deathEffect, transform.position, transform.rotation);
            Instantiate(deathSound, transform.position, transform.rotation);
        }
    }


}
