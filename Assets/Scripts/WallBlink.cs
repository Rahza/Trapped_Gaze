using UnityEngine;
using System.Collections;
using System;

public class WallBlink : Wall {

    // Prefab game objects for the open and closed eye
    public GameObject openEyePrefab, closedEyePrefab;

    // Material of the eyebrow
    public Material eyebrowMaterial;

    // Position of the left and right eye
    public Transform leftPosition, rightPosition;

    // Maximum time
    public float maxTime = 10.0f;

    // How long the player needs to "hold" their pose before the pattern counts as finished
    public float chargeTime = 0.5f;

    // Modifier that will be applied when "discharging"
    public float dischargeModifier = 0.5f;

    // Array of the patterns
    private Pattern[] pattern;

    // Index of the current pattern
    private int currentIndex = 0;

    // Reference to the actual object of the current pattern
    private Pattern currentPattern;

    // Current "charge" (needs to reach chargeTime for the pattern to count as finished)
    private float charge = 0.0f;

    // References to the instantiated eyes
    private GameObject leftEye, rightEye;

    // EyePositionData component reference
    private EyePositionDataComponent eyePositionData;

    // AudioSource reference
    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();

        // Get components
        audioSource = GetComponent<AudioSource>();
        eyePositionData = GetComponent<EyePositionDataComponent>();
    }

    // Check if the player has failed
    protected override bool CheckFail()
    {
        // The player has failed if the time has run out
        return time > maxTime;
    }

    // Check if the player has finished the wall
    protected override bool CheckFinish()
    {
        // The wall is finished if the index of the current pattern is greater than the amount of patterns in the array, meaning he has finished all patterns
        return currentIndex >= pattern.Length;
    }

    // Called upon activation of the wall
    protected override void OnActivate()
    {
        // Reset the charge
        charge = 0.0f;

        // Set the index of the current pattern to 0
        currentIndex = 0;

        // Create the pattern array - one pattern for each level
        pattern = new Pattern[level];

        // Fill the array with new patterns
        for (int i = 0; i < pattern.Length; i++)
        {
            pattern[i] = new Pattern();
        }

        // Update the game object reference to the current pattern
        currentPattern = pattern[currentIndex];

        // Show the first pattern
        ShowPattern();
    }

    // Show a pattern
    private void ShowPattern()
    {
        // If there has already been a previous pattern, destroy the corresponding game objects
        if (leftEye != null) Destroy(leftEye);
        if (rightEye != null) Destroy(rightEye);

        // Check which prefab must be used for instantiation as the left eye and instantiate it
        GameObject toInstantiate = currentPattern.left ? openEyePrefab : closedEyePrefab;
        leftEye = Instantiate(toInstantiate, leftPosition.position, leftPosition.rotation) as GameObject;

        // Repeat for right eye
        toInstantiate = currentPattern.right ? openEyePrefab : closedEyePrefab;
        rightEye = Instantiate(toInstantiate, rightPosition.position, rightPosition.rotation) as GameObject;
    }

    protected override void OnReset()
    {
        // On reset, only both eyes have to be destroyed
        Destroy(leftEye);
        Destroy(rightEye);
    }

    protected override void OnUpdate()
    {
        // Check if the player is performing the pattern
        CheckPattern();

        // Set the color of the eyebrow based upon how high the current charge level is
        eyebrowMaterial.color = Color.Lerp(Color.red, Color.green, charge/chargeTime);

        // If the player has fully "charged" the pattern...
        if (charge >= chargeTime) {
            // Play the success sound
            audioSource.Play();

            // Reset the charge
            charge = 0.0f;

            // Increase the index of the current pattern
            currentIndex++;

            // If there are still patterns left...
            if (currentIndex < pattern.Length)
            {
                // Update the game object reference
                currentPattern = pattern[currentIndex];

                // Show the new pattern
                ShowPattern();
            }
        }
    }

    // Check if the player is performing the current pattern
    private void CheckPattern()
    {
        // Check if the eye position data for each eye is valid - I'm assuming that invalid data corresponds to a closed eye
        bool leftEye = eyePositionData.LastEyePosition.LeftEye.IsValid;
        bool rightEye = eyePositionData.LastEyePosition.RightEye.IsValid;

        // If the patterns match
        if (leftEye == currentPattern.left && rightEye == currentPattern.right)
        {
            // ... increase charge
            charge += Time.deltaTime;
        } else
        {
            // ... otherwise, decrease charge (but at a slightly slower rate)
            charge -= Time.deltaTime * dischargeModifier;
        }

        // Marke sure the current charge doesn't get too small or too high
        charge = Mathf.Clamp(charge, 0.0f, chargeTime);
    }
}
