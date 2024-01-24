using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

    public class PlayersOverheadDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text displayNameText;

        public void Awake()
        {
            displayNameText.text = gameObject.name;
        }
    }
