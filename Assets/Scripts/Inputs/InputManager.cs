using System;
using UnityEngine;

public class InputManager : Singleton<InputManager>
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
            
            //Debug.Log("Player pressed " + keyCode + " key");
            
            foreach (var inputSet in inputSets)
            {
                if (!inputSet.Contains(keyCode))
                    continue;
                
                //Debug.Log("Key " + keyCode + " is in inputSet " + (inputSet.IsAccepted ? "accepted" : "rejected"));
                
                this.Publish(new PlayerChoiceEvent(inputSet.IsAccepted));
                return;
            }
        }
    }
}
