using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public float damage;
	public float projectileSpeed;

	public float lifeTime;

	//Effects
	public GameObject trail;
	public GameObject hitEffect;
	
	//Audio
	public GameObject launchSound;
	public GameObject flightSound;
	public GameObject hitSound;

	// Use this for initialization
	void Start () {
	
		//Set object kill time
		Destroy (this.gameObject, lifeTime);
	}
}
