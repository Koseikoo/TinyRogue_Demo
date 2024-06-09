using System;
using System.Collections.Generic;
using Container;
using Factories;
using Factory;
using UniRx;
using UnityEngine;
using Zenject;

namespace Models
{
    public class Unit : IDisposable
    {
        public int Level;
        public int MaxHealth;
        public Loot Loot;
        public int DropXp;
        public UnitType Type;
        public bool IncreaseComboWithDeath;
        public ReactiveProperty<int> Health = new();
        public ReactiveProperty<Tile> Tile = new();
        public List<Tile> AdditionalTiles = new();
        public IntReactiveProperty AttackCounter = new();
        public BoolReactiveProperty IsDead = new();
        public BoolReactiveProperty IsDestroyed = new();
        public BoolReactiveProperty IsDamaged = new();
        public ReactiveCollection<StatusEffect> ActiveStatusEffects = new();
        public BoolReactiveProperty IsInvincible = new();

        public List<IDisposable> UnitSubscriptions = new();

        
        public ReactiveProperty<Vector3> AttackDirection = new();
        public List<Action<Tile>> DeathActions = new();
        private Unit _lastAttacker;

        protected Unit()
        {
            IDisposable destroySubscription = IsDestroyed.Where(b => b).Subscribe(_ => Dispose());
            UnitSubscriptions.Add(destroySubscription);
        }

        public virtual void Attack(IEnumerable<Mod> mods, Vector3 attackVector, Unit attacker = null)
        {
            if(IsInvincible.Value)
            {
                return;
            }

            _lastAttacker = attacker;
            if(_lastAttacker != null)
            {
                _lastAttacker.AttackCounter.Value++;
            }

            foreach (Mod mod in mods)
            {
                mod.ApplyToUnit(this, attacker);
            }

            IsDead.Value = Health.Value <= 0;
            IsDamaged.Value = Health.Value < MaxHealth || ActiveStatusEffects.Count > 0;
            attackVector.Normalize();
            AttackDirection.Value = attackVector;

            if (IsDead.Value)
            {
                Dispose();
                if(this is Enemy)
                {
                    PersistentPlayerState.IncreaseHeritage(DropXp);
                }
            }
        }

        public virtual void Damage(int damage, Unit attacker = null, bool pierceInvincibility = false)
        {
            if(IsInvincible.Value && !pierceInvincibility)
            {
                return;
            }

            _lastAttacker = attacker;
            Health.Value -= damage;
            IsDead.Value = Health.Value <= 0;
            IsDamaged.Value = Health.Value < MaxHealth;
            
            if (IsDead.Value)
            {
                Dispose();
                if(this is Enemy)
                {
                    PersistentPlayerState.IncreaseHeritage(DropXp);
                }
            }
        }

        public virtual void Death()
        {
            Unit lootReceiver = _lastAttacker ?? GameStateContainer.Player;
            Loot?.RewardTo(lootReceiver, Tile.Value.FlatPosition);
            WorldLootContainer.DropLoot.Execute();

            if (lootReceiver == GameStateContainer.Player)
            {
                GameStateContainer.Player.DropXP(Tile.Value, DropXp);
            }
            
            Tile.Value.RemoveUnit();
            for (int i = 0; i < AdditionalTiles.Count; i++)
                AdditionalTiles[i].RemoveUnit();
            
            if(Tile.Value.Island != null)
            {
                Tile.Value.Island.RemoveUnit(this);
            }

            for (int i = 0; i < DeathActions.Count; i++)
                DeathActions[i]?.Invoke(Tile.Value);
        }

        public void Dispose()
        {
            foreach (IDisposable subscription in UnitSubscriptions)
                subscription?.Dispose();
            
            UnitSubscriptions.Clear();
        }
    }
}