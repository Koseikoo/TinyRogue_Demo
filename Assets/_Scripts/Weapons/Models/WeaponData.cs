using System;
using System.Collections.Generic;
using System.Linq;
using TinyRogue;
using UniRx;
using UnityEngine;

namespace Models
{
    public enum WeaponName
    {
        None,
        BaseSword,
        BaseBow,
        BaseHammer,
        BaseShield,
    }
    public class WeaponData
    {
        public WeaponName Name;
        public int Damage;
        
        public Func<Vector3, List<Tile>> AttackPattern { get; private set; }
        
        private bool PlayerHasSword => Name.ToString().ToLower().Contains("sword");
        private bool PlayerHasBow => Name.ToString().ToLower().Contains("bow");
        private bool PlayerHasHammer => Name.ToString().ToLower().Contains("hammer");
        
        

        public WeaponData(WeaponName name, Func<Vector3, List<Tile>> attackPattern, int damage)
        {
            Name = name;
            Damage = damage;
            SetAttackPattern(attackPattern);
        }

        public void SetAttackPattern(Func<Vector3, List<Tile>> attackPattern)
        {
            AttackPattern = attackPattern;
        }

        public List<Tile> GetAimedTiles(Vector3 direction)
        {
            return AttackPattern(direction);
        }
        
        public WeaponType GetWeaponType()
        {
            WeaponType weaponType = WeaponType.None;
            if (PlayerHasSword)
            {
                weaponType = WeaponType.SingleSword;
            }
            else if(PlayerHasBow)
            {
                weaponType = WeaponType.BowAndArrow;
            }
            else if(PlayerHasHammer)
            {
                weaponType = WeaponType.Hammer;
            }

            return weaponType;
        }
    }
}