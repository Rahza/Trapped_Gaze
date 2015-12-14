using UnityEngine;
using System.Collections;
using System;

public class WallTrack : Wall {

    // Prefab of the disk
    public GameObject diskPrefab;

    // Maximum time
    public float maxTime = 15.0f;

    // Distance necessary for the disk to reach its target
    public float distance = 2.0f;

    // Distance of the disk to its current target at which the sound should get faded out
    public float endSoundDistance = 3.0f;

    // Minimum speed of the disk
    public float minSpeed = 2.0f;

    // Time after which the player fails if he did not look at the disk
    public float focusTime = 3.0f;

    // Probability of the disk to "teleport" after reaching its current target
    public float startTeleportProbability = 0.1f;
    
    // Speed at which the disk "teleports"
    public float teleportSpeed = 10.0f;

    // Color of the disk with high charge
    public Color colorSafe;

    // Color of the disk with low charge
    public Color colorFail;

    // z position of the disk
    private float diskZ;

    // Instantiated disk game object
    private GameObject disk;

    // Current target of the disk
    private Vector3 target;

    // AudioSource component
    private AudioSource audioSource;

    // GazeAware component
    private GazeAwareComponent gazeAwareComponent;

    // Current charge
    private float charge;

    // Maximum charge
    private float maxCharge = 3.0f;

    // Current speed of the disk
    private float speed = 2.0f;

    private float speedModifier = 1.0f;

    private float teleportProbability = 0.1f;

    protected override void Awake()
    {
        base.Awake();

        // Get the bounds of the mesh
        Vector3 meshBounds = GetComponent<BoxCollider>().bounds.size;

        // Calculate the z position of the disk
        diskZ = transform.position.z + meshBounds.z / 2;
    }

    protected override bool CheckFail()
    {
        // The player fails as soon as the charge reaches 0
        return charge <= 0.0f;
    }

    protected override bool CheckFinish()
    {
        // The player wins if the time exceeds maxTime
        return time > maxTime;
    }

    // Upon activation
    protected override void OnActivate()
    {
        // Calculate maxCharge based upon the current level (higher level, lower maxCharge)
        maxCharge = focusTime * Mathf.Pow(0.97f, level);

        // Calculate speed based upon the current level (higher level, higher speed)
        speed = minSpeed * Mathf.Pow(1.1f, level);

        // Calculate teleportProbability based upon the current level (higher level, higer teleportProbability)
        teleportProbability = startTeleportProbability + level * 0.025f;

        // Initialize charge as maxCharge
        charge = maxCharge;

        // Calculate a new random target
        target = new Vector3(UnityEngine.Random.Range(xMin, xMax), UnityEngine.Random.Range(yMin, yMax), diskZ);

        // Calculate a random position for the disk
        Vector3 position = new Vector3(UnityEngine.Random.Range(xMin, xMax), UnityEngine.Random.Range(yMin, yMax), diskZ);

        // Instantiate the disk
        disk = Instantiate(diskPrefab, position, diskPrefab.transform.rotation) as GameObject;

        // Set the target of the LightManager to the disk
        LightManager.instance.SetTarget(disk.transform);

        // Get the AudioSource component
        audioSource = disk.GetComponent<AudioSource>();

        // Get the GazeAware component
        gazeAwareComponent = disk.GetComponent<GazeAwareComponent>();
    }

    // On reset...
    protected override void OnReset()
    {
        // ...destroy the disk game object
        Destroy(disk);
    }

    // Every frame...
    protected override void OnUpdate()
    {
        MoveDisk();

        // Check the user's gaze
        CheckGaze();
    }

    // Move the disk
    private void MoveDisk()
    {
        // Calculate the distance from the disk to the current target
        float currentDistance = Vector3.Distance(target, disk.transform.position);

        // If the distance is lower than the distance necessary to reach the target...
        if (currentDistance <= distance)
        {
            speedModifier = 1.0f;

            // ...stop the sound...
            audioSource.Stop();

            // ...calculate a new target...
            target = new Vector3(UnityEngine.Random.Range(xMin, xMax), UnityEngine.Random.Range(yMin, yMax), diskZ);

            if (UnityEngine.Random.Range(0.0f, 1.0f) < teleportProbability)
            {
                speedModifier = teleportSpeed;
            }

            // ...and start the sound again!
            audioSource.Play();
        }
        else if (currentDistance <= endSoundDistance)
        {
            // If the distance is lower than endSoundDistance, decrease the audio volume
            audioSource.volume -= Time.deltaTime;
        }
        else
        {
            // Otherwise (the disk is pretty far away from its destination), increase the volume
            audioSource.volume += Time.deltaTime;
        }

        // Clamp the volume
        audioSource.volume = Mathf.Clamp(audioSource.volume, 0.0f, 1.0f);

        // Move the disk (slower towards the end)
        disk.transform.position = Vector3.MoveTowards(disk.transform.position, target, Mathf.Lerp(0.0f, speed * speedModifier, currentDistance / 5.0f) * Time.deltaTime);
    }

    // Check the gaze of the player
    private void CheckGaze()
    {
        // If the user does not look at the disk...
        if (!gazeAwareComponent.HasGaze)
        {
            // ...decrease its charge
            charge -= Time.deltaTime;
        } else
        {
            // ...otherwise, increase its charge
            charge += Time.deltaTime;
        }

        // Clamp the charge
        charge = Mathf.Clamp(charge, 0.0f, maxCharge);

        // Set the disk's color based on its current charge
        disk.GetComponent<Renderer>().material.color = Color.Lerp(colorFail, colorSafe, charge / maxCharge);

        // Set the lights intensity based on the disk's current charge
        LightManager.instance.SetIntensity(charge / maxCharge);
    }
}
