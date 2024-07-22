using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
    public static void DebugLog(string message)
    {
        Debug.Log($"{Time.time} {message}");
    }

    public static void DebugLogWarning(string message)
    {
        Debug.LogWarning($"{Time.time} {message}");
    }
    
    
    public static void DebugLogError(string message)
    {
        Debug.LogError($"{Time.time} {message}");
    }
    
    

}    
