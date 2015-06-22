using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
    
    public void startGame()
    {
        Application.LoadLevel("gameScene");
    }
}
