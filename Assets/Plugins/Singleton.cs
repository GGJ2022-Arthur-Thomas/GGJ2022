using UnityEngine;
using System.Collections.Generic;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

    [SerializeField]
    private bool dontDestroyOnLoad = false;

    [SerializeField]
    private bool destroyIfOtherInstancesDetected = true;

    [SerializeField]
    private bool logErrorIfMultipleInstancesDetected = true;


    private static T _Instance = null;

    private Dictionary<string, int> instancesDic = new Dictionary<string, int>();

    private static bool applicationIsQuitting;


    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                Debug.LogError("Tried to get instance of " + typeof(T) + ", but it is not initialized yet.");
                return null;
            }
            else if (applicationIsQuitting)
            {
                Debug.LogWarning("Instance of " + typeof(T) + " was already destroyed on application quit.");
                return null;
            }

            return _Instance;
        }
    }


	protected virtual void Awake()
	{
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(this);
        }

        applicationIsQuitting = false;

        T[] instances = FindObjectsOfType<T>();

        if (instances.Length == 1) // We only have 1 instance and it's a singleton, it's all right
        {
            _Instance = FindObjectOfType<T>();
        }
        else if (instances.Length > 1) // Not good !
        {
            for (int i = 0; i < instances.Length; i++)
            {
                string gameObjectName = instances[i].gameObject.name;

                if (!instancesDic.ContainsKey(gameObjectName))
                {
                    instancesDic[gameObjectName] = 0;
                }

                instancesDic[gameObjectName]++;
            }

            // At this point, we have a dictionary of each GameObject containing our instance,
            // And how many instances there are on this GameObject

            string s = string.Empty;

            foreach(KeyValuePair<string, int> kvp in instancesDic)
            {
                s += $" {kvp.Key} ({kvp.Value})";
            }

            if (logErrorIfMultipleInstancesDetected)
            {
                Debug.LogError($"Multiple instances of {typeof(T)} detected. Keep only one among the following GameObjects : {s}");

                if (destroyIfOtherInstancesDetected)
                {
                    Destroy(gameObject);
                }
            }
            
            return;
        }
	}

    protected virtual void OnDestroy()
    {
        applicationIsQuitting = true;
    }
}