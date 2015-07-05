using UnityEngine;
using System.Collections;

public class AbAnbeStudiosSceneManager : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(loadHome());	
    }
    
    private IEnumerator loadHome()
    {
        yield return new WaitForSeconds(1);
        
        Application.LoadLevel("homeScene");
    }
	
}
