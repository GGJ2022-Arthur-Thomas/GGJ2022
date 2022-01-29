using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuleValidator : MonoBehaviour
{
    public bool IsChoiceRight(Rule rule, Monster monster, bool isAccepted)
    {
        return rule.Monsters.Contains(monster) == isAccepted;
    }
}
