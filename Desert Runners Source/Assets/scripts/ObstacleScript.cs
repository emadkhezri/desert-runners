using UnityEngine;
using System.Collections;

public class ObstacleScript : MonoBehaviour
{
    
    void Start()
    {
    }
	
    void Update()
    {
        float translation = Time.deltaTime * GameObject.Find("GameManager").GetComponent<GameManagerScript>().CurrentMovementSpeed;
        this.transform.Translate(new Vector3(-translation, 0, 0));
    }
}
