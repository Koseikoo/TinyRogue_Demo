using System;
using System.Collections.Generic;
using System.Linq;
using Factories;
using UniRx;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

public enum EnemyState
{
    Idle,
    TargetFound,
    AimAtTarget,
}

namespace Models
{
    public class Enemy : Unit, IAttacker, IDisposable
    {
        private const int EnemyMods = 3;

        [Inject] private CameraModel _cameraModel;

        public int ScanRange;
        public int AttackRange;
        public int TurnDelay;
        public List<Slot> ModSlots { get; set; } = new();

        public ReactiveProperty<EnemyState> State = new();
        public IntReactiveProperty CurrentTurnDelay = new();
        public BoolReactiveProperty AimAtTarget = new();
        public ReactiveCommand<AnimationState> AnimationCommand = new();
        public BoolReactiveProperty IsEnraged = new();
        public Tile NextMoveTile;
        public Unit AttackTarget;

        private List<Tile> _lastSelectedTiles = new();
        private IDisposable _turnSubscription;
        private IDisposable _stateSubscription;
        private IDisposable _destroySubscription;

        public bool SkipTurn => CurrentTurnDelay.Value > 0;
        protected Island Island => Tile.Value?.Island;

        public Enemy()
        {
            for (int i = 0; i < EnemyMods; i++)
            {
                ModSlots.Add(new());
            }
            
            AttackTarget = GameStateContainer.Player;
            _turnSubscription = GameStateContainer.TurnState
                .Where(state => state == TurnState.EnemyTurn)
                .Subscribe(_ => OnEnemyTurn());

            _stateSubscription = State
                .Pairwise()
                .Where(pair => pair.Previous == EnemyState.Idle)
                .Subscribe(_ => CurrentTurnDelay.Value = TurnDelay);

            _destroySubscription = IsDestroyed.Where(b => b).Subscribe(_ =>
            {
                Dispose();
                RemoveLastSelection();
            });
        }

        public override void Death()
        {
            base.Death();
            RemoveLastSelection();
            Dispose();
            _cameraModel.UnitDeathShakeCommand.Execute();
        }

        private void OnEnemyTurn()
        {
            if (State.Value == EnemyState.Idle)
            {
                State.Value = ScanSurroundingTiles();
                RemoveLastSelection();
                CurrentTurnDelay.Value = 0;
                return;
            }

            if (!SkipTurn)
            {
                EnemyAction();
            }
            
            ProgressTurnDelay();
            
            RemoveLastSelection();
            RenderAttackPath();
        }
        
        protected virtual void EnemyAction()
        {
            Debug.Log("Enemy Action");
        }

        protected virtual bool IsAttackPathTile(Tile tile)
        {
            var tileUnit = tile.Unit.Value;
            bool ignoreTile = tile.HasUnit && (tileUnit.IsInvincible.Value || tileUnit is Enemy);
            return !ignoreTile && !tile.WeaponOnTile.Value;
        }
        
        protected EnemyState ScanSurroundingTiles()
        {
            if (Island == null)
                return EnemyState.Idle;
            var tiles = Island.Tiles.GetTilesWithinDistance(Tile.Value, ScanRange);

            Tile tileWithAttackTarget = tiles.FirstOrDefault(tile => tile.Unit.Value == AttackTarget);
            return tileWithAttackTarget == null ? EnemyState.Idle : EnemyState.TargetFound;
        }

        protected virtual void RenderAttackPath()
        {
            if(!AimAtTarget.Value)
                return;
            
            var path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value);
            
            if(path == null)
                return;
            
            if (path.Count <= AttackRange)
            {
                UpdateSelectedTiles(path, TileSelectionType.Attack);
            }
            else if(!path[0].HasUnit)
            {
                UpdateSelectedTiles(new(){path[0]}, TileSelectionType.Move);
                NextMoveTile = path[0];
            }
        }

        private void ProgressTurnDelay()
        {
            if (CurrentTurnDelay.Value > 0)
                CurrentTurnDelay.Value--;
            else
                CurrentTurnDelay.Value = TurnDelay;

            AimAtTarget.Value = CurrentTurnDelay.Value == 0;
        }

        protected void UpdateSelectedTiles(List<Tile> newSelection, TileSelectionType type)
        {
            foreach (Tile tile in newSelection)
            {
               tile.AddSelector(new(this, type));
               _lastSelectedTiles.Add(tile);
            }
        }

        private void RemoveLastSelection()
        {
            foreach (Tile tile in _lastSelectedTiles)
            {
                tile.RemoveSelector(this);
            }
            _lastSelectedTiles.Clear();
        }

        public void Dispose()
        {
            _turnSubscription?.Dispose();
            _stateSubscription?.Dispose();
            _destroySubscription?.Dispose();
        }

    }
}