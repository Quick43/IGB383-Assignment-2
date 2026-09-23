using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditorInternal;

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
        Foraging,
        EliteForaging,
        Attacking,
        Fleeing
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

    // Attacking Targeting
    private Vector3 tarVel;
    private Vector3 tarPrevPos;
    private Vector3 attackPos;
    private float distanceRatio = 0.05f;

    // Attacking
    private float fireTimer;
    private float fireTime = 1.0f;
    [SerializeField] private GameObject alienLaser;

    // Fleeing Targeting
    private Vector3 fleePos;
    private float hunterRadius = 80.0f;

    // Drone Utility Variable
    private float attackOrFlee;

    // Foraging Variables
    private int pickupAmount = 10;
    private int carriedResource = 0;
    public int carryCapacity = 50;
    private float gatherTimer;
    private float gatherTime = 3.0f;
    public GameObject targetResource;

    // Resupply variables
    private float resupplyTimer;
    private float resupplyTime = 5.0f;
    private float healAmount = 10.0f;

    // Use this for initialization
    void Start() {

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        motherShip = gameManager.alienMothership;
        scoutPosition = motherShip.transform.position;
        rb = GetComponent<Rigidbody>();

        // Randomise variables for heuristic
        health = Random.Range(200, 400);
        maxHealth = health;
        speed = Random.Range(25, 75);
        alienLaser.GetComponent<Laser>().damage = Random.Range(25, 125);
        carryCapacity = Random.Range(25, 100);
    }

    // Update is called once per frame
    void Update() {

        //Acquire player if spawned in
        if (gameManager.gameStarted)
        {
            target = gameManager.playerDreadnaught;
            // Heuristic function here
            attackOrFlee = health * Friends() * FriendsHealth();
            if(attackOrFlee >= 1000)
                droneBehaviour = DroneBehaviours.Attacking;
            else if(attackOrFlee < 1000)
                droneBehaviour = DroneBehaviours.Fleeing;
        }

        //Move towards valid targets
        //if(target)
        //    MoveTowardsTarget(target.transform.position);

        BoidBehaviour();

        // Drone Behaviours - State Switching
        switch (droneBehaviour)
        {
            case DroneBehaviours.Idle:
                Resupply();
                break;
            case DroneBehaviours.Scouting:
                Scouting();
                break;
            case DroneBehaviours.Attacking:
                Attacking();
                break;
            case DroneBehaviours.Fleeing:
                Fleeing();
                break;
            case DroneBehaviours.Foraging:
                Foraging();
                break;
            case DroneBehaviours.EliteForaging:
                EliteForaging();
                break;
        }
    }

    private void EliteForaging()
    {
        if(targetResource == null)
            return;
        // Foraging elite moves towards the asteroid
        if(Vector3.Distance(transform.position, targetResource.transform.position) > targetRadius && targetResource.GetComponent<Asteroid>().resource > 0 && carriedResource < carryCapacity)
            MoveTowardsTarget(targetResource.transform.position);
        else
        {
            // Every few seconds, check for new resources
            if(Time.time > detectTimer)
            {
                newResourceObject = DetectNewResources();
                detectTimer = Time.time + detectTime;
                if(newResourceObject != null)
                {
                    motherShip.GetComponent<Mothership>().scouts.Add(gameObject);
                    motherShip.GetComponent<Mothership>().elites.Remove(gameObject);
                    droneBehaviour = DroneBehaviours.Scouting;
                    return;
                }
            }
            // Foraging elite slowly gathers the material from the asteroid until it is depleted or it can not carry any more
            if(Time.time > gatherTimer && carriedResource < carryCapacity)
            {
                int resourceToBeTaken = targetResource.GetComponent<Asteroid>().resource = Mathf.Min(targetResource.GetComponent<Asteroid>().resource, pickupAmount);
                if(resourceToBeTaken + carriedResource > carryCapacity)
                    resourceToBeTaken = carryCapacity - carriedResource;
                targetResource.GetComponent<Asteroid>().resource -= resourceToBeTaken;
                carriedResource += resourceToBeTaken;
                gatherTimer = Time.time + gatherTime;
            }
            if(targetResource.GetComponent<Asteroid>().resource == 0 || carriedResource == carryCapacity)
            {
                // Foraging elite returns to motehrship and deposits resource
                if(Vector3.Distance(transform.position, motherShip.transform.position) > targetRadius)
                    MoveTowardsTarget(motherShip.transform.position);
                else
                {
                    motherShip.GetComponent<Mothership>().drones.Add(gameObject);
                    motherShip.GetComponent<Mothership>().elites.Remove(gameObject);
                    motherShip.GetComponent<Mothership>().totalResource += carriedResource;
                    targetResource = null;
                    carriedResource = 0;
                    droneBehaviour = DroneBehaviours.Idle;
                }
            }
        }
    }

    private void Foraging()
    {
        if(targetResource == null)
            return;
        // Forager moves towards the asteroid
        if(Vector3.Distance(transform.position, targetResource.transform.position) > targetRadius && targetResource.GetComponent<Asteroid>().resource > 0 && carriedResource < carryCapacity)
            MoveTowardsTarget(targetResource.transform.position);
        else
        {
            // Forager slowly gathers the material from the asteroid until it is depleted or it can not carry any more
            if(Time.time > gatherTimer && carriedResource < carryCapacity)
            {
                int resourceToBeTaken = targetResource.GetComponent<Asteroid>().resource = Mathf.Min(targetResource.GetComponent<Asteroid>().resource, pickupAmount);
                if(resourceToBeTaken + carriedResource > carryCapacity)
                    resourceToBeTaken = carryCapacity - carriedResource;
                targetResource.GetComponent<Asteroid>().resource -= resourceToBeTaken;
                carriedResource += resourceToBeTaken;
                gatherTimer = Time.time + gatherTime;
            }
            if(targetResource.GetComponent<Asteroid>().resource == 0 || carriedResource == carryCapacity)
            {
                // Forager returns to motehrship and deposits resource
                if(Vector3.Distance(transform.position, motherShip.transform.position) > targetRadius)
                    MoveTowardsTarget(motherShip.transform.position);
                else
                {
                    motherShip.GetComponent<Mothership>().drones.Add(gameObject);
                    motherShip.GetComponent<Mothership>().elites.Remove(gameObject);
                    motherShip.GetComponent<Mothership>().totalResource += carriedResource;
                    targetResource = null;
                    carriedResource = 0;
                    droneBehaviour = DroneBehaviours.Idle;
                }
            }
        }
    }

    private int Friends()
    {
        int clusterStrength = 0;
        for(int i =0; i < gameManager.enemyList.Length; i++)
        {
            if(Vector3.Distance(transform.position, gameManager.enemyList[i].transform.position) < targetRadius)
            {
                clusterStrength++;
            }
        }
        return clusterStrength;
    }

    private int FriendsHealth()
    {
        float totalHealth = 0;
        for(int i =0; i < gameManager.enemyList.Length; i++)
        {
            if(Vector3.Distance(transform.position, gameManager.enemyList[i].transform.position) < targetRadius)
            {
                totalHealth += gameManager.enemyList[i].GetComponent<Enemy>().health;
            }
        }
        return (int)totalHealth;
    }

    private void Attacking()
    {
        // Calculate target's velocity
        tarVel = (target.transform.position - tarPrevPos)/Time.deltaTime;
        tarPrevPos = target.transform.position;

        // Calculate intercept attack position (p = t + r * d * v)
        attackPos = target.transform.position + distanceRatio * Vector3.Distance(transform.position, target.transform.position) * tarVel;
        attackPos.y = attackPos.y + 10;
        Debug.DrawLine(transform.position, attackPos, Color.red);

        // Not in range of intercept - move into position
        if(Vector3.Distance(transform.position, attackPos) > targetRadius)
            MoveTowardsTarget(attackPos);
        else
        {
            // Look at target - Lerp towards target
            targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);
            
            // Fire Weapons at target
            if(Time.time > fireTimer)
            {
                Instantiate(alienLaser, transform.position, transform.rotation);
                fireTimer = Time.time + fireTime;
            }
        }
    }

    private void Fleeing()
    {
        // Calculate target's velocity
        tarVel = (target.transform.position - tarPrevPos)/Time.deltaTime;
        tarPrevPos = target.transform.position;

        // Calculate the flee position
        fleePos = transform.position + distanceRatio * -Vector3.Distance(transform.position, target.transform.position) * tarVel;
        Debug.DrawLine(transform.position, fleePos, Color.blue);

        // Not in range of flee position - move towards position
        if (Vector3.Distance(transform.position, target.transform.position) < hunterRadius)
            MoveTowardsTarget(fleePos);
        else
        {
            // Not in range of mothership - move towards mothership
            if(Vector3.Distance(transform.position, motherShip.transform.position) > targetRadius)
                MoveTowardsTarget(motherShip.transform.position);
            else
            {
                Resupply();
            }
        }
        
    }

    private void Resupply()
    {
        if (Vector3.Distance(transform.position, motherShip.transform.position) < targetRadius)
            MoveTowardsTarget(motherShip.transform.position);
        else
        {
            if(health == maxHealth)
                return;
            if(Time.time > resupplyTimer)
            {
                health += healAmount;
                if(health > maxHealth)
                    health = maxHealth;
                resupplyTimer = Time.time + resupplyTime;
            }
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
            Vector3 cohesiveForce = cohesionStrength / Vector3.Distance(cohesionPos, transform.position) * (cohesionPos - transform.position);
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
                motherShip.GetComponent<Mothership>().unassignedResourceObjects.Add(newResourceObject);
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
        if(motherShip.GetComponent<Mothership>().unassignedResourceObjects.Contains(newResourceObject) || motherShip.GetComponent<Mothership>().assignedResourceObjects.Contains(newResourceObject))
        {
            return null;
        }
        else
        {
            return newResourceObject;
        }
    }

}
