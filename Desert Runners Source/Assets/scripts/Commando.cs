using UnityEngine;
using System.Collections;

public class Commando : MonoBehaviour
{
    
    private MovementParams movementParams;
    public GameObject gameManager;
    private GameObject moveCommandoSound;
    
    void Start()
    {
        moveCommandoSound = GameObject.Find("MoveCommandoSound");
        gameManager = GameObject.Find("GameManager");
    }
    
    void Update()
    {
        if (gameManager.GetComponent<GameManagerScript>().IsGameOver())
        {
            this.GetComponent<Animator>().speed = 0;
            return;
        }
        
        if (movementParams.isMoving)
        {
            float distCovered = (Time.time - movementParams.startTime) * MovementParams.SPEED;
            float fracJourney = distCovered / movementParams.JourneyLength;
            transform.position = Vector3.Lerp(movementParams.startPos, movementParams.endPos, fracJourney);
            if (fracJourney >= 1)
                movementParams.isMoving = false;
        }
        
        GetComponent<Animator>().speed = gameManager.GetComponent<GameManagerScript>().CurrentMovementSpeed / 500f;
    }
    
    void OnTriggerEnter2D(Collider2D col)
    {
        this.GetComponent<SpriteRenderer>().color = new Color(255, 0, 0, 0.5f);
        col.gameObject.GetComponent<SpriteRenderer>().color = new Color(255, 0, 0, 0.5f);
        gameManager.GetComponent<GameManagerScript>().gameOver();
    }
    
    public void Move(Vector3 dest)
    {
        if (this.transform.position == dest
            || movementParams.endPos == dest)
            return;
        
        movementParams.startPos = this.transform.position;
        movementParams.endPos = dest;            
        movementParams.startTime = Time.time;
        movementParams.isMoving = true;
        
        if (gameManager.GetComponent<GameManagerScript>().SettingsFile.enableSFX)
            moveCommandoSound.GetComponent<AudioSource>().Play();
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
