using System;
using UnityEngine;

public static class DataManager
{
    /// <summary>
    /// Call this from anywhere to assert unit-tests.
    /// </summary>
    public static void TestClass()
    {
        // Test strings
        SetValue("_test_string_", "coucou");
        if (GetValue<string>("_test_string_") != "coucou") throw new InvalidOperationException();
        // Test ints
        SetValue("_test_int_", 4);
        if (GetValue<int>("_test_int_") != 4) throw new InvalidOperationException();
        // Test floats
        SetValue("_test_float_", 4.5f);
        if (GetValue<float>("_test_float_") != 4.5f) throw new InvalidOperationException();

        Debug.Log("<color=green>All tests passed.</color>");
    }

    public static void SetValue<T>(string key, T value)
    {
        if (typeof(T) == typeof(string))
            PlayerPrefs.SetString(key, (string)Convert.ChangeType(value, typeof(string)));
        else if (typeof(T) == typeof(int))
            PlayerPrefs.SetInt(key, (int)Convert.ChangeType(value, typeof(int)));
        else if (typeof(T) == typeof(float))
            PlayerPrefs.SetFloat(key, (float)Convert.ChangeType(value,typeof(float)));
        else
            throw new ArgumentException($"Unknown type.\nExpected string,int,float,\nbut got {typeof(T)}");
    }

    public static T GetValue<T>(string key)
    {
        if (typeof(T) == typeof(string))
            return (T)Convert.ChangeType(PlayerPrefs.GetString(key), typeof(T));
        if (typeof(T) == typeof(int))
            return (T)Convert.ChangeType(PlayerPrefs.GetInt(key), typeof(T));
        if (typeof(T) == typeof(float))
            return (T)Convert.ChangeType(PlayerPrefs.GetFloat(key), typeof(T));

        throw new ArgumentException($"Unknown type.\nExpected string,int,float,\nbut got {typeof(T)}");
    }
}
