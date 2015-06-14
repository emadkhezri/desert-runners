using UnityEngine;
using System.Collections;

public class ObstacleScript : MonoBehaviour
{

    private float MOVEMENT_SPEED;
    
    void Start()
    {
        MOVEMENT_SPEED = GameObject.Find("GameManager").GetComponent<GameManagerScript>().MovementSpeed;
    }
	
    void Update()
    {
        float translation = Time.deltaTime * MOVEMENT_SPEED;
        this.transform.Translate(new Vector3(-translation, 0, 0));
    }
}
