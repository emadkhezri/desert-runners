using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class GameManagerScript : MonoBehaviour
{
    
    public GameObject Prefab_obs_indest;
    public GameObject[] commandos;
    public GameObject scoreText;
    public GameObject selectedCommandoGlyphRef;
    public GameObject selectedLaneHightlightRef;
    
    //Settings
    public bool tapAndReleaseMode = false;
    public bool allowSwaps = false;
    public bool rightToleftMovement = false;
    public bool useCommandoShadow = false;
    
    //properties
    public float CurrentMovementSpeed
    {
        get
        {
            return currentSpeed;
        }
    }
    
    //private fields
    private const float InitialSpeed = 150;
    private float currentSpeed = InitialSpeed;
    private float LANE_WIDTH = 100;
    private float lanesStartY; // read from gameobject
    private int LANE_COUNT = 6;
    private float OBSTACLE_GENERATE_DISTANCE = 1000; 
    private int OBSTACLE_COUNT = 1;
    private GameObject[] terrains; 
    private GameObject toMoveCommando;
    private GameObject toSwapCommando;
    private GameObject movingCommandoShadow;
    private float distanceTraveled = 0;
    private int lastGeneratedObstacleId = -1;
    private GameObject gameOverCanvasRef;
    private bool draggingCommando;

    void Start()
    {
        if (rightToleftMovement)
        {
            Matrix4x4 mat = Camera.main.projectionMatrix;
            mat *= Matrix4x4.Scale(new Vector3(-1.0f, 1f, 1f));
            Camera.main.projectionMatrix = mat;
        }
        
        Time.timeScale = 1;

        lanesStartY = GameObject.Find("LaneGuide").transform.position.y;

        // set initial commando positions
        for (int i=0; i<3; i++)
            commandos [i].transform.position = new Vector3(-400, getLaneCenterY(i + 1), 0);

        toMoveCommando = null;

        terrains = new GameObject[2];
        terrains [0] = GameObject.Find("Terrain1");
        terrains [1] = GameObject.Find("Terrain2");

        gameOverCanvasRef = GameObject.Find("GameOverCanvas");
        gameOverCanvasRef.SetActive(false);
		
        selectedCommandoGlyphRef.transform.position = new Vector3(-400, 0, 0);
        moveItemToLane(selectedCommandoGlyphRef, -1);
		
        selectedLaneHightlightRef.transform.position = new Vector3(0, 0, 0);
        moveItemToLane(selectedLaneHightlightRef, -1);

        draggingCommando = false;
    }

    void Update()
    {
        if (Time.timeScale == 0)
            return;

        float translation = Time.deltaTime * currentSpeed;
        distanceTraveled += translation;

        generateObstacles();

        // terrains
        if (terrains [0].transform.position.x <= -2048)
            terrains [0].transform.position = new Vector3(terrains [1].transform.position.x + 2048, terrains [1].transform.position.y, terrains [1].transform.position.z);
        if (terrains [1].transform.position.x <= -2048)
            terrains [1].transform.position = new Vector3(terrains [0].transform.position.x + 2048, terrains [0].transform.position.y, terrains [0].transform.position.z);

        terrains [0].transform.Translate(-translation, 0, 0);
        terrains [1].transform.Translate(-translation, 0, 0);

        if (Input.GetMouseButtonDown(0))
        {
            // find lane
            Vector3 convertedMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int laneNum = findLaneNum(convertedMouse.y);	
            toMoveCommando = getCommandoInLane(laneNum);
            if (toMoveCommando != null)
            {
                moveItemToLane(selectedCommandoGlyphRef, laneNum);
                draggingCommando = true;
                
                if (useCommandoShadow)
                    movingCommandoShadow = createCommandoShadow(toMoveCommando);
            }
        } else if (toMoveCommando != null && Input.GetMouseButtonUp(0))
        {
            if (draggingCommando)
            {
                draggingCommando = false;
                moveItemToLane(selectedLaneHightlightRef, -1);
                moveItemToLane(selectedCommandoGlyphRef, -1);
                if (useCommandoShadow)
                    Destroy(movingCommandoShadow);
            }
            
            if (tapAndReleaseMode)
            {
                Vector3 convertedMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                int laneNum = findLaneNum(convertedMouse.y);
			
                toSwapCommando = getCommandoInLane(laneNum);            
                if (toSwapCommando != null)
                {
                    if (!allowSwaps)
                        return;
                    toSwapCommando.GetComponent<Commando>().Move(toMoveCommando.transform.position);
                }
            
                Vector3 toMoveCommandoDest = toMoveCommando.transform.position;
                toMoveCommandoDest.y = getLaneCenterY(laneNum);
                toMoveCommando.GetComponent<Commando>().Move(toMoveCommandoDest);
            }
        } else if (draggingCommando)
        {
            Vector3 convertedMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int destLaneNum = findLaneNum(convertedMouse.y);
            
            moveItemToLane(selectedLaneHightlightRef, destLaneNum);
            print("alpha= " + getLaneHightlightAlpha());
            selectedLaneHightlightRef.GetComponent<SpriteRenderer>().color = new Color(0, 255, 0, .2f);
            
            if (!tapAndReleaseMode)
            {
                toSwapCommando = getCommandoInLane(destLaneNum);            
                if (toSwapCommando != null && toMoveCommando.name != toSwapCommando.name)
                {
                    if (!allowSwaps)
                    {
                        selectedLaneHightlightRef.GetComponent<SpriteRenderer>().color = new Color(255, 0, 0, getLaneHightlightAlpha());
                        return;
                    }
                        
                    Vector3 dest = toSwapCommando.transform.position;
                    int moveDir = toMoveCommando.transform.position.y < toSwapCommando.transform.position.y ? 1 : -1;
                    dest.y = getLaneCenterY(findLaneNum(toSwapCommando.transform.position.y) - moveDir);
                    toSwapCommando.GetComponent<Commando>().Move(dest);
                }
            
                int selectedCommandoLaneNum = findLaneNum(toMoveCommando.transform.position.y);
                if (destLaneNum != selectedCommandoLaneNum)
                {
                    Vector3 dest = toMoveCommando.transform.position;
                    if (allowSwaps)
                    {
                        int moveDir = destLaneNum - selectedCommandoLaneNum;
                        moveDir = moveDir / Mathf.Abs(moveDir);
                        dest.y = getLaneCenterY(selectedCommandoLaneNum + moveDir);
                    } else
                        dest.y = getLaneCenterY(destLaneNum);
                    toMoveCommando.GetComponent<Commando>().Move(dest);
                
                    moveItemToLane(selectedCommandoGlyphRef, destLaneNum);
                }
            }
            
            moveItemToLane(selectedLaneHightlightRef, destLaneNum);
            if (useCommandoShadow)
                movingCommandoShadow.transform.position = new Vector3(-300, convertedMouse.y, 0);
        }

        //update score
        scoreText.GetComponent<UnityEngine.UI.Text>().text = "score: " + getScore();

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.LoadLevel("homeScene");
    }
    
    private float getLaneHightlightAlpha()
    {
        return 0.2f + .1f * Mathf.Sin(Time.time * 10);
    }
    
    private int getScore()
    {
        return Mathf.FloorToInt(distanceTraveled / 100);
    }

    private Vector3 move(Vector3 currentPosition, Vector3 dest, float distanceToMove)
    {
        if (currentPosition.y < dest.y)
            currentPosition.y = Mathf.Min(currentPosition.y + distanceToMove, dest.y);
        else
            currentPosition.y = Mathf.Max(currentPosition.y - distanceToMove, dest.y);

        return currentPosition;
    }

    private int findLaneNum(float y)
    {
        int laneNum = (int)Mathf.Floor((y - lanesStartY) / LANE_WIDTH);
        if (laneNum < 0)
            return 0;
        if (laneNum >= LANE_COUNT)
            return LANE_COUNT - 1;

        return laneNum;
    }
    
    private float getLaneCenterY(int laneNum)
    {
        return (laneNum * 100) + lanesStartY + LANE_WIDTH / 2;
    }
    
    private GameObject getCommandoInLane(int laneNum)
    {
        float laneCenterY = getLaneCenterY(laneNum);
        foreach (GameObject commando in commandos)
            if (commando.transform.position.y == laneCenterY)
                return commando;
        return null;
    }

    private void generateObstacles()
    {
        // check to generate or not
        int currentObstacleId = (int)Mathf.Floor(distanceTraveled / OBSTACLE_GENERATE_DISTANCE);
        if (currentObstacleId == lastGeneratedObstacleId)
            return;
            
        lastGeneratedObstacleId = currentObstacleId;
        if (lastGeneratedObstacleId < 10)
            OBSTACLE_COUNT = 1;
        else if (lastGeneratedObstacleId < 20)
            OBSTACLE_COUNT = 2;
        else if (lastGeneratedObstacleId < 30)
            OBSTACLE_COUNT = 3;

        currentSpeed = InitialSpeed + Mathf.Sqrt(2 * distanceTraveled);
        print("current speed: " + currentSpeed);
        // generate
        List<GameObject> currentList = new List<GameObject>();
        for (int i=0; i<Random.Range(Mathf.Max(1,OBSTACLE_COUNT-1), OBSTACLE_COUNT+1); i++)
        {
            float y;
            bool found;
            while (true)
            {
                y = getLaneCenterY(Random.Range(0, 6));
                found = false;
                foreach (GameObject obs in currentList)
                    if (obs.transform.position.y == y)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    break;
            }

            GameObject tempObs = (GameObject)Instantiate(Prefab_obs_indest, new Vector3(1200, y, 0), transform.rotation);
            currentList.Add(tempObs);
        }
    }

    public void gameOver()
    {
        Time.timeScale = 0;
        print("score:" + getScore());
        gameOverCanvasRef.SetActive(true);
        
    }

    public void submitHighscore()
    {
        string playerName = GameObject.Find("PlayerNameInput").GetComponent<UnityEngine.UI.InputField>().text.text;
        GameObject.Find("SubmitBtn").SetActive(false);
        GameObject.Find("PlayerNameInput").SetActive(false);
        HighScoreController hsController = GameObject.Find("HighScoreController").GetComponent<HighScoreController>();
        StartCoroutine(hsController.PostScores(playerName, getScore()));
    }

    public void restartGame()
    {
        Application.LoadLevel("gameScene");
    }

    public void loadHome()
    {
        Application.LoadLevel("homeScene");
    }

    private void moveItemToLane(GameObject item, int laneNum)
    {
        if (laneNum < 0)
        {
            item.SetActive(false);
            return;
        }

        item.SetActive(true);
        Vector3 oldPos = item.transform.position;
        item.transform.position = new Vector3(oldPos.x, getLaneCenterY(laneNum), oldPos.z);
    }

    private GameObject createCommandoShadow(GameObject commando)
    {
        GameObject shadow = (GameObject)Instantiate(commando);
        Color oldColor = shadow.GetComponent<SpriteRenderer>().color;
        shadow.GetComponent<SpriteRenderer>().color = new Color(oldColor.r, oldColor.g, oldColor.b, 0.5f);
        Destroy(shadow.GetComponent<BoxCollider2D>());
        Destroy(shadow.GetComponent<Animator>());
        Destroy(shadow.GetComponent<Commando>());

        return shadow;
    }
}