using System;
using System.Collections.Generic;
using Models;
using UniRx;
using UnityEngine;

namespace TinyRogue
{
    public enum WeaponType
    {
        None,
        SingleSword,
        SwordAndShield,
        BowAndArrow,
        DualWield,
        Hammer
    }
    public class PlayerAnimationView : MonoBehaviour, IView<Player>
    {
        //BaseSword,
        //BaseBow,
        //BaseHammer,
        //BaseShield,
            
            
        [SerializeField]
        private Animator animator;
        [Space]
        [SerializeField] RuntimeAnimatorController noWeaponController;
        [SerializeField] RuntimeAnimatorController singleSwordController;
        [SerializeField] RuntimeAnimatorController singleSwordPiercingController;

        [Header("Weapons")]
        [SerializeField] private GameObject baseSword;
        [SerializeField] private GameObject longSword;
        [SerializeField] private GameObject baseBow;
        [SerializeField] private GameObject baseHammer;
        [SerializeField] private GameObject baseShield;
        
        private Player _model;
        private Dictionary<WeaponName, GameObject> _rightHandWeaponDict = new();
        private IDisposable _weaponUnlockSubscription;

        private void Awake()
        {
            _rightHandWeaponDict[WeaponName.BaseSword] = baseSword;
        }

        public void Initialize(Player model)
        {
            _model = model;
            model.Weapon
                .Where(data => data != null)
                .Subscribe(data =>
                    {
                        UpdateSubscription(data);
                        UpdatePrefab(data);
                    })
                .AddTo(this);
        }

        private void UpdatePrefab(WeaponData data)
        {
            UpdateAnimationController();
            DisableAllWeapons();
            EnableWeapon(data);
        }

        private void UpdateSubscription(WeaponData data)
        {
            _weaponUnlockSubscription?.Dispose();
            _weaponUnlockSubscription = data.UnlockedSkills
                .ObserveAdd()
                .Subscribe(_ => UpdatePrefab(_model.Weapon.Value));
        }

        private void UpdateAnimationController()
        {
            WeaponData weapon = _model.Weapon.Value;
            switch (weapon.GetWeaponType())
            {
                case WeaponType.None:
                    animator.runtimeAnimatorController = noWeaponController;
                    break;
                
                case WeaponType.SingleSword:
                    animator.runtimeAnimatorController = weapon.Piercing
                        ? singleSwordPiercingController
                        : singleSwordController;
                    break;
                
                case WeaponType.SwordAndShield:
                    break;
                
                case WeaponType.BowAndArrow:
                    break;
                
                case WeaponType.DualWield:
                    break;
                
                case WeaponType.Hammer:
                    break;
            }
        }

        private void EnableWeapon(WeaponData data)
        {
            if(_rightHandWeaponDict.TryGetValue(data.Name, out GameObject weapon))
            {
                weapon.SetActive(true);
            }
        }
        
        private void DisableAllWeapons()
        {
            foreach (KeyValuePair<WeaponName,GameObject> kvp in _rightHandWeaponDict)
            {
                kvp.Value.SetActive(false);
            }
        }
    }
}