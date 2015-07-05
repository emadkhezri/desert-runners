using UnityEngine;
using System.Collections;
using System.IO;

public class SettingsFile
{
    public bool enableRTL = false;
    public bool enableMusic = true;
    public bool enableSFX = true;
    private string fileName;
    
    public SettingsFile(string fileName)
    {
        this.fileName = fileName;
        try
        {
            load();
        } catch (System.Exception)
        {
            save();
        }
    }
    
    /**
    * @throws Exception
    */
    public void save()
    {
        string[] lines = new string[3];
        
        lines [0] = enableRTL.ToString();
        lines [1] = enableMusic.ToString();
        lines [2] = enableSFX.ToString();
        
        File.WriteAllLines(fileName, lines);
    }
    
    /**
    * @throws Exception
    */
    public void load()
    {
        string[] lines = File.ReadAllLines(fileName);
        enableRTL = bool.Parse(lines [0]);
        enableMusic = bool.Parse(lines [1]);
        enableSFX = bool.Parse(lines [2]);
    }
}
