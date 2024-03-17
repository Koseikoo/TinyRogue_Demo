using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Models
{
    public enum AnimationState
    {
        MoveToTile,
        Attack1,
        GetDamaged,
        Death,
        Attack2,
        Attack3,
        Aim,
        Idle
    }
    
    public class Player : Unit
    {
        public Weapon Weapon { get; private set; }
        public Bag Bag { get; private set; }
        
        public ReactiveCollection<Tile> SelectedTiles = new();
        public ReactiveCollection<Tile> SwipedTiles = new();
        
        public IntReactiveProperty SkippedTurns = new();
        public FloatReactiveProperty AnchorYRotation = new();
        public BoolReactiveProperty InAttackMode = new();
        public List<Slot> EquipmentSlots = new();

        public bool InMovementFlow;

        public Transform HolsterTransform;

        public Vector3 FacingDirection { get; private set; }

        public Player(Weapon weapon)
        {
            Weapon = weapon;
            Bag = new Bag(this);
            GameStateContainer.Player = this;
        }
        
        public bool TryMoveInDirection(Vector2 swipeDirection)
        {
            Tile swipedTile = CalculateDirectionNeighbour(swipeDirection);
            if(swipedTile == null || swipedTile.HasUnit)
                return false;

            FacingDirection = (swipedTile.WorldPosition - Tile.Value.WorldPosition).normalized;
            swipedTile.MoveUnit(this);
            Weapon.RecoverAttackCharge();
            return true;
        }

        public override void Death()
        {
            GameStateContainer.GameState.Value = GameState.Dead;
        }

        private Tile CalculateDirectionNeighbour(Vector2 swipe)
        {
            Vector3 worldSwipeDirection = (Vector3.forward * swipe.y) + (Vector3.right * swipe.x);
            worldSwipeDirection.Normalize();
            worldSwipeDirection = MathHelper.RotateVector(worldSwipeDirection, Vector3.up, AnchorYRotation.Value);
            float length = (Tile.Value.WorldPosition - Tile.Value.Neighbours[0].WorldPosition).magnitude;
            worldSwipeDirection *= length;

            Tile closestNeighbour = IslandHelper.GetNeighbourFromDirection(Tile.Value, worldSwipeDirection);
            return closestNeighbour;
        }
    }
}