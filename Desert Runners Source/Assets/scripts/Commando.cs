using UnityEngine;
using System.Collections;

public class Commando : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D col)
    {
        GameObject.Find("GameManager").GetComponent<GameManagerScript>().gameOver();
    }
	
}
