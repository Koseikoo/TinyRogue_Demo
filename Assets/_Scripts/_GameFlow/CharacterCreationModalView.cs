using System;
using UniRx;
using UnityEngine;

public class CharacterCreationModalView : MonoBehaviour
{
    public void Initialize()
    {
        GameStateContainer.GameState
            .Where(state => state != GameState.CharacterCreation)
            .Subscribe(_ => Destroy(gameObject))
            .AddTo(this);
    }

    public void CreateDefaultCharacter()
    {
        GameStateContainer.GameState.Value = GameState.Island;
    }
}