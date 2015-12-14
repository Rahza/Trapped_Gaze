using UnityEngine;
using System.Collections.Generic;

public class LifeManager : MonoBehaviour
{
    // Margin between the hearts
    public float margin = 20.0f;

    // Heart GameObject prefab
    public GameObject heartObject;

    // Static reference to the instance of this class
    public static LifeManager instance = null;

    // List of the heart GameObjects
    private List<GameObject> hearts;

    void Start()
    {
        // Initialise the instance
        instance = this;

        // Initialise the hearts list
        hearts = new List<GameObject>();

        // Get the width of the heart
        float width = heartObject.GetComponent<RectTransform>().rect.width;

        // Instantiate a heart for each life the player has
        for (int i = 0; i < GameManager.instance.lifes; i++)
        {
            // Instantiate the GameObject
            GameObject heart = Instantiate(heartObject) as GameObject;

            // Set the parent of the heart's transform
            heart.transform.SetParent(this.transform, false);

            // Reposition the heart so that they don't overlap each other
            heart.GetComponent<RectTransform>().position += new Vector3(i * (width + margin), 0, 0);

            // Add the GameObject to the hearts list
            hearts.Add(heart);
        }
    }

    // Called whenever the player has lost a life
    public void RemoveLife()
    {
        // Destroy the last GameObject in the list of hearts (the rightmost heart)
        Destroy(hearts[hearts.Count - 1]);

        // And then remove it from the list...
        hearts.RemoveAt(hearts.Count - 1);
    }

}
