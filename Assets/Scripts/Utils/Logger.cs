using UnityEngine;

public static class Logger
{
    /// <summary>
    /// Change this to log or not.
    /// </summary>
    public static bool CanLog = false;


    public static void Log(string message)
    {
        if (CanLog)
        {
            Debug.Log(message);
        }
    }
}
