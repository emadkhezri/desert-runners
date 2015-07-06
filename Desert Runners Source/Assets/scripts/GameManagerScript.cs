using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.IO;

public class GameManagerScript : MonoBehaviour, IFaderListener
{
    
    public GameObject Prefab_obs_indest;
    public GameObject[] commandos;
    public GameObject selectedCommandoGlyphRef;
    public GameObject selectedLaneHightlightRef;
    public GameObject[] terrains;
    public GameObject scoreText;
    public GameObject backGroundMusic;
    
    //Settings
    public bool tapAndReleaseMode = false;
    public bool allowSwaps = false;
    public bool useCommandoShadow = false;
    private SettingsFile settingsFile;
    
    //properties
    public float CurrentMovementSpeed
    {
        get { return currentSpeed;}
    }
    
    public SettingsFile SettingsFile
    {
        get { return settingsFile;}
    }
    
    //private fields
    private const float InitialSpeed = 150;
    private float currentSpeed = InitialSpeed;
    private float LANE_WIDTH = 100;
    private float lanesStartY; // read from gameobject
    private int LANE_COUNT = 6;
    private float OBSTACLE_GENERATE_DISTANCE = 1000; 
    private int OBSTACLE_COUNT = 1;
    private GameObject toMoveCommando;
    private GameObject toSwapCommando;
    private GameObject movingCommandoShadow;
    private float distanceTraveled = 0;
    private int lastGeneratedObstacleId = -1;
    private bool draggingCommando;
    private Sprite[] obstacleSprites;
    private ScreenFader screenFader;
    private bool isGameStopped = false;
    
    //UI elements
    private GameObject gameOverCanvasRef;
    private GameObject pauseButton;
    private GameObject pausePanel;
    public GameObject hiscoresPanel;
    public GameObject prefabCountDown;

    void Start()
    {
        print(Application.persistentDataPath);
        Screen.fullScreen = true;
        //Time.timeScale = 1;
        
        settingsFile = new SettingsFile(Path.Combine(Application.persistentDataPath, "settings.dat"));
        try
        {
            settingsFile.load();
        } catch (System.Exception ex)
        {
            print("Failed to load settings. ex= " + ex.Message);
        }
        
        if (settingsFile.enableRTL)
        {
            Matrix4x4 mat = Camera.main.projectionMatrix;
            mat *= Matrix4x4.Scale(new Vector3(-1.0f, 1f, 1f));
            Camera.main.projectionMatrix = mat;
        }
        if (!settingsFile.enableMusic)
            backGroundMusic.gameObject.SetActive(false);

        lanesStartY = GameObject.Find("LaneGuide").transform.position.y;

        // set initial commando positions
        for (int i=0; i<3; i++)
            commandos [i].transform.position = new Vector3(-400, getLaneCenterY(i + 1), 0);

        toMoveCommando = null;
		
        selectedCommandoGlyphRef.transform.position = new Vector3(-400, 0, 0);
        moveItemToLane(selectedCommandoGlyphRef, -1);
		
        selectedLaneHightlightRef.transform.position = new Vector3(0, 0, 0);
        moveItemToLane(selectedLaneHightlightRef, -1);

        draggingCommando = false;
        
        obstacleSprites = Resources.LoadAll<Sprite>("ObstacleSprites");

        //Init UI
        gameOverCanvasRef = GameObject.Find("GameOverCanvas");
        gameOverCanvasRef.SetActive(false);
        
        pauseButton = GameObject.Find("PauseButton");
        scoreText.SetActive(false);
        pauseButton.SetActive(false);
        
        pausePanel = GameObject.Find("PausePanel");
        hidePausePanel();
        
        //hiscoresPanel = GameObject.Find("HiscoresPanel");
        hideHiscoresPanel();
        
        //prefabCountDown = GameObject.Find("prefabCountDown");
        //
        screenFader = GameObject.Find("ScreenFader").GetComponent<ScreenFader>();
        isGameStopped = true;
        screenFader.FadeIn(this);
    }

