using System;
using System.Collections.Generic;
using UnityEngine;
using Views;

public enum Choices
{
    NextIsland,
    ToShip,
    IncreaseHealth,
    IncreaseDamage,
    IncreaseRange
}

[System.Serializable]
public class ChoiceDefinition
{
    public string CardText;
    public Sprite Icon;
    public Choices Choice;
}

public class ChoiceContainer
{
    private Dictionary<Choices, ChoiceDefinition> _choiceDefinitions = new();
    public ChoiceContainer(
        ChoiceDefinition[] choices
        )
    {
        foreach (ChoiceDefinition c in choices)
        {
            _choiceDefinitions[c.Choice] = c;
        }
    }

    public Choice GetChoice(Choices choices, Action logic)
    {
        ChoiceDefinition definition = _choiceDefinitions[choices];
        return new Choice()
        {
            Icon = definition.Icon,
            Text = definition.CardText,
            Logic = logic
        };
    }
}