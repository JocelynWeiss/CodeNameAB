using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class JowLogger
{
    static public StreamWriter m_writer;
    static public bool m_logToFile = true;
    static public bool m_logToLog = true;
    static public bool m_logTime = false;
    static public bool m_addTimeToName = false;
    static public string m_logFileName = "AppLog";


    public JowLogger()
    {
    }


    // Create a file in persistentDataPath (this will work on Android devices)
    static private void CreateFile()
    {
        if (m_writer != null)
            return;

        //string fileName = "AppLog.txt";
        string fileName = m_logFileName;

        // To avoid collision with multiple app instance we add the time to the name
        if (m_addTimeToName == true)
            fileName += System.DateTime.Now.ToString("mmss");

        fileName += ".txt";

        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log($"Creating log file {filePath}");
        m_writer = new StreamWriter(filePath, false);

        string introTxt = $"Creating log file @ {System.DateTime.Now}";
        m_writer.WriteLine(introTxt);
    }


    static public void Log(string _log)
    {
        if (m_writer == null)
        {
            JowLogger.CreateFile();
        }

        string txt = _log;

        // log to unity log
        if (m_logToLog == true)
        {
            Debug.Log($"{txt}");
        }

        // Concat time if needed
        if (m_logTime)
        {
            txt = System.DateTime.Now.ToString("HH:mm:ss") + " " + _log;
        }

        // log to file
        if (m_logToFile == true)
        {
            m_writer.WriteLine(txt);
        }
    }


    public void Close()
    {
        string outroTxt = $"Closing log file @ {System.DateTime.Now}";
        m_writer.WriteLine(outroTxt);
        m_writer.Close();
    }
}
