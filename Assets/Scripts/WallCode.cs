using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class WallCode : Wall {

    // Prefab of the number game object
    public GameObject numberPrefab;

    // Maximum amount of time
    public float maxTime = 15.0f;

    // Gap between number objects
    public float gap = 1.0f;

    // "Charge" time (how long does the user focus his ganze on one of the numbers to "activate" it)
    public float chargeTime = .5f;

    // Text color for active and inactive numbers
    public Color colorActive;
    public Color colorInactive;

    // List with all of the number game objects
    private List<GameObject> numbers;

    // The current number the user needs to look at
    private int currentNumber;

    // x position of the numbers
    private float codeX;

    // Current charge
    private float chargingTime;

    // GazePointData component
    private GazePointDataComponent gazePointDataComponent;

    // AudioSource component
    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();

        // Get components
        audioSource = GetComponent<AudioSource>();
        gazePointDataComponent = GetComponent<GazePointDataComponent>();

        // Get the bounds of the wall
        Vector3 meshBounds = GetComponent<BoxCollider>().bounds.size;

        // Calculate the x position where the numbers should be instantiated at
        codeX = transform.position.x + meshBounds.x / 2 + numberPrefab.GetComponent<Renderer>().bounds.size.x / 2;
    }

    protected override void OnUpdate()
    {
        // Get the last gaze point of the player
        var lastGazePoint = gazePointDataComponent.LastGazePoint;

        // Kind of unnecessary - allows the mouse to replace gaze if the eye tracker does not return valid data
        Vector3 direction = Camera.main.ScreenPointToRay(Input.mousePosition).direction;

        // Check if the last gaze point is valid
        if (lastGazePoint.IsValid)
        {
            // Get screen space position of the gaze point
            Vector3 gazePointInScreenSpace = lastGazePoint.Screen;

            // Get the world space position of the gaze point
            Vector3 gazePointInWorldSpace = Camera.main.ScreenToWorldPoint(new Vector3(gazePointInScreenSpace.x, gazePointInScreenSpace.y, -transform.position.x));

            // Make the spotlight focus on the gaze target (some kind of movement smoothing would be appropriate here)
            LightManager.instance.FocusTarget(gazePointInWorldSpace);

            // Substract the camera's position from the gaze point to get the direction of the gaze from the camera
            direction = gazePointInWorldSpace - Camera.main.transform.position;
        }

        RaycastHit hit;
        // Raycast from the camera's position in the direction of the previously calculated vector
        if (Physics.Raycast(Camera.main.transform.position, direction, out hit))
        {
            // Check if the raycast hit a number
            if (hit.collider.tag == "Number")
            {
                // Get the index in the numbers array of the hit object
                int index = numbers.IndexOf(hit.collider.gameObject);

                // If the index matches the current number...
                if (index == currentNumber)
                {
                    // And if it is fully charged...
                    if (chargingTime >= chargeTime)
                    {
                        // Play a sound
                        audioSource.Play();

                        // Reset the charging time
                        chargingTime = 0.0f;

                        // Increase the current number by one
                        currentNumber++;
                    } else
                    {
                        // If it is not fully charged yet, increase the charge
                        chargingTime += Time.deltaTime;
                    }
                // If the index is a number greater than the current number...
                } else if (index > currentNumber)
                {
                    for (int i = 0; i <= currentNumber; i++)
                    {
                        // Reset the color for each number that has already been activated
                        numbers[i].GetComponentInChildren<TextMesh>().color = colorInactive;
                    }

                    // Set the current number back to 0
                    currentNumber = 0;

                    // Reset the charging time
                    chargingTime = 0.0f;
                } 
            }
            // If it didn't hit a number...
            else
            {
                // ...decrease the charge
                chargingTime -= Time.deltaTime;
            }
        }
        // And if it didn't hit anything at all...
        else
        {
            // ...also decrease the charge!
            chargingTime -= Time.deltaTime;
        }

        // Make sure the charge doesn't get too low/high
        chargingTime = Mathf.Clamp(chargingTime, 0.0f, chargeTime);

        // First, check if the current number does still exist - if yes, set its color according to its current charge
        if (currentNumber < numbers.Count) numbers[currentNumber].GetComponentInChildren<TextMesh>().color = Color.Lerp(colorInactive, colorActive, chargingTime / chargeTime);
    }

    protected override bool CheckFail()
    {
        // The player has failed if the time exceeds the maximum time
        return time > maxTime;
    }

    protected override bool CheckFinish()
    {
        // The player has finished the wall if the current number is greater than or equal the amount of numbers in the numbers array
        return currentNumber >= numbers.Count;
    }

    // Called upon activation
    protected override void OnActivate()
    {
        // Reset the charging time
        chargingTime = 0.0f;

        // Reset the current number
        currentNumber = 0;

        // Create an empty list of numbers
        numbers = new List<GameObject>();

        // Spawn new numbers
        for (int i = 1; i <= level; i++)
        {
            SpawnNumber(i);
        }
    }

    // Spawn a new number
    private void SpawnNumber(int number)
    {
        // Get a random position on the wall
        Vector3 position = new Vector3(codeX, UnityEngine.Random.Range(yMin, yMax), UnityEngine.Random.Range(zMin, zMax));

        // Make sure the sphere doesn't recognize the wall game object 
        int layerMask = 1 << 0;

        // Use a count variable to prevent an endless loop
        int count = 0;

        // Check if there already is a number at the previously calculated position
        while (Physics.CheckSphere(position, gap, layerMask))
        {
            // Calculate a new position
            position = new Vector3(codeX, UnityEngine.Random.Range(yMin, yMax), UnityEngine.Random.Range(zMin, zMax));

            // Check if count is getting too big and return if that is the case
            if (count > 10)
            {
                return;
            }

            // Otherwise, increase count by one
            count++;
        }
        
        // Instantiate a new number object at the position
        GameObject numberObject = Instantiate(numberPrefab, position, numberPrefab.transform.rotation) as GameObject;

        // Get the TextMesh component
        TextMesh textComponent = numberObject.GetComponentInChildren<TextMesh>();

        // Set the text to the number
        textComponent.text = "" + number;

        // Set the color to the inactive color
        textComponent.color = colorInactive;

        // Add the game object to the list
        numbers.Add(numberObject);
    }

    protected override void OnReset()
    {
        // Upon reset, destroy all of the number game objects

        foreach (GameObject o in numbers)
        {
            Destroy(o);
        }
    }
}
