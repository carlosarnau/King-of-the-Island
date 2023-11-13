using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    // Start is called before the first frame update
    // Method to change the scene by name
    public void SceneChange(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
