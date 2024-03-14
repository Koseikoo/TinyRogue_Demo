using System;
using System.Collections.Generic;
using UnityEngine;
using Views;

public enum Choices
{
    NextIsland,
    ToShip,
    IncreaseHealth,
    IncreaseDamage
}

[System.Serializable]
public class ChoiceDefinition
{
    public Sprite Icon;
    public string CardText;
}

public class ChoiceContainer
{
    private Dictionary<Choices, ChoiceDefinition> _choiceDefinitions = new();
    public ChoiceContainer(
        ChoiceDefinition nextIsland,
        ChoiceDefinition toShip,
        ChoiceDefinition increaseHealth,
        ChoiceDefinition increaseDamage
        )
    {
        _choiceDefinitions[Choices.NextIsland] = nextIsland;
        _choiceDefinitions[Choices.ToShip] = toShip;
        _choiceDefinitions[Choices.IncreaseHealth] = increaseHealth;
        _choiceDefinitions[Choices.IncreaseDamage] = increaseDamage;
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