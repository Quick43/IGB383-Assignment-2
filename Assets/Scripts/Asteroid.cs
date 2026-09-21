using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public int resource = 50;
    // Start is called before the first frame update
    void Start()
    {
        resource = Random.Range(100, 1000);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
