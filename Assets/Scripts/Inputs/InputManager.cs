using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Array AllKeys;
    [SerializeField] private InputSet[] inputSets;

    void Start()
    {
        AllKeys = Enum.GetValues(typeof(KeyCode));
    }
    
    void Update()
    {
        SortMonsters();
    }

    private void SortMonsters()
    {
        foreach (KeyCode keyCode in AllKeys)
        {
            if (!Input.GetKeyDown(keyCode))
                continue;
            
            foreach (var inputSet in inputSets)
            {
                if (!inputSet.Contains(keyCode))
                    continue;
                
                this.Publish(new PlayerChoiceEvent(inputSet.IsAccepted));
                return;
            }
        }
    }
}
