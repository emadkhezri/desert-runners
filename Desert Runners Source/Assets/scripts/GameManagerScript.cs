using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class GameManagerScript : MonoBehaviour
{


    //private float COMMANDO_SPEED = 2000; //drag/swap

    private float SPEED = 150;

    private float LANE_WIDTH = 100;
    private float lanesStartY; // read from gameobject
    private int LANE_COUNT = 6;
    private float OBSTACLE_GENERATE_DISTANCE = 1000; 
    private int OBSTACLE_COUNT = 1;
    // prefabs
    public GameObject Prefab_obs_indest; //set from editor

    public GameObject[] commandos;//set from editor

    private List<List<GameObject>> obstacles;

    private GameObject[] terrains; 

    private float translation;

    private GameObject toMoveCommando;
    private GameObject toSwapCommando;
    private GameObject movingCommandoShadow;

    private float distanceTraveled = 0;

    private int lastObstacleIndexGenerated = -1;

    public GUIStyle scoreStyle;

    private GameObject gameOverCanvasRef;
    public GameObject scoreText;
    public GameObject selectedCommandoGlyphRef;
    public GameObject selectedLaneHightlightRef;

    private bool draggingCommando;

    // Use this for initialization
    void Start()
    {
        Time.timeScale = 1;

        lanesStartY = GameObject.Find("LaneGuide").transform.position.y;

        // set initial commando positions
        for (int i=0; i<3; i++)
            commandos [i].transform.position = new Vector3(-400, getLaneCenterY(i + 1), 0);

        toMoveCommando = null;

        terrains = new GameObject[2];
        terrains [0] = GameObject.Find("Terrain1");
        terrains [1] = GameObject.Find("Terrain2");

        obstacles = new List<List<GameObject>>();

        gameOverCanvasRef = GameObject.Find("GameOverCanvas");
        gameOverCanvasRef.SetActive(false);
		
        selectedCommandoGlyphRef.transform.position = new Vector3(-400, 0, 0);
        moveItemToLane(selectedCommandoGlyphRef, -1);
		
        selectedLaneHightlightRef.transform.position = new Vector3(0, 0, 0);
        moveItemToLane(selectedLaneHightlightRef, -1);

        draggingCommando = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0)
            return;

        float elapsedTime = Time.deltaTime;

        translation = elapsedTime * SPEED;
        distanceTraveled += translation;

        generateObstacles();

        // move
        // obstacles
        foreach (var obsList in obstacles)
            foreach (var obs in obsList)
                obs.transform.Translate(-translation, 0, 0);

        // terrains
        if (terrains [0].transform.position.x <= -2048)
            terrains [0].transform.position = new Vector3(terrains [1].transform.position.x + 2048, terrains [1].transform.position.y, terrains [1].transform.position.z);
        if (terrains [1].transform.position.x <= -2048)
            terrains [1].transform.position = new Vector3(terrains [0].transform.position.x + 2048, terrains [0].transform.position.y, terrains [0].transform.position.z);

        terrains [0].transform.Translate(-translation, 0, 0);
        terrains [1].transform.Translate(-translation, 0, 0);

        // destroy out of screen obstacles 
        if (obstacles.Count > 0 && obstacles [0] [0].transform.position.x < -512 - 50) // check first list, first item
        {
            foreach (var obs in obstacles[0])
                Destroy(obs);
            obstacles.Remove(obstacles [0]);
        }

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

                movingCommandoShadow = createCommandoShadow(toMoveCommando);
            }
        } else if (toMoveCommando != null && Input.GetMouseButtonUp(0))
        {
            if (draggingCommando)
            {
                draggingCommando = false;
                moveItemToLane(selectedLaneHightlightRef, -1);
                Destroy(movingCommandoShadow);
            }
            
            Vector3 convertedMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int laneNum = findLaneNum(convertedMouse.y);
			
            toSwapCommando = getCommandoInLane(laneNum);            
            if (toSwapCommando != null)
                toSwapCommando.GetComponent<Commando>().Move(toMoveCommando.transform.position);
            
            Vector3 toMoveCommandoDest = toMoveCommando.transform.position;
            toMoveCommandoDest.y = getLaneCenterY(laneNum);
            toMoveCommando.GetComponent<Commando>().Move(toMoveCommandoDest);
			
            moveItemToLane(selectedCommandoGlyphRef, -1);
        } else if (draggingCommando)
        {
            Vector3 convertedMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int destLaneNum = findLaneNum(convertedMouse.y);
            
            /*toSwapCommando = getCommandoInLane(destLaneNum);            
            if (toSwapCommando != null && toMoveCommando.name != toSwapCommando.name)
            {
                Vector3 dest = toSwapCommando.transform.position;
                int moveDir = toMoveCommando.transform.position.y < toSwapCommando.transform.position.y ? 1 : -1;
                dest.y = getLaneCenterY(findLaneNum(toSwapCommando.transform.position.y) - moveDir);
                toSwapCommando.GetComponent<Commando>().Move(dest);
            }
            
            int selectedCommandoLaneNum = findLaneNum(toMoveCommando.transform.position.y);
            if (destLaneNum != selectedCommandoLaneNum)
            {
                int moveDir = destLaneNum - selectedCommandoLaneNum;
                moveDir = moveDir / Mathf.Abs(moveDir);
                Vector3 dest = toMoveCommando.transform.position;
                dest.y = getLaneCenterY(selectedCommandoLaneNum + moveDir);
                toMoveCommando.GetComponent<Commando>().Move(dest);
                
                moveItemToLane(selectedCommandoGlyphRef, destLaneNum);
            }*/
            
            moveItemToLane(selectedLaneHightlightRef, destLaneNum);
            movingCommandoShadow.transform.position = new Vector3(-300, convertedMouse.y, 0);
        }

        //update score
        scoreText.GetComponent<UnityEngine.UI.Text>().text = "score: " + Mathf.FloorToInt(distanceTraveled / 100);

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.LoadLevel("homeScene");
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
    
    private float getLaneCenterY(int laneNum) // 0..
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
        int currentObstIndex = (int)Mathf.Floor(distanceTraveled / OBSTACLE_GENERATE_DISTANCE);
        if (currentObstIndex == lastObstacleIndexGenerated)
            return;
        lastObstacleIndexGenerated = currentObstIndex;
        if (lastObstacleIndexGenerated < 10)
            OBSTACLE_COUNT = 1;
        else if (lastObstacleIndexGenerated < 20)
            OBSTACLE_COUNT = 2;
        else if (lastObstacleIndexGenerated < 30)
            OBSTACLE_COUNT = 3;

        SPEED += 10;
        print("SPEED:" + SPEED);
        // generate
        List<GameObject> currentList = new List<GameObject>();
        obstacles.Add(currentList);

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
        print("score:" + distanceTraveled);
        // TODO: show score
        gameOverCanvasRef.SetActive(true);
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