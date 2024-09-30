using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public enum EnemyState
{
    Idle,
    TargetFound,
    AimAtTarget,
}

namespace Models
{
    public class Enemy : Unit, IAttacker
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
            CurrentTurnDelay.Value = TurnDelay;
            
            AttackTarget = GameStateContainer.Player;
            IDisposable turnSubscription = GameStateContainer.TurnState
                .Where(state => state == TurnState.EnemyTurn)
                .Subscribe(_ => OnEnemyTurn());

            IDisposable stateSubscription = State
                .Pairwise()
                .Where(pair => pair.Previous == EnemyState.Idle)
                .Subscribe(_ => CurrentTurnDelay.Value = TurnDelay);

            IDisposable destroySubscription = IsDestroyed.Where(b => b).Subscribe(_ =>
            {
                RemoveLastSelection();
            });
            
            UnitSubscriptions.Add(turnSubscription);
            UnitSubscriptions.Add(stateSubscription);
            UnitSubscriptions.Add(destroySubscription);
        }

        public override void Death()
        {
            base.Death();
            RemoveLastSelection();
            _cameraModel.RotationShakeCommand.Execute();
        }

        private void OnEnemyTurn()
        {
            if (Tile.Value != null)
            {
                if (State.Value == EnemyState.Idle)
                {
                    State.Value = ScanSurroundingTiles();
                    RemoveLastSelection();
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
        }
        
        protected virtual void EnemyAction()
        {
            Debug.Log("Enemy Action");
        }

        protected virtual bool IsAttackPathTile(Tile tile)
        {
            bool isPlayerOnTile = tile.Unit.Value is Player;
            return !tile.HasUnit || isPlayerOnTile;
        }
        
        protected EnemyState ScanSurroundingTiles()
        {
            if (Island == null)
            {
                return EnemyState.Idle;
            }

            List<Tile> tiles = Island.Tiles.GetTilesWithinDistance(Tile.Value, ScanRange);

            Tile tileWithAttackTarget = tiles.FirstOrDefault(tile => tile.Unit.Value == AttackTarget);
            return tileWithAttackTarget == null ? EnemyState.Idle : EnemyState.TargetFound;
        }

        protected virtual void RenderAttackPath()
        {
            if(!AimAtTarget.Value)
            {
                return;
            }

            List<Tile> path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value);
            
            if(path == null)
            {
                return;
            }

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
            {
                CurrentTurnDelay.Value--;
            }
            else
            {
                CurrentTurnDelay.Value = TurnDelay;
            }

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
    }
}