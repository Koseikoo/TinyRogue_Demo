using System;
using System.Collections.Generic;
using Models;
using UnityEngine;
using Zenject;

namespace Game
{
    public class MoveInputTracker
    {
        const float WaitTimeInMilliseconds = 15000f;
        private const int ContinuousSwipesNeeded = 3;
        
        private List<Vector3> _moveInputHistory = new();

        [Inject] private CameraModel _cameraModel;
        [Inject] private PlayerManager _playerManager;

        private DateTime _lastMoved;

        private MoveInputTracker()
        {
            _lastMoved = DateTime.Now;
        }
        
        public void AddMovement(Vector3 newDirection)
        {
            var passedMilliseconds = (DateTime.Now - _lastMoved).TotalMilliseconds;
            if(passedMilliseconds > WaitTimeInMilliseconds)
                Reset();
            _lastMoved = DateTime.Now;
            
            if(_moveInputHistory.Count == 0)
                _moveInputHistory.Add(newDirection);
            else
            {
                float dot = Vector3.Dot(_moveInputHistory[^1], newDirection);
                bool sameDirectionAsPrevious = dot > .5f;
                if(!sameDirectionAsPrevious)
                    _moveInputHistory.Clear();
                
                _moveInputHistory.Add(newDirection);
            }

            if (_moveInputHistory.Count >= ContinuousSwipesNeeded)
            {
                _cameraModel.LookCommand.Execute(_playerManager.Player.FacingDirection);
                _moveInputHistory.Clear();
                
            }
        }

        public void Reset()
        {
            _moveInputHistory.Clear();
        }
    }
}