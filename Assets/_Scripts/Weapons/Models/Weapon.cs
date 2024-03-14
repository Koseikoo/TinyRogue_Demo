using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;
using DG.Tweening;
using Factory;
using Views;

public enum WeaponType
{
    TestWeapon,
    Sword
}

namespace Models
{
    public class Weapon : IAttacker, IDisposable
    {
        public const int MaxCombo = 5;
        public const float AttackAnimationDuration = .15f;
        
        private const int WeaponMods = 3;
        private const int RingDistanceInTiles = 2;

        [Inject] private CameraModel _cameraModel;
        [Inject] private ChoiceContainer _choiceContainer;
        [Inject] private ModalFactory _modalFactory;
        
        public Player Owner;
        public int Range;
        public int MaxAttackCharges;
        public int MaxMoveCharges;
        public IntReactiveProperty AttackCharges = new();
        public IntReactiveProperty MoveCharges = new();
        public ReactiveCollection<Unit> ActiveCombo = new();
        public ReactiveProperty<Tile> Tile = new();
        public ReactiveProperty<Vector3> AttackDirection = new();
        public ReactiveProperty<Vector3> AimedPoint = new();
        public BoolReactiveProperty IsDestroyed = new();
        
        public bool FixToHolster = new();
        public bool BounceBack;

        public IntReactiveProperty Xp = new();
        public IntReactiveProperty Level = new();
        public List<Slot> ModSlots { get; set; } = new();
        public bool HasAttackCharge => AttackCharges.Value > 0;
        public bool HasMoveCharge => MoveCharges.Value > 0;
        
        private int _attackDirection;
        private bool _usedChargeLastTurn;

        private AnimationCurve _attackDelayCurve;
        private IDisposable _updatePositionSubscription;
        private IDisposable _levelUpSubscription;

        public Weapon(int range, int maxAttackCharges, int maxMoveCharges)
        {
            for (int i = 0; i < WeaponMods; i++)
                ModSlots.Add(new());
            ModSlots[0].IsLocked.Value = true;

            Level.Value = 1;
            
            Range = range;
            MaxAttackCharges = maxAttackCharges;
            AttackCharges.Value = maxAttackCharges;
            
            MaxMoveCharges = maxMoveCharges;
            MoveCharges.Value = maxMoveCharges;
            _attackDirection = 1;
            
            AnimationCurve curve = new AnimationCurve();

            curve.AddKey(new Keyframe(0, 0, 0, 3));
            curve.AddKey(new Keyframe(1, 1, 3, 0));

            _attackDelayCurve = curve;

            _updatePositionSubscription = GameStateContainer.TurnState
                .SkipLatestValueOnSubscribe()
                .Where(state => state == TurnState.EnemyTurn && GameStateContainer.GameState.Value == GameState.Island)
                .Subscribe(_ => UpdateAttackState());

            _levelUpSubscription = Level.SkipLatestValueOnSubscribe().Subscribe(_ => OpenLevelUpModal());
        }

        private void OpenLevelUpModal()
        {
            List<Choice> choices = new()
            {
                _choiceContainer.GetChoice(Choices.IncreaseHealth, () =>
                {
                    Owner.MaxHealth++;
                    Owner.Health.Value++;
                }),
                _choiceContainer.GetChoice(Choices.IncreaseDamage, () =>
                {
                    var attackMod = ModSlots[0].Item.Value as Mod;
                    attackMod.Power.Value++;
                })
            };
            _modalFactory.CreateChoiceModal(choices, false);
        }
        
        public void AddXp(int amount)
        {
            Xp.Value += amount;
            Level.Value = WeaponHelper.GetLevel(Xp.Value);
        }

        public void AttackTiles(List<Tile> tiles, Tile startTile)
        {
            if(tiles == null || tiles.Count == 0)
                throw new Exception("No Tiles To Attack!");
            
            if (!HasAttackCharge)
            {
                // TODO Add "No Charge Left" Feedback
                Debug.Log("Trigger 'no charge left' feedback here");
                return;
            }

            Sequence sequence = DOTween.Sequence();

            Move(tiles[^1]);
            GameStateContainer.TurnState.Value = TurnState.PlayerTurnEnd;
            
            for (int i = 0; i < tiles.Count; i++)
            {
                float t = Mathf.InverseLerp(0, 
                    (startTile.WorldPosition - tiles[^1].WorldPosition).magnitude, (startTile.WorldPosition - tiles[i].WorldPosition).magnitude);
                int index = i;
                sequence.InsertCallback(_attackDelayCurve.Evaluate(t) * AttackAnimationDuration,
                    () =>
                    {
                        AttackTile(tiles[index], AttackDirection.Value);
                    });
            }

            sequence.OnComplete(() =>
            {
                BounceBack = tiles[^1].HasAliveUnit;
                if (!HasAttackCharge || BounceBack)
                    ReturnToHolster();

            });
            
            sequence.Play();
        }

        public void ReturnToHolster()
        {
            Vector3 worldSwipeVector = Owner.Tile.Value.WorldPosition - Tile.Value.WorldPosition;

            List<Tile> allTiles = Tile.Value.TileCollection;
            List<Tile> tiles = allTiles
                .GetSwipedTiles(Owner.Tile.Value, Tile.Value)
                .GetTilesInWeaponRange(this);

            if (tiles.Count > 0)
            {
                AttackDirection.Value = worldSwipeVector.normalized;
                Move(Owner.Tile.Value);
                RecoverAttackCharge();
            }
            
            Tile.Value = Owner.Tile.Value;
        }


        public void RecoverAttackCharge()
        {
            AttackCharges.Value = Mathf.Clamp(AttackCharges.Value + 1, 0, MaxAttackCharges);
        }

        public void UseAttackCharge()
        {
            _usedChargeLastTurn = true;
            AttackCharges.Value = Mathf.Clamp(AttackCharges.Value - 1, 0, MaxAttackCharges);
        }
        
        public void Dispose()
        {
            _updatePositionSubscription.Dispose();
            _levelUpSubscription?.Dispose();
        }
        
        private void UpdateAttackState()
        {
            if(GameStateContainer.Player.InAttackMode.Value)
                GameStateContainer.Player.InAttackMode.Value = AttackCharges.Value > 0;
        }
        
        private void AttackTile(Tile tile, Vector3 attackDirection)
        {
            Unit tileUnit = tile.CurrentUnit.Value;
            if (tileUnit != null && tileUnit != GameStateContainer.Player) {
                tileUnit.Attack(ModSlots.GetMods(), attackDirection, Owner);
                _cameraModel.AttackShakeCommand.Execute();
                if (tileUnit.IsDead.Value && tileUnit is Enemy)
                {
                    RecoverAttackCharge();
                }
            }
        }
        
        public void Move(Tile tappedTile)
        {
            if (Tile.Value != null)
                Tile.Value.WeaponOnTile.Value = false;
            Tile.Value = tappedTile;
            Tile.Value.WeaponOnTile.Value = true;

            BounceBack = false;
        }
    }
}
