using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    // Start is called before the first frame update
    // Method to change the scene by name
    public InputField usernameInput;
    public InputField ipInput;
    public Toggle isServerButton;
    public void SceneChange(string sceneName)
    {
        string username = usernameInput.text;
        if (string.IsNullOrEmpty(username)) username = "Unknown Username";
        string ip = ipInput.text;
        if (string.IsNullOrEmpty(ip)) ip= "127.0.0.1";
        int isServer = isServerButton.isOn ? 1 : 0;


        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("ipAddress", ip);
        PlayerPrefs.SetInt("isServer", isServer);
        Debug.Log(PlayerPrefs.GetString("username") + PlayerPrefs.GetString("ipAddress") + PlayerPrefs.GetInt("isServer"));
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
