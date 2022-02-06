using UnityEngine;
using System.Collections;

public class StaticCoroutine : MonoBehaviour
{
	
	private static StaticCoroutine Instance = null;
    private static int nbCoroutinesRunning = 0;
	
	private static StaticCoroutine instance
    {
		get
        {
			if (Instance == null)
            {
				Instance = FindObjectOfType<StaticCoroutine>();
				if (Instance == null)
				{
					Instance = new GameObject("StaticCoroutine").AddComponent<StaticCoroutine>();
				}
			}
			return Instance;
		}
	}
	
	void Awake()
    {
		if (Instance == null)
        {
			Instance = this;
		}
	}
	
	IEnumerator Perform(IEnumerator coroutine)
    {
		yield return StartCoroutine(coroutine);
        nbCoroutinesRunning--;

        if (nbCoroutinesRunning == 0) // No more coroutines running, we can die
        {
            Die();
        }
    }
	
	/// <summary>
	/// Method to call in order to run a static Coroutine
	/// </summary>
	public static void DoCoroutine(IEnumerator coroutine)
    {
        nbCoroutinesRunning++;
        instance.StartCoroutine(instance.Perform(coroutine));
	}

	public static void StopAll()
	{
		instance.Die();
	}
	
	void Die()
    {
		Instance = null;
		Destroy(gameObject);
	}
	
	void OnApplicationQuit()
    {
		Instance = null;
	}
}