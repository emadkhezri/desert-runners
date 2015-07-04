using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public float fadeDuration = 1;
    
    private float startTime;
    private State currentState;
    private Color solidColor;
    private Color transparentColor;
    private IFaderListener listener;
    //private float fadeProgress;
    private string listenerParam;
    
    void Start()
    {
        moveIntoSight();
        
        Color currentColor = this.GetComponent<UnityEngine.UI.Image>().color;
        solidColor = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
        transparentColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0);
    }
    
    void Update()
    {
        if (currentState == State.FadeIn)
        {
            float progress = (Time.time - startTime) / fadeDuration;
            //float progress = ++fadeProgress / fadeDuration;
            this.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(solidColor, transparentColor, progress);
            if (progress >= 1)
            {
                currentState = State.Idle;                
                moveOutOfSight();
                if (listener != null)
                    listener.onFadeInDone();
            }
        } else if (currentState == State.FadeOut)
        {
            float progress = (Time.time - startTime) / fadeDuration;
            //float progress = ++fadeProgress / fadeDuration;
            this.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(transparentColor, solidColor, progress);
            if (progress >= 1)
            {
                currentState = State.Idle;
                moveOutOfSight();
                if (listener != null)
                    listener.onFadeOutDone(listenerParam);
            }
        }
    }
    
    public void FadeIn(IFaderListener listener)
    {
        //moveIntoSight();
        //fadeProgress = 0;
        this.listener = listener;
        startTime = Time.time;
        currentState = State.FadeIn;
    }
    
    public void FadeOut(IFaderListener listener, string param)
    {
        //moveIntoSight();
        this.listenerParam = param;
        //fadeProgress = 0;
        this.listener = listener;
        startTime = Time.time;
        currentState = State.FadeOut;
    }
    
    private void moveIntoSight()
    {
        this.transform.position = new Vector3(0, 0, 0);
    }
    
    private void moveOutOfSight()
    {
        //this.transform.Translate(-5000, 0, 0);
    }
    
    private enum State
    {
        Idle,
        FadeIn,
        FadeOut,
    }
}
