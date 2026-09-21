using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms.Impl;

public class Mothership : MonoBehaviour {

    public GameObject enemy;
    public int numberOfEnemies = 20;

    public GameObject spawnLocation;

    // Resource Harvesting Variables
    public List<GameObject> drones = new List<GameObject>();
    public List<GameObject> scouts = new List<GameObject>();
    public List<GameObject> elites = new List<GameObject>();
    public int maxScouts = 4;
    public int maxElites = 4;
    public List<GameObject> resourceObjects = new List<GameObject>();
    private float forageTimer;
    private float forageTime = 10.0f;
    public float totalResource = 0;

    // initialise the boids
    void Start() {

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
        if(elites.Count < maxElites && resourceObjects.Count != 0)
        {
            GameObject elite = GetBestElite();
            elites.Add(elite);
            drones.Remove(elite);
            elites[elites.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.Foraging;
        }
        // (Re)Determine best resource objects periodically
        if(resourceObjects.Count > 0 && Time.time > forageTimer)
        {
            // Sort resource objects delegated by their resource amount
            resourceObjects.Sort(delegate(GameObject a, GameObject b) {
                return b.GetComponent<Asteroid>().resource.CompareTo(a.GetComponent<Asteroid>().resource);
            });
            forageTimer = Time.time + forageTime;
        }
    }

    private GameObject GetBestElite()
    {
        int bestEliteIndex = 0;
        float bestScore = 0;
        float speedWeight = 0.35f;
        float carryWeight = 0.45f;
        float healthWeight = 0.2f;
        for(int i = 0; i < drones.Count; i++)
        {
            float score = drones[i].GetComponent<Drone>().speed * speedWeight + drones[i].GetComponent<Drone>().health * healthWeight + drones[i].GetComponent<Drone>().carryCapacity * carryWeight;
            if(score > bestScore)
            {
                bestScore = score;
                bestEliteIndex = i;
            }
        }
        return drones[bestEliteIndex];
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
}

