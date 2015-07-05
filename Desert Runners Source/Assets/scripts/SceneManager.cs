﻿using UnityEngine;
using System.Collections;
using System.IO;

public class SceneManager : MonoBehaviour, IFaderListener
{
    private ScreenFader screenFader;
    private GameObject settingsPanel;
    private GameObject hiscoresPanel;
    private SettingsFile settingsFile;
    private UnityEngine.UI.Toggle chkRTL;
    
    void Start()
    {
        Time.timeScale = 1;
        
        chkRTL = GameObject.Find("ChkRTL").GetComponent<UnityEngine.UI.Toggle>();
        
        try
        {
            settingsFile = new SettingsFile("settings.dat");
            settingsFile.load();
            chkRTL.isOn = settingsFile.RTL;
            
        } catch (System.Exception ex)
        {
            print("Failed to load settings. ex= " + ex.Message);
        }        
        
        settingsPanel = GameObject.Find("SettingsPanel");
        hiscoresPanel = GameObject.Find("HiscoresPanel");
        hideHiscores();
        hideSettings();
        
        screenFader = GameObject.Find("ScreenFader").GetComponent<ScreenFader>();
        screenFader.FadeIn(null);
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
    
    public void startGame()
    {
        hideSettings();
        hideHiscores();
        GameObject.Find("MainPanel").SetActive(false);
        GameObject.Find("Title").SetActive(false);
        
        screenFader.FadeOut(this, null);
    }
    
    public void onFadeOutDone(string param)
    {
        Application.LoadLevel("gameScene");
    }
    
    public void onFadeInDone()
    {
    }
    
    public void showSettings()
    {
        hideHiscores();
        settingsPanel.SetActive(true);
    }
    
    public void hideSettings()
    {
        settingsPanel.SetActive(false);
    }
    
    public void showHiscores()
    {
        hideSettings();
        hiscoresPanel.SetActive(true);
        refreshHiscores();
    }
    
    public void hideHiscores()
    {
        hiscoresPanel.SetActive(false);
    }
    
    public void refreshHiscores()
    {
        HighScoreController hsc = GameObject.Find("HighScoreController").GetComponent<HighScoreController>();
        StartCoroutine(hsc.GetScores());
    }
    
    public void saveSettings()
    {
        settingsFile.RTL = chkRTL.isOn;
        try
        {
            settingsFile.save();
        } catch (System.Exception ex)
        {
            print("Failed to save settings. ex= " + ex.Message);
        }
    }
    
}
