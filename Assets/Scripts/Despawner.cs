using UnityEngine;
using System.Collections;

public class Despawner : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider otherObject){

		Destroy (otherObject.gameObject);
	}
}
