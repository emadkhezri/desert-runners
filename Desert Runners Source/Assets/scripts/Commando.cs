using UnityEngine;
using System.Collections;

public class Commando : MonoBehaviour
{
    
    private MovementParams movementParams;
    
    void Update()
    {
        if (movementParams.isMoving)
        {
            float distCovered = (Time.time - movementParams.startTime) * MovementParams.SPEED;
            float fracJourney = distCovered / movementParams.JourneyLength;
            transform.position = Vector3.Lerp(movementParams.startPos, movementParams.endPos, fracJourney);
            if (fracJourney >= 1)
                movementParams.isMoving = false;
        }
    }
    
    void OnTriggerEnter2D(Collider2D col)
    {
        GameObject.Find("GameManager").GetComponent<GameManagerScript>().gameOver();
    }
    
    public void Move(Vector3 dest)
    {
        if (this.transform.position == dest
            || movementParams.endPos == dest)
            return;
        
        movementParams.startPos = this.transform.position;
        movementParams.endPos = dest;            
        movementParams.startTime = Time.time;
        GameObject.Find("MoveCommandoSound").GetComponent<AudioSource>().Play();
        movementParams.isMoving = true;
    }
    
    private struct MovementParams
    {
        public const float SPEED = 2000;
        public bool isMoving;
        public Vector3 startPos;
        public Vector3 endPos;        
        public float startTime;
        
        
        public float JourneyLength
        {
            get
            {
                return Vector3.Distance(startPos, endPos); 
            }
        }
    }
	
}
