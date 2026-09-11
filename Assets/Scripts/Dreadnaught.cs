using UnityEngine;
using System.Collections;

public class Dreadnaught : MonoBehaviour {

    //Missiles
	public GameObject missile;
	public float missileAmount = 10;
	public float missileMaxAmount = 10;
	private float missileFireRate = 0.25f;
	private float missileFireTime;
	public float missileRegenRate = 5.0f;
	private float missileRegenTimer;
    public GameObject missileLauncher;

    //Lasers
    public int laserAmmo = 5000;

    //Movement vars
    public GameObject[] waypoints;
    private int currentWaypoint = 0;
    private float moveSpeed = 20.0f;
    private float warpSpeed = 5000.0f;
    private float rotationSpeed = 0.25f;
    private float adjRotSpeed;
    private Quaternion targetRotation;
    private float minDistance = 5.0f;

    //Warp Effects
    public GameObject warpSound;

    // Use this for initialization
    void Start () {

        //Warp effects
        Instantiate(warpSound, transform.transform.position, transform.transform.rotation);
    }
	
	// Update is called once per frame
	void Update () {

        //Reduce warpspeed when spawned
        if (warpSpeed > 0) {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypoint].transform.position, warpSpeed * Time.deltaTime);
            warpSpeed -= Time.deltaTime * 6000.0f;
        }

        Missiles();

        MoveToWaypoints();
	}


    //Controls manual firing and regeneration of Dreadnaught missiles
    private void Missiles() {
        //Fire Missile
        if (Input.GetMouseButtonDown(1) && Time.time > missileFireTime && missileAmount > 0) {

            Instantiate(missile, missileLauncher.transform.position, missileLauncher.transform.rotation);

            missileAmount--;

            missileFireTime = Time.time + missileFireRate;
        }

        //Regenerate Missiles
        if (Time.time > missileRegenTimer && missileAmount < missileMaxAmount) {
            missileAmount++;
            missileRegenTimer = Time.time + missileRegenRate;
        }
    }

    private void MoveToWaypoints() {

        //Distance check to move to next waypoint
        if(Vector3.Distance(transform.position, waypoints[currentWaypoint].transform.position) <= minDistance) {

            currentWaypoint++;

            //If at last waypoint, make first waypoint next
            if (currentWaypoint == waypoints.Length)
                currentWaypoint = 0;
        }

        //Rotate towards waypoint
        targetRotation = Quaternion.LookRotation(waypoints[currentWaypoint].transform.position - transform.position);
        adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);

        //Move towards waypoint
        //transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypoint].transform.position, moveSpeed * Time.deltaTime);
    }
}
