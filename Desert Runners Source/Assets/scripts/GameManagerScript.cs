using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class GameManagerScript : MonoBehaviour {


	private float COMMANDO_SPEED = 2000; //drag/swap

	private float SPEED = 150;

	private float LANE_WIDTH = 100;
	private float lanesStartY; // read from gameobject
	private int LANE_COUNT = 6;
	private float OBSTACLE_GENERATE_DISTANCE = 1000; 
	private int OBSTACLE_COUNT=1;
	// prefabs
	public GameObject Prefab_obs_indest; //set from editor

	public GameObject[] commandos;//set from editor

	private List<List<GameObject>> obstacles;

	private GameObject[] terrains; 

	private float translation;

	private bool moveCommando = false;
	private GameObject toMoveCommando;
	private GameObject toSwapCommando;
	private float moveCommandoDestinationY;
	private float swapCommandoDestinationY;
	private float moveCommandoDist;

	private float distanceTraveled = 0;

	private int lastObstacleIndexGenerated = -1;

	public GUIStyle scoreStyle;

	private GameObject gameOverCanvasRef;

	// Use this for initialization
	void Start ()
	{
		Time.timeScale = 1;

		lanesStartY = GameObject.Find ("LaneGuide").transform.position.y;

		// set initial commando positions
		for (int i=0; i<3; i++)
			commandos [i].transform.position = new Vector3 (-400, getLaneCenterY (i + 1), 0);

		toMoveCommando = null;

		terrains = new GameObject[2];
		terrains[0] = GameObject.Find ("Terrain1");
		terrains[1] = GameObject.Find ("Terrain2");

		obstacles = new List<List<GameObject>>();

		gameOverCanvasRef = GameObject.Find ("GameOverCanvas");
		gameOverCanvasRef.SetActive (false);
	}

	void OnGUI ()
	{
		// Make a background box
		GUI.Label(new Rect(1024-150,15,150,Screen.height), "score: "+Mathf.FloorToInt(distanceTraveled/100),scoreStyle);

	}

	// Update is called once per frame
	void Update ()
	{
		if (Time.timeScale == 0)
						return;

		float elapsedTime = Time.deltaTime;

		translation = elapsedTime * SPEED;
		distanceTraveled += translation;

		generateObstacles ();

		// move
				// obstacles
				foreach (var obsList in obstacles)
						foreach (var obs in obsList)
								obs.transform.Translate (-translation, 0, 0);

				// terrains
				if (terrains [0].transform.position.x <= -2048)
					terrains [0].transform.position = new Vector3 (terrains [1].transform.position.x + 2048, terrains [1].transform.position.y, terrains [1].transform.position.z);
				if (terrains [1].transform.position.x <= -2048)
					terrains [1].transform.position = new Vector3 (terrains [0].transform.position.x + 2048, terrains [0].transform.position.y, terrains [0].transform.position.z);

				terrains [0].transform.Translate (-translation, 0, 0);
				terrains [1].transform.Translate (-translation, 0, 0);

		// destroy out of screen obstacles 
		if (obstacles.Count > 0 && obstacles[0][0].transform.position.x < -512-50) // check first list, first item
		{
			foreach (var obs in obstacles[0])
					Destroy(obs);
			obstacles.Remove(obstacles[0]);
		}


		// drag/swap
		if (moveCommando)
		{
			/*toMoveCommando.transform.position = Vector3.Lerp(toMoveCommando.transform.position
			                                                 ,new Vector3(toMoveCommando.transform.position.x,moveCommandoDestinationY,toMoveCommando.transform.position.z)		                                                 
			                                                 ,Time.deltaTime*MOVE_SPEED);*/
			Vector3 currentPosition = toMoveCommando.transform.position;
			Vector3 dest = new Vector3(currentPosition.x, moveCommandoDestinationY, currentPosition.z);

			toMoveCommando.transform.position = move(currentPosition, dest, elapsedTime * COMMANDO_SPEED);

			if(toSwapCommando != null)
			{
				currentPosition = toSwapCommando.transform.position;
				dest.y = swapCommandoDestinationY;
				toSwapCommando.transform.position = move(currentPosition, dest, elapsedTime * COMMANDO_SPEED);
			}

			if ( toMoveCommando.transform.position.y == moveCommandoDestinationY)
			{
				toMoveCommando.transform.position = new Vector3(toMoveCommando.transform.position.x, moveCommandoDestinationY, toMoveCommando.transform.position.z);
				if(toSwapCommando != null)
					toSwapCommando.transform.position = new Vector3(toSwapCommando.transform.position.x, swapCommandoDestinationY, toSwapCommando.transform.position.z);
				moveCommando = false;
				toSwapCommando = null;
			}
		}
		else if (Input.GetMouseButtonDown (0))
		{
			// find lane
			Vector3 convertedMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			int laneNum = findLaneNum(convertedMouse.y);
			//prtoMoveCommando			
			toMoveCommando = getCommandoInLane(laneNum);
			//print("toMoveCommando:"+toMoveCommando.name);
		}
		else if (toMoveCommando != null && Input.GetMouseButtonUp (0))
		{
			// find lane
			Vector3 convertedMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			int laneNum = findLaneNum(convertedMouse.y);
			if ( laneNum == -1 )
				return;

			toSwapCommando = getCommandoInLane(laneNum);
			moveCommandoDestinationY = getLaneCenterY(laneNum);
			swapCommandoDestinationY = toMoveCommando.transform.position.y;
			moveCommandoDist = Mathf.Abs(moveCommandoDestinationY-toMoveCommando.transform.position.y);
			moveCommando = true;
		}
	}

	private Vector3 move(Vector3 currentPosition, Vector3 dest, float distanceToMove)
	{
		if(currentPosition.y < dest.y)
			currentPosition.y = Mathf.Min (currentPosition.y + distanceToMove, dest.y);
		else
			currentPosition.y = Mathf.Max (currentPosition.y - distanceToMove, dest.y);

		return currentPosition;
	}

	private int findLaneNum(float y)
	{
		int laneNum = (int)Mathf.Floor ( (y - lanesStartY) / LANE_WIDTH );
		if (laneNum < 0 || laneNum >= LANE_COUNT)
			return -1;

		return laneNum;
	}
	private float getLaneCenterY(int laneNum) // 0..
	{
		return (laneNum * 100) + lanesStartY + LANE_WIDTH/2;
	}
	private GameObject getCommandoInLane(int laneNum)
	{
		float laneCenterY = getLaneCenterY (laneNum);
		foreach (GameObject commando in commandos)
						if (commando.transform.position.y == laneCenterY)
								return commando;
		return null;
	}
	private void generateObstacles()
	{
		// check to generate or not
		int currentObstIndex = (int)Mathf.Floor (distanceTraveled / OBSTACLE_GENERATE_DISTANCE);
		if ( currentObstIndex == lastObstacleIndexGenerated)
			return;
		lastObstacleIndexGenerated = currentObstIndex;
		if (lastObstacleIndexGenerated < 10)
						OBSTACLE_COUNT = 1;
				else if (lastObstacleIndexGenerated < 20)
						OBSTACLE_COUNT = 2;
				else if (lastObstacleIndexGenerated < 30)
						OBSTACLE_COUNT = 3;

		SPEED += 20;
		print ("SPEED:"+SPEED);
		// generate
		List<GameObject> currentList = new List<GameObject> ();
		obstacles.Add (currentList);

		for (int i=0; i<Random.Range(Mathf.Max(1,OBSTACLE_COUNT-1), OBSTACLE_COUNT+1); i++) {
				float y;
				bool found;
				while (true) {
						y = getLaneCenterY (Random.Range (0, 6));
						found = false;
						foreach (GameObject obs in currentList)
								if (obs.transform.position.y == y) {
										found = true;
										break;
								}
						if (!found)
								break;
				}

				GameObject tempObs = (GameObject)Instantiate (Prefab_obs_indest, new Vector3 (1200, y, 0), transform.rotation);
				currentList.Add (tempObs);
		}
	}

	public void gameOver()
	{
		Time.timeScale = 0;
		print ("score:"+distanceTraveled);
		// TODO: show score
		gameOverCanvasRef.SetActive(true);
	}

	public void restartGame()
	{
		Application.LoadLevel ("gameScene");
	}

	public void loadHome()
	{
		Application.LoadLevel ("homeScene");
	}
}