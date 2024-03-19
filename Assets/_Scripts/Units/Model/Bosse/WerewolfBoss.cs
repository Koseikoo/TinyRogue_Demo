using System.Collections.Generic;
using Container;
using Factories;
using UnityEngine;
using Zenject;

namespace Models
{
    public class WerewolfBoss : Enemy
    {
        private const int SlashRange = 2;
        private const int LeapRange = 4;
        private const int WolfSpawnRange = 3;
        private const int TeleportRange = 3;

        private const int LeapRecoverTurns = 2;
        
        private readonly int _defaultTurnDelay = 1;

        [Inject] private UnitFactory _unitFactory;
        [Inject] private UnitContainer _unitContainer;

        private List<Tile> _leapPath = new();
        private bool _enraged;
        private bool _leaped;
        private bool _slashed;
        private bool _damaged;

        public WerewolfBoss()
        {
            _defaultTurnDelay = TurnDelay;
        }
        protected override void EnemyAction()
        {
            var path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value, 
                IsAttackPathTile);
            
            if(path == null)
                return;

            if (TurnDelay == LeapRecoverTurns)
                TurnDelay = _defaultTurnDelay;

            if (path.Count <= LeapRange || _leapPath.Count > 0)
            {
                //if(_enraged)
                //    EnragedAction(path);
                DefaultAction(path);
            }
            else
            {
                this.FollowTarget(path[0]);
            }
        }

        protected override void RenderAttackPath()
        {
            if (!AimAtTarget.Value)
                return;

            var path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value);

            if (path == null)
                return;
            
            if (_damaged)
            {
                Debug.Log("Show Teleportation Indicator");
            }
            else if (path.Count <= SlashRange)
            {
                // Visualize Slash
                UpdateSelectedTiles(path, TileSelectionType.Attack);
            }
            else if (path.Count <= LeapRange) // if in leap range => aim and leap
            {
                // Visualize Leap
                UpdateSelectedTiles(path, TileSelectionType.Attack);
                _leapPath = path;
            }
            else if (!path[0].HasUnit)
            {
                UpdateSelectedTiles(new() { path[0] }, TileSelectionType.Move);
                NextMoveTile = path[0];
            }
        }

        public override void Attack(IEnumerable<Mod> mods, Vector3 attackVector, Unit attacker = null)
        {
            base.Attack(mods, attackVector, attacker);
            _damaged = true;
            if ((float)Health.Value / MaxHealth <= .6f)
                _enraged = true;
        }

        private void DefaultAction(List<Tile> path)
        {

            if (path.Count <= SlashRange)
            {
                // slash player
                SlashAttack(path);
                _slashed = true;
                
            }
            else if (_damaged)
            {
                // Teleport Away
                TeleportOutOfRange();
                SpawnWolf();
                _damaged = false;
            }
            else if (_leapPath.Count > 0) // if in leap range => aim and leap
            {
                LeapAttack(_leapPath);
                _leapPath.Clear();
                _leaped = true;
            }
        }

        private void EnragedAction(List<Tile> path)
        {
            // if in stab Range => Stab
            
            // if in leap range => aim and leap => Stab after leap
            
            // if just leap-stabbed => Teleport away and Spawn Wolf
        }

        private void LeapAttack(List<Tile> path)
        {
            // if in leap range
            int jumpIndex = path.Count - 1;
            bool attack = false;

            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (path[i].Unit.Value == AttackTarget)
                {
                    jumpIndex = i - 1;
                    attack = true;
                }
            }
            
            if (jumpIndex >= 0 && !path[jumpIndex].HasUnit)
            {
                path[jumpIndex].MoveUnit(this);
            }

            if (attack)
            {
                this.AttackUnit(AttackTarget);
                AnimationCommand.Execute(AnimationState.Attack3);
            }

            TurnDelay = Mathf.RoundToInt(Random.value) + 1;
            AimAtTarget.Value = false;
        }

        private void SlashAttack(List<Tile> path)
        {
            // if in stab range
            this.TryAttackTarget(path);
            AnimationCommand.Execute(AnimationState.Attack1);
        }

        private void SpawnWolf()
        {
            // Spawn Wolf on nearby Tile

            Tile wolfSpawnTile = Tile.Value.Island.Tiles
                .GetTilesWithinDistance(Tile.Value, WolfSpawnRange)
                .WithoutUnitOnTile()
                .PickRandom();

            var wolfDefinition = _unitContainer.GetEnemyDefinition(UnitType.WolfEnemy);
            var wolf = _unitFactory.CreateEnemy(wolfDefinition, wolfSpawnTile);
            wolf.State.Value = EnemyState.TargetFound;
        }

        private void TeleportOutOfRange()
        {
            // after leap (if enraged after stab attack)
            List<Tile> teleportTiles = Island.Tiles.GetTilesWithinDistance(Tile.Value, TeleportRange)
                .GetTilesOutsideOfDistance(Tile.Value, TeleportRange-2)
                .WithoutUnitOnTile();
                
            teleportTiles.PickRandom().MoveUnit(this);
        }
        
        
    }
}