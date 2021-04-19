using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopPanelUI : MonoBehaviour
{
    public Text playerNameText;
    private String playerName;
    public Slider helthBar;
    private float healthBarValue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerNameText.text = playerName;
        helthBar.value = healthBarValue;
    }

    public string PlayerName
    {
        get => playerName;
        set => playerName = value;
    }
    
    public float HealthBarValue
    {
        get => healthBarValue;
        set => healthBarValue = value;
    }
}
