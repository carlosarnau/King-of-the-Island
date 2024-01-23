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
    public MenuController _menu;

    public void Start()
    {
        PlayerPrefs.SetString("ClientStatus", ClientStatus.Menu.ToString());
        _menu = GameObject.Find("MenuController").GetComponent<MenuController>();
    }

    public void SceneChange(string sceneName)
    {
        string username = usernameInput.text;
        if (string.IsNullOrEmpty(username)) username = "Unknown Username";
        string ip = ipInput.text;
        if (string.IsNullOrEmpty(ip)) ip = "127.0.0.1";
        int isServer = isServerButton.isOn ? 1 : 0;


        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("ipAddress", ip);
        PlayerPrefs.SetInt("isServer", isServer);
        PlayerPrefs.SetString("ClientStatus", ClientStatus.Ingame.ToString());
        PlayerPrefs.SetFloat("ColorR", _menu.colorR.value);
        PlayerPrefs.SetFloat("ColorG", _menu.colorG.value);
        PlayerPrefs.SetFloat("ColorB", _menu.colorB.value);

        //Debug.Log(PlayerPrefs.GetString("username") + PlayerPrefs.GetString("ipAddress") + PlayerPrefs.GetInt("isServer"));
        SceneManager.LoadScene(sceneName);

    }

    public enum ClientStatus
    {
        Menu,
        Ingame,
        Disconnected
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
