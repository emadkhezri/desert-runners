using UnityEngine;
using System.Collections;

public class InfiniteBackgroundCreator : MonoBehaviour
{
    public float speed;
    public Vector3 initialPos;
    public float patternLen;
    public GameObject[] backgroundItems;
	
    void Start()
    {
        for (int i=0; i<backgroundItems.Length; ++i)
            backgroundItems [i].transform.position = new Vector3(initialPos.x + i * patternLen, initialPos.y, initialPos.z);
    }    
    
    void Update()
    {
        for (int i=0; i<backgroundItems.Length; ++i)
            moveBackgroundItem(backgroundItems [i]);
    }
    
    private void moveBackgroundItem(GameObject gameObj)
    {
        gameObj.transform.Translate(-speed * Time.deltaTime, 0, 0);
        if (gameObj.transform.position.x < -patternLen)
        {
            Vector3 currentPos = gameObj.transform.position;
            print("moving to: " + currentPos.x + backgroundItems.Length * patternLen);
            gameObj.transform.position = new Vector3(currentPos.x + backgroundItems.Length * patternLen, initialPos.y, initialPos.z);
        }
    }
}
