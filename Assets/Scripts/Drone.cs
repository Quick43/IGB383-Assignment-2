using UnityEngine;
using System.Collections;

public class Drone : Enemy {

    GameManager gameManager;

    Rigidbody rb;

    //Movement & Rotation Variables
    public float speed = 50.0f;
    private float rotationSpeed = 5.0f;
    private float adjRotSpeed;
    private Quaternion targetRotation;
    public GameObject target;
    public float targetRadius = 200f;

    //Boid Steering/Flocking Variables
    public float seperationDistance = 25.0f;
    public float cohesionDistance = 50.0f;
    public float seperationStrength = 250.0f;
    public float cohesionStrength = 25.0f;
    private Vector3 cohesionPos = new Vector3(0f, 0f, 0f);
    private int boidIndex = 0;

    // Drone FSM Enumerator
    public enum DroneBehaviours
    {
        Idle,
        Scouting,
        Foraging
    }

    public DroneBehaviours droneBehaviour;

    // Drone Behaviour Variables
    public GameObject motherShip;
    public Vector3 scoutPosition;
    private float scoutTimer;
    private float detectTimer;
    private float scoutTime = 10.0f;
    private float detectTime = 5.0f;
    private float detectRadius = 400.0f;
    private int newResourceVal;
    public GameObject newResourceObject;

    // Use this for initialization
    void Start() {

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        motherShip = gameManager.alienMothership;
        scoutPosition = motherShip.transform.position;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {

        //Acquire player if spawned in
        if (gameManager.gameStarted)
            target = gameManager.playerDreadnaught;

        //Move towards valid targets
        if(target)
            MoveTowardsTarget(target.transform.position);

        BoidBehaviour();

        // Drone Behaviours - State Switching
        switch (droneBehaviour)
        {
            case DroneBehaviours.Scouting:
                Scouting();
                break;
        }
    }

    private void MoveTowardsTarget(Vector3 targetPos) {
        //Rotate and move towards target if out of range
        if (Vector3.Distance(targetPos, transform.position) > targetRadius) {

            //Lerp Towards target
            targetRotation = Quaternion.LookRotation(targetPos - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);

            rb.AddRelativeForce(Vector3.forward * speed * 20 * Time.deltaTime);
        }
    }

    private void BoidBehaviour()
    {
        // Increment boid index reference
        boidIndex++;

        // Check if the last boid  is in enemy list
        if(boidIndex >= gameManager.enemyList.Length)
        {
            // Recompute the cohesionForce
            Vector3 cohesiveForce = (cohesionStrength / Vector3.Distance(cohesionPos, transform.position)) * (cohesionPos - transform.position);
            // Apply Force
            rb.AddForce(cohesiveForce);
            // Reset the boid index
            boidIndex = 0;
            // Reset the cohesion position
            cohesionPos.Set(0f, 0f, 0f);
        }
        // Currently analysed boid variables
        Vector3 pos = gameManager.enemyList[boidIndex].transform.position;
        Quaternion rot = gameManager.enemyList[boidIndex].transform.rotation;
        float dist = Vector3.Distance(transform.position, pos);

        // If not this boid
        if(dist > 0f)
        {
            // If within seperation
            if(dist <= seperationDistance)
            {
                // Compute scale of seperation
                float scale = seperationStrength / dist;
                //Apply force to ourselves
                rb.AddForce(scale * Vector3.Normalize(transform.position - pos));
            }
            else if(dist < cohesionDistance && dist > seperationDistance)
            {
                // Calculate the current cohesionPos
                cohesionPos = cohesionPos + pos * (1f/(float)gameManager.enemyList.Length);
                //Rotate slightly towards current boid
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 1f);
            }
        }
    }

    // Drone FSM Behaviour - Scouting
    private void Scouting()
    {
        // If no new resource object found
        if(!newResourceObject)
        {
            // If close to scoutPosition, randomise new position within gamespace around mothership
            if(Vector3.Distance(transform.position, scoutPosition) < detectRadius && Time.time > scoutTimer)
            {
                // Generate new random position
                Vector3 position;
                position.x = motherShip.transform.position.x + Random.Range(-1500, 1500);
                position.y = motherShip.transform.position.y + Random.Range(-400, 400);
                position.z = motherShip.transform.position.z + Random.Range(-1500, 1500);
                scoutPosition = position;
                // Reset scout timer
                scoutTimer = Time.time + scoutTime;
            }
            else
            {
                MoveTowardsTarget(scoutPosition);
                Debug.DrawLine(transform.position, scoutPosition, Color.yellow);
            }
            // Every few seconds, check for new resources
            if(Time.time > detectTimer)
            {
                newResourceObject = DetectNewResources();
                detectTimer = Time.time + detectTime;
            }
        }
        // Resource found, head back to mothership
        else
        {
            target = motherShip;
            Debug.DrawLine(transform.position, target.transform.position, Color.green);
            // In range of mothership, relay information and reset to drone again
            if(Vector3.Distance(transform.position, motherShip.transform.position) < targetRadius)
            {
                motherShip.GetComponent<Mothership>().drones.Add(this.gameObject);
                motherShip.GetComponent<Mothership>().scouts.Remove(this.gameObject);
                motherShip.GetComponent<Mothership>().resourceObjects.Add(newResourceObject);
                newResourceVal = 0;
                newResourceObject = null;
                droneBehaviour = DroneBehaviours.Idle;
            }
        }
    }

    // Method used periodically by scouts/elite forages to check new valid resources
    private GameObject DetectNewResources()
    {
        // Go through lost of asteroids
        for(int i = 0; i < gameManager.asteroids.Length; i++)
        {
            // check if they are within detection radius
            if(Vector3.Distance(transform.position, gameManager.asteroids[i].transform.position) <= detectRadius)
            {
                if(gameManager.asteroids[i].GetComponent<Asteroid>().resource > newResourceVal)
                {
                    newResourceObject = gameManager.asteroids[i];
                    newResourceVal = newResourceObject.GetComponent<Asteroid>().resource;
                }
            }
        }
        // Double check to see if the mothership already knows about it
        if(motherShip.GetComponent<Mothership>().resourceObjects.Contains(newResourceObject))
        {
            return null;
        }
        else
        {
            return newResourceObject;
        }
    }

}
