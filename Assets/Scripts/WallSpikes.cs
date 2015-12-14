using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class WallSpikes : Wall {

    // Prefab of the spikes
    public GameObject spikePrefab;

    // Prefab of the hole projectors
    public GameObject holeProjectorPrefab;

    // Maximum time
    public float maxTime = 10.0f;

    // Gap between spikes
    public float gap = 2.0f;

    // Minimum speed of a spike
    public float minSpeed = 0.1f;

    // Maximum speed of a spike
    public float maxSpeed = 1.0f;

    // z offset of the spikes
    public const float spikeOffset = 2f;

    // Maximum distance the spikes may reach before the player fails
    public float failDistance = 10.0f;

    // Total cumulative speed of all the spikes
    private float totalSpeed = 1.0f;

    // z position of the spikes
    private float spikeZ;

    // Spike list
    private List<GameObject> spikes;

    // Projector list
    private List<GameObject> projectors;

    // Current target
    private Spike target;

    protected override void Awake()
    {
        base.Awake();

        // Get the bounds of the wall
        Vector3 meshBounds = GetComponent<BoxCollider>().bounds.size;

        // Calculate the z position at which spikes should be instantiated
        spikeZ = transform.position.z - meshBounds.z / 2 + spikePrefab.GetComponent<Renderer>().bounds.size.z - spikeOffset;
    }

    protected override bool CheckFail()
    {
        foreach (GameObject o in spikes)
        {
            // For every spike, check if its distance from the wall is greater than the failDistance. If true, it means the player has failed
            if (o.GetComponent<Spike>().distance >= failDistance)
            {
                return true;
            }
        }

        return false;
    }

    protected override bool CheckFinish()
    {
        // The player has failed if the time exceeds maxTime
        return time > maxTime;
    }

    protected override void OnUpdate()
    {
    }

    // Upon activation
    protected override void OnActivate()
    {
        // Calculate the cumulative speed of all spikes based on the current level
        totalSpeed = level / 3.0f;

        // Initialize lists
        spikes = new List<GameObject>();
        projectors = new List<GameObject>();

        // Spawn the spikes!
        for (int i = 0; i < level; i++)
        {
            SpawnSpike();
        }

        // Set the speed of the spikes
        SetSpikeSpeed();
    }

    private void SpawnSpike()
    {
        // Calculate a random position
        Vector3 position = new Vector3(UnityEngine.Random.Range(xMin, xMax), UnityEngine.Random.Range(yMin, yMax), spikeZ);

        // Use a count variable to prevent an endless loop
        int count = 0;

        // Check if there already is a number at the previously calculated position
        while (Physics.CheckSphere(position, gap))
        {
            // Calculate a new position
            position = new Vector3(UnityEngine.Random.Range(xMin, xMax), UnityEngine.Random.Range(yMin, yMax), spikeZ);

            // Check if count is getting too big and return if that is the case
            if (count > 10)
            {
                count = 0;
                return;
            }

            // Otherwise, increase count by one
            count++;
        }

        // Instantiate a new number object at the position
        GameObject spike = Instantiate(spikePrefab, position, spikePrefab.transform.rotation) as GameObject;

        // Add it to the spikes list
        spikes.Add(spike);

        // Set the z coordinate of the position vector to 0
        position.z = 0;

        // Get the rotation of the hole projector prefab
        Quaternion rotation = holeProjectorPrefab.transform.rotation;

        // For more variety, randomly assign z rotation
        rotation.z = UnityEngine.Random.Range(0, 360);

        // Instantiate the projector
        GameObject projector = Instantiate(holeProjectorPrefab, position, rotation) as GameObject;

        // Add it to the projectors list
        projectors.Add(projector);
    }

    // Set the speed of the spikes
    private void SetSpikeSpeed()
    {
        // Create a new array of random floats
        float[] rnd = new float[spikes.Count];

        // Initialize a sum float
        float sum = 0.0f;

        // For each spike...
        for (int i = 0; i < spikes.Count; i++)
        {
            // Calculate a random speed between the previously defined minSpeed and maxSpeed values
            float x = UnityEngine.Random.Range(minSpeed, maxSpeed);

            // Set the value of the float at the corresponding array position to the calculated random number
            rnd[i] = x;

            // ...and add it to the sum
            sum += x;
        }

        // After the array has been filled, iterate again
        for (int i = 0; i < spikes.Count; i++)
        {
            // Divide each random value by the sum and multiplicate it by totalSpeed to make sure that the sum of all random numbers equals totalSpeed
            rnd[i] = (rnd[i] / sum) * totalSpeed;

            // Set the speed of the corresponding spike
            spikes[i].GetComponent<Spike>().SetSpeed(rnd[i]);
        }
    }

    // On Reset...
    protected override void OnReset()
    {
        // ...destroy all of the spikes
        foreach (GameObject o in spikes)
        {
            Destroy(o);
        }

        // ...and all of the projectors
        foreach (GameObject o in projectors)
        {
            Destroy(o);
        }
    }

}
