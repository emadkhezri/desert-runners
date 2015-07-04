using UnityEngine;
using System.Collections;
using System.IO;

public class SettingsFile
{
    private string fileName;
    
    public SettingsFile(string fileName)
    {
        this.fileName = fileName;
    }
    
    public bool RTL
    {
        get;
        set;
    }
    
    /**
    * @throws Exception
    */
    public void save()
    {
        File.WriteAllText(fileName, RTL.ToString());
    }
    
    /**
    * @throws Exception
    */
    public void load()
    {
        RTL = bool.Parse(File.ReadAllText(fileName));
    }
}
