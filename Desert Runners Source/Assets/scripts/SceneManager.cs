using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour {

	void onUpdate()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}
}
