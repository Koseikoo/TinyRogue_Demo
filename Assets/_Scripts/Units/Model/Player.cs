using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

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
    
    public class Player : GameUnit
    {
        public ReactiveProperty<WeaponData> Weapon = new();
        public ReactiveCollection<PlayerSkill> UnlockedSkills = new();
        public Bag Bag { get; private set; }

        public List<Tile> AimedTiles = new();
        public ReactiveProperty<Vector3> LookDirection = new();
        public ReactiveCollection<Tile> SelectedTiles = new();
        
        public IntReactiveProperty SkippedTurns = new();
        public FloatReactiveProperty AnchorYRotation = new();

        public ReactiveCommand<(Tile dropTile, int xp)> XpAnimationCommand = new();
        public ReactiveCommand StartAttackCommand = new();
        public ReactiveCommand EnterIsland = new();
        public ReactiveCommand<Action> ExitIsland = new();
        public ReactiveCommand<Action> EnterShip = new();
        public ReactiveProperty<Tile[]> LastAimedTiles = new();
        
        public IntReactiveProperty Xp = new();
        public IntReactiveProperty Level = new();
        
        public bool CanDashHit => UnlockedSkills.Any(skill => skill.Name == SkillName.DashHit);
        public bool CanKnockBack => UnlockedSkills.Any(skill => skill.Name == SkillName.KnockBack);
        public bool CanPierce => UnlockedSkills.Any(skill => skill.Name == SkillName.PiercingDamage);
        
        [Inject] private CameraModel _cameraModel;

        public bool InMovementFlow;
        public bool InAction;

        public Transform HolsterTransform;
        public Vector3 FacingDirection { get; private set; }

        public Player()
        {
            Bag = new Bag(this);
            AssignWeapon(WeaponName.None, WeaponHelper.BasePattern, 1);
            GameStateContainer.Player = this;
        }

        public void AddSkill(PlayerSkill skill)
        {
            if (UnlockedSkills.All(s => s.Name != skill.Name))
            {
                UnlockedSkills.Add(skill);
            }
        }


        public void AttackTiles(Tile[] aimedTiles)
        {
            Tile tileToAttack = aimedTiles.FirstOrDefault(tile => tile.HasUnit);
            if (tileToAttack != null)
            {
                LastAimedTiles.Value = aimedTiles;
                tileToAttack.Unit.Value.Damage(1);
            }
        }

        public void AssignWeapon(WeaponName name, Func<Vector3, List<Tile>> pattern, int damage)
        {
            Weapon.Value = new(name, pattern, damage);
        }

        public void StartAttack(Tile[] aimedTiles)
        {
            InAction = true;
            LastAimedTiles.Value = aimedTiles;
            StartAttackCommand.Execute();
        }

        public void Attack()
        {
            List<Tile> tilesToAttack = LastAimedTiles.Value
                .Where(tile => tile.HasUnit)
                .OrderBy(tile => Vector3.Distance(tile.FlatPosition, Tile.Value.FlatPosition))
                .ToList();

            _cameraModel.ForwardShakeCommand.Execute();

            List<Tile> attackedTiles = new();
            foreach (Tile tile in tilesToAttack)
            {
                bool isBehindAttackedTile = attackedTiles.Count(t => tile.IsBehind(t)) > 0;
                if (isBehindAttackedTile && CanPierce)
                {
                    tile.Unit.Value.Damage(Weapon.Value.Damage);
                    attackedTiles.Add(tile);
                }
                else if(!isBehindAttackedTile)
                {
                    
                    tile.Unit.Value.Damage(Weapon.Value.Damage);
                    attackedTiles.Add(tile);
                }
            }

            if (CanKnockBack)
            {
                KnockBackEnemies(tilesToAttack, LookDirection.Value);
            }
            InAction = false;
        }

        private void KnockBackEnemies(List<Tile> tilesToAttack, Vector3 knockBackDirection)
        {
            for (int tileIndex = tilesToAttack.Count - 1; tileIndex >= 0; tileIndex--)
            {
                Tile tile = tilesToAttack[tileIndex];
                Tile nextTile = knockBackDirection.GetTileInDirection(tile);

                if (tile.HasUnit && tile.Unit.Value is Enemy)
                {
                    GameUnit tileGameUnit = tile.Unit.Value;
                    if (nextTile == null)
                    {
                        Debug.Log("Fall Down");
                        tileGameUnit.OnKnockDown.Execute(knockBackDirection);
                        tileGameUnit.Damage(tileGameUnit.Health.Value);
                    }
                    else if (nextTile.HasUnit)
                    {
                        nextTile.Unit.Value.Damage(1);
                        tile.Unit.Value.OnKnockback.Execute(knockBackDirection);
                    }
                    else
                    {
                        nextTile.MoveUnit(tile.Unit.Value);
                    }
                }
            }
        }
        
        public void DropXP(Tile dropTile, int amount)
        {
            XpAnimationCommand.Execute((dropTile, amount));
        }

        public void AddXP(int amount)
        {
            Xp.Value += amount;
            Level.Value = WeaponHelper.GetLevel(Xp.Value);
        }
        
        public bool TryMoveInDirection(Vector2 swipeDirection)
        {
            Tile swipedTile = CalculateDirectionNeighbour(swipeDirection);
            if(swipedTile == null || swipedTile.HasUnit)
            {
                return false;
            }

            FacingDirection = (swipedTile.FlatPosition - Tile.Value.FlatPosition).normalized;
            swipedTile.MoveUnitWithAction(this);
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
            float length = (Tile.Value.FlatPosition - Tile.Value.Neighbours[0].FlatPosition).magnitude;
            worldSwipeDirection *= length;

            Tile closestNeighbour = IslandHelper.GetTileInDirection(Tile.Value, worldSwipeDirection);
            return closestNeighbour;
        }
    }
}