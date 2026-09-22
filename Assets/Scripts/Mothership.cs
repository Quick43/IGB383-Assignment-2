using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms.Impl;

public class Mothership : Enemy {
    GameManager gameManager;
    public GameObject enemy;
    public int numberOfEnemies = 20;

    public GameObject spawnLocation;

    // Resource Harvesting Variables
    public List<GameObject> drones = new List<GameObject>();
    public List<GameObject> scouts = new List<GameObject>();
    public List<GameObject> foragers = new List<GameObject>();
    public List<GameObject> elites = new List<GameObject>();
    private int maxScouts = 4;
    private int maxElites = 2;
    private int maxForagers = 3;
    public List<GameObject> unassignedResourceObjects = new List<GameObject>();
    public List<GameObject> assignedResourceObjects = new List<GameObject>();
    private float forageTimer;
    private float forageTime = 10.0f;
    public float totalResource = 0;

    // Beam Weapon
    public GameObject beamMuzzle;
    public GameObject beamTarget;
    public LineRenderer beam;
    private float beamFireRate = 3.0f;
    private float beamFireTime;
    private float beamFireDuration = 1.5f;

    // Shield
    public float shield = 5000;
    public GameObject shieldObject;

    public override void takeDamage(float dmg)
    {
        // Take damage with regards to active shield value
        if(shield > dmg)
        {
            shield -= dmg;
            dmg = 0;
        }
        else if(shield <= dmg)
        {
            dmg -= shield;
            shield = 0;
            shieldObject.SetActive(false);
        }
        health -= dmg;
    }

    // initialise the boids
    void Start() {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        for (int i = 0; i < numberOfEnemies; i++) {

            Vector3 spawnPosition = spawnLocation.transform.position;

            spawnPosition.x = spawnPosition.x + Random.Range(-50, 50);
            spawnPosition.y = spawnPosition.y + Random.Range(-50, 50);
            spawnPosition.z = spawnPosition.z + Random.Range(-50, 50);

            GameObject thisEnemy = Instantiate(enemy, spawnPosition, spawnLocation.transform.rotation) as GameObject;
            drones.Add(thisEnemy);
        }
    }

    // Update is called once per frame
    void Update() {
        // (Re)Initialise scouts continuously
        if(scouts.Count < maxScouts)
        {
            GameObject scout = GetBestScout();
            scouts.Add(scout);
            drones.Remove(scout);
            scouts[scouts.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.Scouting;
        }
        // Once resources have been found, initialise elites
        if(unassignedResourceObjects.Count >= 5 && elites.Count < maxElites && foragers.Count < maxForagers)
        {
            AssignForagers();  
        }
        // (Re)Determine best resource objects periodically
        if(unassignedResourceObjects.Count > 0 && Time.time > forageTimer)
        {
            // Sort resource objects delegated by their resource amount
            unassignedResourceObjects.Sort(delegate(GameObject a, GameObject b) {
                return b.GetComponent<Asteroid>().resource.CompareTo(a.GetComponent<Asteroid>().resource);
            });
            forageTimer = Time.time + forageTime;
        }
        // Fire beam weapon
        if(gameManager.gameStarted)
        {
            FireBeam();
        }
    }

    private void AssignForagers()
    {
        float speedWeight = 0.3f;
        float carryWeight = 0.2f;
        float resourceWeight = 0.4f;
        float distanceWeight = 0.1f;
        for(int i = 0; i < 5; i++)
        {
            float bestScore = 0;
            int bestDroneIndex = 0;
            GameObject thisResourceObject = unassignedResourceObjects[i];
            Asteroid thisAsteroid = thisResourceObject.GetComponent<Asteroid>();
            for(int j = 0; j < drones.Count; j++)
            {
                Drone thisDrone = drones[j].GetComponent<Drone>();
                float score = thisDrone.speed * speedWeight + thisDrone.carryCapacity * carryWeight + thisAsteroid.resource * resourceWeight - Vector3.Distance(transform.position, thisResourceObject.transform.position) * distanceWeight;
                if(score > bestScore)
                    bestDroneIndex = j;
            }
            GameObject bestDrone = drones[bestDroneIndex];
            if(i < 3)
            {
                elites.Add(bestDrone);
                drones.Remove(bestDrone);
                bestDrone.GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.EliteForaging;
                bestDrone.GetComponent<Drone>().targetResource = thisResourceObject;
            }
            else
            {
                foragers.Add(bestDrone);
                drones.Remove(bestDrone);
                foragers[foragers.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.Foraging;
            }
        }
        for(int i = 0; i < 5; i++)
        {
            assignedResourceObjects.Add(unassignedResourceObjects[i]);
            unassignedResourceObjects.Remove(unassignedResourceObjects[i]);
        }
    }

    private GameObject GetBestScout()
    {
        float speedWeight = 0.7f;
        float carryweight = 0.3f;
        float bestScore = 0;
        int bestScoutIndex = 0;
        for(int i = 0; i < drones.Count; i++)
        {
            float score =  drones[i].GetComponent<Drone>().speed * speedWeight - drones[i].GetComponent<Drone>().carryCapacity * carryweight;
            if(score > bestScore)
            {
                bestScore = score;
                bestScoutIndex = i;
            }
        }
        return drones[bestScoutIndex];
    }

    // Mothership Beam weapon firing
    private void FireBeam()
    {
        // Raycast hit object - out parameter
        RaycastHit hit;
        // If time to fire within LOS of the player
        if(Time.time > beamFireTime && Physics.Raycast(beamMuzzle.transform.position, -(beamMuzzle.transform.position - gameManager.playerDreadnaught.transform.position).normalized, out hit, 1000.0f))
        {
            // rotate beam empty towards player
            beamMuzzle.transform.LookAt(gameManager.playerDreadnaught.transform.position);
            // Turn beam renderer on
            beam.enabled = true;
            // Set draw for origin of beam
            beam.SetPosition(0, beamMuzzle.transform.position);
            // set default position for destination of beam
            beam.SetPosition(1, beamTarget.transform.position);
            // Animate beam texture
            beam.material.SetTextureOffset("_MainTex", new Vector2(-Time.time * 3, 0.0f));
            // Playe beam sound
            if(!GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Play();
            }
            // If the beam is intercepted by the player
            if(hit.transform.tag == "Player")
            {
                // Set player position for destination of beam
                beam.SetPosition(1, hit.transform.position);
            }
        }
        else
        {
            beam.enabled = false;
            GetComponent<AudioSource>().Stop();
        }
        // Cooldown Beam
        if(Time.time >= beamFireTime + beamFireDuration)
        {
            beam.enabled = false;
            beamFireTime = Time.time + beamFireRate;
        }
    }
}


