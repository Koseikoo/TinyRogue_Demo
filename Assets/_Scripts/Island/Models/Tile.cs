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
        
        public Vector3 WorldPosition;
        public List<Tile> Neighbours = new();
        public ReactiveProperty<Unit> Unit = new();

        public bool IsSegmentTile;
        public BoolReactiveProperty Destroyed = new();
        public BoolReactiveProperty IsPathTile = new();
        public BoolReactiveProperty WeaponOnTile = new();
        public ReactiveCollection<TileSelection> Selections = new();
        public Node Node { get; private set; }

        private Island _island;
        private Ship _ship;
        
        private IDisposable _turnSubscription;
        private event Action<Unit> _onMoveTo;

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
        public bool IsEndTile => _island.EndTile == this;

        public Island Island => _island;
        public Ship Ship => _ship;
        public List<Tile> TileCollection => _island == null ? _ship.Tiles : _island.Tiles;
        
        public bool AttackBouncesFromTile => HasUnit && (Unit.Value.Health.Value > GameStateContainer.Player.Weapon.GetAttackDamage() || Unit.Value.IsInvincible.Value);


        public Tile(Vector3 worldPosition)
        {
            WorldPosition = worldPosition;
            Neighbours = new();
            Node = new Node(this);

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

        public void AddNeighbour(Tile tile)
        {
            if(tile == this)
                return;
            
            Neighbours.Add(tile);
        }

        public void AddMoveToLogic(Action<Unit> moveToAction)
        {
            _onMoveTo += moveToAction;
        }

        public void MoveUnit(Unit unit)
        {
            if (unit.Tile.Value != null)
                unit.Tile.Value.RemoveUnit();
            
            unit.Tile.Value = this;
            Unit.Value = unit;
            _onMoveTo?.Invoke(unit);
            
            if (unit is Player player)
                player.Weapon.Tile.Value = this;
        }

        public void RemoveUnit()
        {
            Unit.Value = null;
        }

        public void AddSelector(TileSelection selection)
        {
            var previousSelection = Selections.FirstOrDefault(s => s.Unit == selection.Unit);
            if(previousSelection != null && previousSelection.Type == selection.Type)
                return;
            
            Selections.Add(selection);
        }

        public void RemoveSelector(Unit unit, TileSelectionType type = TileSelectionType.None)
        {
            var selectionsToRemove =
                Selections.Where(s => s.Unit == unit && (type == TileSelectionType.None || s.Type == type)).ToList();
            for (int i = 0; i < selectionsToRemove.Count; i++)
            {
                Selections.Remove(selectionsToRemove[i]);
            }
        }

        private void TurnAction()
        {
            // TODO Implement Tile Logic per Turn
        }
    }
}