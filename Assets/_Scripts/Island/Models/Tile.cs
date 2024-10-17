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
    public Models.GameUnit GameUnit;
    public TileSelectionType Type;
    
    public TileSelection(Models.GameUnit gameUnit, TileSelectionType type)
    {
        GameUnit = gameUnit;
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
        public ReactiveProperty<GameUnit> Unit = new();

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
        public event Action<GameUnit> OnMoveTo;
        private List<Action<GameUnit>> _singleExecutionFunctions = new();

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

        public void AddMoveToLogic(Action<GameUnit> moveToAction)
        {
            OnMoveTo += moveToAction;
        }

        public void AddSingleExecutionLogic(Action<GameUnit> moveToAction)
        {
            OnMoveTo += moveToAction;
            _singleExecutionFunctions.Add(moveToAction);
        }

        public void MoveUnit(GameUnit gameUnit)
        {
            if (gameUnit.Tile.Value != null)
            {
                gameUnit.Tile.Value.RemoveUnit();
            }

            gameUnit.Tile.Value = this;
            Unit.Value = gameUnit;
        }

        public void MoveUnitWithAction(GameUnit gameUnit)
        {
            MoveUnit(gameUnit);
            OnMoveTo?.Invoke(gameUnit);
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
            TileSelection previousSelection = Selections.FirstOrDefault(s => s.GameUnit == selection.GameUnit);
            if(previousSelection != null && previousSelection.Type == selection.Type)
            {
                return;
            }

            Selections.Add(selection);
        }

        public void RemoveSelector(GameUnit gameUnit, TileSelectionType type = TileSelectionType.None)
        {
            List<TileSelection> selectionsToRemove =
                Selections.Where(s => s.GameUnit == gameUnit && (type == TileSelectionType.None || s.Type == type)).ToList();
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
            foreach (Action<GameUnit> func in _singleExecutionFunctions)
            {
                OnMoveTo -= func;
            }
            _singleExecutionFunctions.Clear();
        }
    }
}