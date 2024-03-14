using System;
using UnityEngine;

namespace Container
{
    public class EnemyStateIconContainer
    {
        private Sprite _idle;
        private Sprite _foundTarget;
        private Sprite _aimAtTarget;

        public EnemyStateIconContainer(
            Sprite idle,
            Sprite foundTarget,
            Sprite aimAtTarget)
        {
            _idle = idle;
            _foundTarget = foundTarget;
            _aimAtTarget = aimAtTarget;
        }

        public Sprite GetStateSprite(EnemyState state)
        {
            return state switch
            {
                EnemyState.Idle => _idle,
                EnemyState.TargetFound => _foundTarget,
                EnemyState.AimAtTarget => _aimAtTarget,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, $"No icon for State {state}")
            };
        }
    }
}