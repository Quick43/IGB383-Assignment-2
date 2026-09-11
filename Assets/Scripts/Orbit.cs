using UnityEngine;
using System.Collections;

public class Orbit : MonoBehaviour {

	public GameObject target;

	public float orbitSpeed = 100.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		transform.LookAt (target.transform.position);

		transform.RotateAround(target.transform.position, Vector3.up, orbitSpeed * Time.deltaTime);
	}
}
