using System;
using UnityEngine;

public class InitializationManager : MonoBehaviour
{
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject createNewButton;
    private void Awake()
    {
        SetContinueButton();
        // Load Game
    }

    public void Continue()
    {
        SceneHelper.LoadScene(SceneType.Game);
    }

    public void CreateNewCharacter()
    {
        SceneHelper.LoadScene(SceneType.Game);
    }

    private void SetContinueButton()
    {
        
    }
}