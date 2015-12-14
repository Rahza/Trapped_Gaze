using UnityEngine;
using System.Collections;

public abstract class Wall : MonoBehaviour
{

    // Margin values - how big the margin of possibly initialised game objects should be for each axis
    public float marginX = 1.0f;
    public float marginY = 1.0f;
    public float marginZ = 1.0f;

    // The two torches that illuminate the wall
    public GameObject light1, light2;

    // If the wall is currently active
    private bool active = false;

    // Current level of the wall
    protected int level = 0;

    // Time that the wall has been active
    protected float time = 0.0f;

    // Values of the wall's bounds
    protected float xMin, xMax, yMin, yMax, zMin, zMax;

    protected virtual void Awake()
    {
        // Get the bounds of the object
        Vector3 meshBounds = GetComponent<BoxCollider>().bounds.size;

        // Save the bound values
        xMin = transform.position.x - meshBounds.x / 2 + marginX;
        xMax = transform.position.x + meshBounds.x / 2 - marginX;
        yMin = transform.position.y - meshBounds.y / 2 + marginY;
        yMax = transform.position.y + meshBounds.y / 2 - marginY;
        zMin = transform.position.z - meshBounds.z / 2 + marginZ;
        zMax = transform.position.z + meshBounds.z / 2 - marginZ;
    }

    protected virtual void Update()
    {
        // If the wall is active...
        if (active)
        {
            // Call the OnUpdate() function
            OnUpdate();

            // Check if the wall has been finished or if the player has failed
            if (CheckFinish())
            {
                Finished();
            }
            else if (CheckFail())
            {
                Failed();
            }

            // Increase the time
            time += Time.deltaTime;
        }
    }

    // The wall has been finished!
    protected virtual void Finished()
    {
        Reset();
        GameManager.instance.WallFinished(false);
    }

    // The player has failed!
    protected virtual void Failed()
    {
        Reset();
        GameManager.instance.WallFinished(true);
    }

    // Reset the wall
    private void Reset()
    {
        // Set it to false
        active = false;

        // Deactive the torches
        light1.SetActive(false);
        light2.SetActive(false);

        // Call the OnReset() function
        OnReset();
    }

    // Activate the wall
    public void Activate()
    {
        // Increase its level by 1
        level++;

        // Set active to true
        active = true;

        // Reset the time
        time = 0.0f;

        // Activate the corresponding torches - necessary because apparantly Unity doesn't like too many simultaneously active light sources
        light1.SetActive(true);
        light2.SetActive(true);

        // Call OnActivate()
        OnActivate();
    }

    // Return the current level
    public int GetLevel()
    {
        return level;
    }

    // Called upon activation
    protected abstract void OnActivate();

    // Called when the wall is reset
    protected abstract void OnReset();

    // Called every frame if the wall is active
    protected abstract void OnUpdate();

    // Return TRUE if the player has failed, FALSE if he didn't
    protected abstract bool CheckFail();

    // Return TRUE if the player has finished the wall, FALSE if he didn't
    protected abstract bool CheckFinish();
}