    void Update()
    {
        if (isGameStopped)
            return;

        float translation = Time.deltaTime * currentSpeed;
        distanceTraveled += translation;

        generateObstacles();

        // terrains
        if (terrains [0].transform.position.x <= -4096)
            terrains [0].transform.position = new Vector3(terrains [1].transform.position.x + 4095, terrains [1].transform.position.y, terrains [1].transform.position.z);
        if (terrains [1].transform.position.x <= -4096)
            terrains [1].transform.position = new Vector3(terrains [0].transform.position.x + 4095, terrains [0].transform.position.y, terrains [0].transform.position.z);

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
            selectedLaneHightlightRef.GetComponent<SpriteRenderer>().color = Color.green;
            
            if (!tapAndReleaseMode)
            {
                toSwapCommando = getCommandoInLane(destLaneNum);            
                if (toSwapCommando != null && toMoveCommando.name != toSwapCommando.name)
                {
                    if (!allowSwaps)
                    {
                        selectedLaneHightlightRef.GetComponent<SpriteRenderer>().color = Color.red;
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
        scoreText.GetComponent<UnityEngine.UI.Text>().text = "" + getScore();

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.LoadLevel("homeScene");
    }
    
    public bool IsGameOver()
    {
        return isGameStopped;
    }
    
    private float getLaneHightlightAlpha()
    {
        return 0.2f + .1f * Mathf.Sin(Time.time * 3);
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
            
            //randomize obstacle image
            Prefab_obs_indest.GetComponent<SpriteRenderer>().sprite = obstacleSprites [Random.Range(0, obstacleSprites.Length - 1)];
            GameObject tempObs = (GameObject)Instantiate(Prefab_obs_indest, new Vector3(1200, y, 0), transform.rotation);
            currentList.Add(tempObs);
        }
    }

    private IEnumerator showGameOverMenu()
    {
        scoreText.SetActive(false);
        pauseButton.SetActive(false);
        yield return new WaitForSeconds(.5f);
        gameOverCanvasRef.SetActive(true);
        
        GameObject.Find("GameOverScoreText").GetComponent<UnityEngine.UI.Text>().text = "" + getScore();
        GameObject.Find("GameOverPersonalBest").GetComponent<UnityEngine.UI.Text>().text = "" + getPersonalBest();
    }

    public void gameOver()
    {
        StartCoroutine(showGameOverMenu());
        backGroundMusic.GetComponent<AudioSource>().Stop();
        if (settingsFile.enableSFX)
            GameObject.Find("GameOverSound").GetComponent<AudioSource>().Play();
        this.isGameStopped = true;
    }
    
    public void onFadeOutDone(string param)
    {
        Application.LoadLevel(param);
    }
    
    public void onFadeInDone()
    {
        StartCoroutine(startCountDown());
    }
    
    private IEnumerator startCountDown()
    {
        Sprite[] numberSprites = Resources.LoadAll<Sprite>("Numbers");
        GameObject countDownImage;
        for (int i=numberSprites.Length; i>0; --i)
        {
            prefabCountDown.GetComponent<SpriteRenderer>().sprite = numberSprites [i - 1];
            countDownImage = Instantiate(prefabCountDown);
            countDownImage.transform.position = new Vector3(0, 0, 0);
            yield return new WaitForSeconds(1);
            Destroy(countDownImage);
        }
        
        scoreText.SetActive(true);
        pauseButton.SetActive(true);
        this.isGameStopped = false;
    }

    public void submitHighscore()
    {
        showHiscoresPanel();
        string playerName = GameObject.Find("PlayerNameInput").GetComponent<UnityEngine.UI.InputField>().text;
        GameObject.Find("SubmitBtn").SetActive(false);
        GameObject.Find("PlayerNameInput").SetActive(false);
        HighScoreController hsController = GameObject.Find("HighScoreController").GetComponent<HighScoreController>();
        StartCoroutine(hsController.PostScores(playerName, getScore()));
    }
    
    public void refreshHiscores()
    {
        HighScoreController hsController = GameObject.Find("HighScoreController").GetComponent<HighScoreController>();
        StartCoroutine(hsController.GetScores());
    }

    public void restartGame()
    {
        hidePausePanel();
        gameOverCanvasRef.SetActive(false);
        screenFader.FadeOut(this, "gameScene");
    }

    public void loadHome()
    {
        hidePausePanel();
        gameOverCanvasRef.SetActive(false);
        screenFader.FadeOut(this, "homeScene");
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
    
    private int getPersonalBest()
    {
        string fileName = Path.Combine(Application.persistentDataPath, "personalBest.dat");
        try
        {
            if (!File.Exists(fileName))
            {
                File.WriteAllText(fileName, getScore().ToString());
                return getScore();
            }
                
            int personalBest = int.Parse(File.ReadAllText(fileName));
            if (getScore() > personalBest)
            {
                File.WriteAllText(fileName, getScore().ToString());
                return getScore();
            }
            
            return personalBest;
        } catch (System.Exception ex)
        {
            print("An exception occurred. ex= " + ex.Message);
            return getScore();
        }
    }
    
    public void pauseGame()
    {
        pauseButton.SetActive(false);
        backGroundMusic.GetComponent<AudioSource>().Pause();
        isGameStopped = true;
        showPausePanel();
        Time.timeScale = 0;
    }
    
    public void resumeGame()
    {
        pauseButton.SetActive(true);
        backGroundMusic.GetComponent<AudioSource>().UnPause();
        hidePausePanel();
        isGameStopped = false;
        Time.timeScale = 1;
    }
    
    public void hidePausePanel()
    {
        pausePanel.SetActive(false);
    }
    
    public void showPausePanel()
    {
        pausePanel.SetActive(true);
    } 
    
    public void hideHiscoresPanel()
    {
        hiscoresPanel.SetActive(false);
    }
    
    public void showHiscoresPanel()
    {
        hiscoresPanel.SetActive(true);
    } 
}