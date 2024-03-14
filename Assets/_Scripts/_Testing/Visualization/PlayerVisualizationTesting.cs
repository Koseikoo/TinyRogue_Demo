using System;
using Factories;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Views;
using Zenject;
using AnimationState = Models.AnimationState;

public class PlayerVisualizationTesting : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown characterActionDropdown;
    [SerializeField] private Button characterActionButton;
    [SerializeField] private Animator playerAnimator;

    [SerializeField] private Transform[] TestTilePoints;
    [SerializeField] private PlayerView playerView;

    private void Awake()
    {
        string[] enumNames = Enum.GetNames(typeof(AnimationState));
        Dropdown.OptionData[] options = new Dropdown.OptionData[enumNames.Length];
        
        characterActionButton.onClick.AddListener(() =>
        {
            
            TriggerCharacterAction((AnimationState)characterActionDropdown.value);
        });
    }

    private void TriggerCharacterAction(AnimationState action)
    {
        playerAnimator.SetTrigger(action.ToString());
    }

    private void MoveToTile()
    {
        
    }

    private void AttackCommand()
    {
        
    }

    private void GetDamaged()
    {
        
    }

    private void Death()
    {
        
    }
    
}