using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public enum TileSelectionType
{
    None,
    Attack,
    Aim,
    Move,
    Blocked,
}

public class TileSelection
{
    public Models.Unit Unit;
    public TileSelectionType Type;
    
    public TileSelection(Models.Unit unit, TileSelectionType type)
    {
        Unit = unit;
        Type = type;
    }
}

namespace Models
{
    public class Tile
    {
        public const float BoardOffset = .15f;
        public const float GrassOffset = .1f;
        
        private const float HeightMult = .5f;

        public Vector3 FlatPosition;
        public int HeightLevel;
        public List<Tile> Neighbours = new();
        public ReactiveProperty<Unit> Unit = new();

        public bool IsSegmentTile;
        public bool IsWeak;
        public BoolReactiveProperty Destroyed = new();
        public BoolReactiveProperty IsPathTile = new();
        public BoolReactiveProperty WeaponOnTile = new();
        public ReactiveCollection<TileSelection> Selections = new();
        public Node Node { get; private set; }

        private Island _island;
        private Ship _ship;
        
        private IDisposable _turnSubscription;
        public event Action<Unit> OnMoveTo;
        private List<Action<Unit>> _singleExecutionFunctions = new();

        // Visual Properties

        public TerrainType TerrainType;
        public GrassType GrassType;
        public float GrassRotation;
        public BoardType BoardType;
        public BridgeType BridgeType;
        
        //Debug
        public ReactiveProperty<bool> DebugElevate = new();

        public bool IsEdgeTile => Neighbours.Count < 6;
        public bool HasUnit => Unit.Value != null;
        public bool HasAliveUnit => Unit.Value != null && Unit.Value.Health.Value > 0;
        public bool IsStartTile => _island.StartTile == this;
        public bool IsHeartTile => _island.HeartTile == this;

        public Vector3 WorldPosition => FlatPosition + (HeightLevel * HeightMult * Vector3.up);

        public Island Island => _island;
        public Ship Ship => _ship;
        public List<Tile> TileCollection => _island.Tiles;
        
        public bool AttackBouncesFromTile => HasUnit && (Unit.Value.Health.Value > GameStateContainer.Player.Weapon.Value.Damage || Unit.Value.IsInvincible.Value);
        public int MoveToActions => OnMoveTo == null ? 0 : OnMoveTo.GetInvocationList().Length;

        public Tile(Vector3 flatPosition, Island island, int heightLevel = 0)
        {
            FlatPosition = flatPosition;
            Node = new Node(this);
            _island = island;

            _turnSubscription = GameStateContainer.TurnState
                .Where(state => state == TurnState.IslandTurn)
                .Subscribe(_ => TurnAction());
        }

        public void SetIsland(Island island)
        {
            _island = island;
        }

        public void SetShip(Ship ship)
        {
            _ship = ship;
        }

        public void AddMoveToLogic(Action<Unit> moveToAction)
        {
            OnMoveTo += moveToAction;
        }

        public void AddSingleExecutionLogic(Action<Unit> moveToAction)
        {
            OnMoveTo += moveToAction;
            _singleExecutionFunctions.Add(moveToAction);
        }

        public void MoveUnit(Unit unit)
        {
            if (unit.Tile.Value != null)
            {
                unit.Tile.Value.RemoveUnit();
            }

            unit.Tile.Value = this;
            Unit.Value = unit;
        }

        public void MoveUnitWithAction(Unit unit)
        {
            MoveUnit(unit);
            OnMoveTo?.Invoke(unit);
            RemoveSingleExecutionActions();
        }

        public void RemoveUnit()
        {
            Unit.Value = null;
            if (IsWeak && Island != null)
            {
                Island.DestroyTile(this);
            }
        }

        public void AddSelector(TileSelection selection)
        {
            TileSelection previousSelection = Selections.FirstOrDefault(s => s.Unit == selection.Unit);
            if(previousSelection != null && previousSelection.Type == selection.Type)
            {
                return;
            }

            Selections.Add(selection);
        }

        public void RemoveSelector(Unit unit, TileSelectionType type = TileSelectionType.None)
        {
            List<TileSelection> selectionsToRemove =
                Selections.Where(s => s.Unit == unit && (type == TileSelectionType.None || s.Type == type)).ToList();
            foreach (TileSelection selection in selectionsToRemove)
            {
                Selections.Remove(selection);
            }
        }

        private void TurnAction()
        {
            // TODO Implement Tile Logic per Turn
        }

        private void RemoveSingleExecutionActions()
        {
            foreach (Action<Unit> func in _singleExecutionFunctions)
            {
                OnMoveTo -= func;
            }
            _singleExecutionFunctions.Clear();
        }
    }
}