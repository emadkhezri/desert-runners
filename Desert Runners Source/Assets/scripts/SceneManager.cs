using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour
{

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
