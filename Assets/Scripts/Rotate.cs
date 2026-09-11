using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {
	
	public Vector3 rotation;
	
	// Use this for initialization
	void Start () {
		rotation.x = Random.Range (-5, 5);
		rotation.y = Random.Range (-5, 5);
		rotation.z = Random.Range (-5, 5);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(rotation * Time.deltaTime);
	}
}
