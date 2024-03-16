using System;
using System.Collections.Generic;
using Container;
using Factories;
using UniRx;
using UnityEngine;
using Zenject;

namespace Models
{
    public class Unit
    {
        public int Level;
        public int MaxHealth;
        public Loot Loot;
        public int DropXp;
        public UnitType Type;
        public ReactiveProperty<int> Health = new();
        public ReactiveProperty<Tile> Tile = new();
        public List<Tile> AdditionalTiles = new();
        public IntReactiveProperty AttackCounter = new();
        public BoolReactiveProperty IsDefeated = new();
        public BoolReactiveProperty IsDead = new();
        public BoolReactiveProperty IsDestroyed = new();
        public BoolReactiveProperty IsDamaged = new();
        public ReactiveCollection<StatusEffect> ActiveStatusEffects = new();
        public BoolReactiveProperty IsInvincible = new();
        
        public ReactiveProperty<Vector3> AttackDirection = new();
        public List<Action<Tile>> DeathActions = new();
        private Unit _lastAttacker;

        [Inject] private UnitDeathActionContainer _unitDeathActionContainer;

        public virtual void Attack(IEnumerable<Mod> mods, Vector3 attackVector, Unit attacker = null)
        {
            if(IsInvincible.Value)
                return;

            _lastAttacker = attacker;
            if(_lastAttacker != null)
                _lastAttacker.AttackCounter.Value++;
            
            foreach (Mod mod in mods)
            {
                mod.ApplyToUnit(this, attacker);
            }

            IsDead.Value = Health.Value <= 0;
            IsDamaged.Value = Health.Value < MaxHealth || ActiveStatusEffects.Count > 0;
            attackVector.Normalize();
            AttackDirection.Value = attackVector;
            
            if(IsDead.Value && this is Enemy)
                PersistentPlayerState.IncreaseHeritage(Type, Level);
        }

        public virtual void Damage(int damage, Unit attacker = null)
        {
            _lastAttacker = attacker;
            Health.Value -= damage;
            IsDead.Value = Health.Value <= 0;
            IsDamaged.Value = Health.Value < MaxHealth;
        }

        public virtual void Death()
        {
            if (_lastAttacker != null)
            {
                Loot?.RewardTo(_lastAttacker, Tile.Value.WorldPosition, false);
            }
            IslandLootContainer.DropLoot.Execute(false);

            if (_lastAttacker == GameStateContainer.Player)
            {
                if(this is not Interactable)
                    GameStateContainer.Player.Weapon.ActiveCombo.Add(this);
                GameStateContainer.Player.Weapon.AddXp(DropXp);
            }
            
            Tile.Value.RemoveUnit();
            for (int i = 0; i < AdditionalTiles.Count; i++)
                AdditionalTiles[i].RemoveUnit();
            
            if(Tile.Value.Island != null)
                Tile.Value.Island.RemoveUnit(this);

            for (int i = 0; i < DeathActions.Count; i++)
                DeathActions[i]?.Invoke(Tile.Value);
        }
    }
}