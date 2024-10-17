using System;
using UniRx;
using UnityEngine;

namespace Models
{
    public class PoisonEffect : StatusEffect
    {
        private const int Damage = 1;
        
        public IntReactiveProperty Duration = new();
        private int _maxDuration;

        private IDisposable _effectSubscription;
        
        public PoisonEffect(GameUnit target, GameUnit caster, int power) : base(target, caster)
        {
            _maxDuration = power;
            Duration.Value = _maxDuration;
        }

        public override void ResetEffect()
        {
            Duration.Value = _maxDuration;
        }

        protected override void SubscribeToUpdateCondition()
        {
            _effectSubscription = GameStateContainer.TurnState
                .Where(state => state == TurnState.PlayerTurnEnd && _target != null && !_target.IsDead.Value)
                .Skip(1)
                .Subscribe(_ => ProgressLogic());
        }

        public override void ProgressLogic()
        {
            if(_target == null)
            {
                return;
            }

            if (Duration.Value <= 1 || _target.Health.Value - Damage <= 0)
            {
                EndLogic();
            }

            _target.Damage(Damage, _caster);
            Duration.Value -= 1;
        }

        public override void EndLogic()
        {
            _effectSubscription.Dispose();
            _target.ActiveStatusEffects.Remove(this);
        }
    }
}