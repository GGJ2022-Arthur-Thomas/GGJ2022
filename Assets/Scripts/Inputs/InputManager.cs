using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputSet[] inputSets;
    
    void Update()
    {
        SortMonsters();
    }

    private void SortMonsters()
    {
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(keyCode))
                return;
            
            foreach (var inputSet in inputSets)
            {
                if (inputSet.Contains(keyCode))
                {
                    this.Publish(new PlayerChoiceEvent(inputSet.IsAccepted));
                    return;
                }
            }
        }
    }
}
