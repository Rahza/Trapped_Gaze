using UnityEngine;
using System.Collections;

public class Spike : MonoBehaviour
{

    // How much faster does the spike move if fixated? (applied multiplicative)
    public float fixatedModifier = 3.0f;

    // Speed of the spike (randomly assigned)
    private float speed = 0.3f;

    // Starting z position
    private float startZ;

    // Current distance to the wall
    public float distance = 0.0f;

    // GazeAware component
    private GazeAwareComponent gazeAwareComponent;

    // Use this for initialization
    void Start()
    {
        // Get component
        gazeAwareComponent = GetComponent<GazeAwareComponent>();

        // Save the start poisition
        startZ = transform.position.z;

        // Randomly increase or decrease the pitch of the wood sound for more variety
        GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.5f);

        // Randomly delay the sound for more variety with multiple simultaneously active spikes
        GetComponent<AudioSource>().PlayDelayed(Random.Range(0.0f, 0.5f));
    }

    void Update()
    {
        // Recalculate the current distance to the wall
        distance = startZ - transform.position.z;

        // Check if the player is looking at the spike and if the spike is in front of the wall
        if (gazeAwareComponent.HasGaze && distance > 0)
        {
            // Move backwards!
            transform.position -= transform.forward * speed * Time.deltaTime * fixatedModifier;
        }
        else
        {
            // Move forwards!
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    // Set the speed of the spike (called upon initialization)
    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
}
