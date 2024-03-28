using System;
using Game;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using Zenject;

namespace Views
{
    public class TapHoldProgressView : MonoBehaviour
    {
        [Inject] private IntuitiveInputManager _inputManager;

        [SerializeField] private ProceduralImage _progressImage;

        private void Update()
        {
            if(GameStateContainer.Player == null)
                return;
            
            transform.position = UIHelper.Camera.WorldToScreenPoint(GameStateContainer.Player.Tile.Value.FlatPosition);
            _progressImage.fillAmount = _inputManager.MoveModeTracker / IntuitiveInputManager.MoveModeTimer;
        }
    }
}