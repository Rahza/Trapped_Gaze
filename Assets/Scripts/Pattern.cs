using UnityEngine;

// Very simple class to create "Pattern" objects used by the "Blink Wall" - TRUE represents an opened eye, FALSE represents a closed eye
public class Pattern
{

    public bool left = true;
    public bool right = true;

    public Pattern()
    {
        // Upon initialization, randomly assign a value of true or false to the left and right bools
        left = Random.Range(0, 2) > 0 ? true : false;
        right = Random.Range(0, 2) > 0 ? true : false;
    }

}
