using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour, IFaderListener
{
    private ScreenFader screenFader;
    
    void Start()
    {
        Time.timeScale = 1;
        
        screenFader = GameObject.Find("ScreenFader").GetComponent<ScreenFader>();
        screenFader.FadeIn(null);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
    
    public void startGame()
    {
        screenFader.FadeOut(this, null);
    }
    
    public void onFadeOutDone(string param)
    {
        Application.LoadLevel("gameScene");
    }
    
    public void onFadeInDone()
    {
    }
    
}
