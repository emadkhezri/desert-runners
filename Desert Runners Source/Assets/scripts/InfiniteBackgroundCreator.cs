using UnityEngine;
using System.Collections;

public class InfiniteBackgroundCreator : MonoBehaviour
{
    public float speed;
	
    void Update()
    {
        this.transform.Translate(-speed * Time.deltaTime, 0, 0);
        if (this.transform.position.x < -2048)
        {
            Vector3 currentPos = this.transform.position;
            this.transform.position = new Vector3(currentPos.x + 4096, currentPos.y, currentPos.z);
        }
            
    }
}
