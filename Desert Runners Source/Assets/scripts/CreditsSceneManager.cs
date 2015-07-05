using UnityEngine;
using System.Collections;

public class CreditsSceneManager : MonoBehaviour
{

    void Start()
    {
        GameObject.Find("ScreenFader").GetComponent<ScreenFader>().FadeIn(null);
    }
	
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
            loadHome();
    }
    
    public void loadHome()
    {
        Application.LoadLevel("homeScene");
    }
}
