using TMPro;
using UnityEngine;

public class PlayersOverheadDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayNameText;

    public void Awake()
    {
        displayNameText.text = gameObject.name;
    }
}
