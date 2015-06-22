using UnityEngine;
using System.Collections;

public class LaneHighlight : MonoBehaviour
{
    public float glowSpeed;
    
    void Start()
    {
        Color previousColor = this.GetComponent<SpriteRenderer>().color;
        this.GetComponent<SpriteRenderer>().color = new Color(previousColor.r, previousColor.g, previousColor.b, 0.2f);
    }
	
    void Update()
    {
        Color previousColor = this.GetComponent<SpriteRenderer>().color;
        this.GetComponent<SpriteRenderer>().color = new Color(previousColor.r, previousColor.g, previousColor.b, 0.2f + .1f * Mathf.Sin(Time.time * glowSpeed));
    }
}
