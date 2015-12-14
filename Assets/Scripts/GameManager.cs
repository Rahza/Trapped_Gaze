using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class GameManager : MonoBehaviour
{

    // Camera rotation speed (after a wall has been finished)
    public float cameraSpeed = 3.0f;

    // The amount of lifes the player starts with
    public int lifes = 3;

    // Create a static variable to allow other classes to access the GameManager without explicitly referencing it
    public static GameManager instance = null;

    // Text that is shown when the player has lost all of his lifes, and the 
    public GameObject gameOverObject;

    // The camera will focus this object when the player has lost the game
    public Transform gameOverCameraTarget;

    // Text that shows the current score
    public Text scoreText;

    public Wall[] walls;

    // The wall that is currently active
    private Wall activeWall;
    // ID (index in the walls array) of the wall that is currently active 
    private int activeWallId;

    // Current amount of lifes of the player
    private int health;

    //Current score
    private float score = 0;

    // Set to true after the game is over
    private bool waitForRestart = false;

    void Start()
    {
        // Set the (static) instance as a reference to this game object
        instance = this;

        // Initialize health
        health = lifes;

        // Init the first wall and camera
        InitWall();
        InitCamera();
    }

    void Update()
    {
        // If waitForRestart is true (meaning that the player has lost all of his lives)...
        if (waitForRestart)
        {
            // ...wait for the player to press the space key...
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // ...and then restart the game by reloading the current scene!
                Application.LoadLevel(Application.loadedLevel);
            }
        }
        else
        {
            // The game is not over yet, so increase the score and update the corresponding text!
            score += Time.deltaTime * activeWall.GetLevel();
            scoreText.text = "Score: " + Mathf.Round(score);
        }
    }

    // Init the first wall
    void InitWall()
    {
        // Set the ID (index) to 0
        activeWallId = 0;
        // Update the activeWall game object reference
        activeWall = walls[activeWallId];
        // Activate the wall
        activeWall.Activate();
    }

    // Init the camera
    // (The initial purpose of this method was to allow easy removal/addition of walls - however, the walls themselves have been scripted in a way that this is not possible so this function is somewhat redundant but still fulfills its purpose)
    void InitCamera()
    {
        // Initialise a new Vector3
        Vector3 position = new Vector3();

        // For each wall...
        foreach (Wall w in walls)
        {
            // Add the walls position to the position vector
            position += w.transform.position;
        }

        // Divide the position vector by the amount of walls to get their center
        position /= walls.Length;

        // Position the camera in the center of the wall
        Camera.main.transform.position = position;

        // Rotate the camera so that it is facing the current wall
        Camera.main.transform.LookAt(activeWall.transform);
    }

    // Called by each wall script once the wall has been finished - "failed" indicates if the player succeeded or not
    public void WallFinished(bool failed)
    {
        // If the player has failed...
        if (failed)
        {
            // ...reduce health by 1
            health--;
            // ...update the GUI
            LifeManager.instance.RemoveLife();
        }

        // Reset the camera
        ResetCamera();

        // If the wall ID can still be increased without exceeding the array's bounds, increase it by 1 - if not, reset it to 0
        if (activeWallId < walls.Length - 1)
        {
            activeWallId++;
        }
        else
        {
            activeWallId = 0;
        }

        // Update the activeWall game object
        activeWall = walls[activeWallId];

        // Check if the player still has lifes left
        if (health > 0)
        {
            // If yes, activate the next wall...
            activeWall.Activate();

            // ...and start rotating the camera towards it
            StartCoroutine(RotateCamera());
        }
        else
        {
            // If not, end the game
            GameOver();
        }
    }

    // Resets the camera by abandoning the coroutine and resetting its rotation because if walls were finished way too fast, the camera would get "stuck" mid rotation
    private void ResetCamera()
    {
        // First of all, stop all coroutines that are currently running 
        StopAllCoroutines();

        // Calculate the point that the camera should be looking at
        Vector3 targetPoint = activeWall.transform.position;
        // Calculate the necessary rotation for the camera to look at the target
        Quaternion targetRotation = Quaternion.LookRotation(targetPoint - Camera.main.transform.position, Vector3.up);

        // Check if the angle between the camera's current rotation and the target rotation is greater than 1 degree, meaning that it has not reached its target yet
        if (Quaternion.Angle(Camera.main.transform.rotation, targetRotation) > 1f)
        {
            // Immediately set the camera's rotation to what it should be
            Camera.main.transform.rotation = targetRotation;
        }
    }

    // Called when the game has been lost
    private void GameOver()
    {
        // Show the game over object (Canvas text)
        gameOverObject.SetActive(true);

        // Start the gamer over camera coroutine
        StartCoroutine(GameOverCamera());

        // Set waitForRestart to true (the score will not increase anymore and the game will restart when pressing space)
        waitForRestart = true;
    }

    // Coroutine for camera rotation
    private IEnumerator RotateCamera()
    {
        // Get the target point of the rotation
        Vector3 targetPoint = activeWall.transform.position;
        // Calculate the necessary rotation for the camera to look at the target point
        Quaternion targetRotation = Quaternion.LookRotation(targetPoint - Camera.main.transform.position, Vector3.up);

        // While the angle between the camera's current rotation and the target rotation is greater than 1 degree...
        while (Quaternion.Angle(Camera.main.transform.rotation, targetRotation) > 1f)
        {
            // Slowly update the camera's rotation (slower towards the end)
            Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, targetRotation, Time.deltaTime * cameraSpeed);
            yield return null;
        }

    }

    // Coroutine for camera rotation when the game is over
    private IEnumerator GameOverCamera()
    {
        // Slowly decrease the level's saturation so that it turns black and white
        Camera.main.GetComponent<ColorCorrectionCurves>().saturation = Mathf.Lerp(Camera.main.GetComponent<ColorCorrectionCurves>().saturation, 0.0f, Time.deltaTime);

        // Basically the same as RotateCamera(), except with gameOverCameraTarget instead of activeWall

        Vector3 targetPoint = gameOverCameraTarget.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetPoint - Camera.main.transform.position, Vector3.up);

        while (Quaternion.Angle(Camera.main.transform.rotation, targetRotation) > 1f)
        {
            Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, targetRotation, Time.deltaTime * cameraSpeed);
            yield return null;
        }

    }
}
