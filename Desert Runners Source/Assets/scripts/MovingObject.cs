using UnityEngine;
using System.Collections;

public class MovingObject : MonoBehaviour
{
    
    public float speed;
    public Vector3 initialPos;
    public float leftHorizontalBound;
    
    void Start()
    {
        this.transform.position = new Vector3(initialPos.x, initialPos.y, initialPos.z);
    }    
    
    void Update()
    {
        this.transform.Translate(-speed * Time.deltaTime, 0, 0);
        if (this.transform.position.x < leftHorizontalBound)
            this.transform.position = new Vector3(initialPos.x, initialPos.y, initialPos.z);
    }
    
}
