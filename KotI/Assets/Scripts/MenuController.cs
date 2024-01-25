using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Slider colorR;
    public Slider colorG;
    public Slider colorB;

    void Start()
    {
        colorR.onValueChanged.AddListener((v) =>
        {
            GameObject.Find("Player Mesh").GetComponent<SkinnedMeshRenderer>().materials[1].SetColor("_Color", new Color(v, GameObject.Find("Player Mesh").GetComponent<SkinnedMeshRenderer>().materials[1].color.g, GameObject.Find("Player Mesh").GetComponent<SkinnedMeshRenderer>().materials[1].color.b));
        });
        colorG.onValueChanged.AddListener((v) =>
        {
            GameObject.Find("Player Mesh").GetComponent<SkinnedMeshRenderer>().materials[1].SetColor("_Color", new Color(GameObject.Find("Player Mesh").GetComponent<SkinnedMeshRenderer>().materials[1].color.r, v, GameObject.Find("Player Mesh").GetComponent<SkinnedMeshRenderer>().materials[1].color.b));
        });
        colorB.onValueChanged.AddListener((v) =>
        {
            GameObject.Find("Player Mesh").GetComponent<SkinnedMeshRenderer>().materials[1].SetColor("_Color", new Color(GameObject.Find("Player Mesh").GetComponent<SkinnedMeshRenderer>().materials[1].color.r, GameObject.Find("Player Mesh").GetComponent<SkinnedMeshRenderer>().materials[1].color.g, v));
        });
    }
}
