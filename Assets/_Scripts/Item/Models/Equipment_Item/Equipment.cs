using UniRx;
using UnityEngine;

namespace Models
{
    public class Equipment : Item
    {
        public int AttackBonus;
        public int ArmorBonus;
        public int HealthBonus;
        
        public ReactiveProperty<Mod> InfusedMod = new();
        
        public Equipment(ItemType type, int stack = 1) : base(type, stack)
        {
            
        }

        public void InfuseMod(Mod mod)
        {
            if (InfusedMod.Value != null)
            {
                Debug.Log("Already Equipped A Mod");
                return;
            }
            
            InfusedMod.Value = mod;
        }
    }
}