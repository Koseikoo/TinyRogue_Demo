using System;
using UniRx;
using Zenject;

public enum ItemType
{
    // Equipment
    StartArmorEquipment = 8,
    StartHelmetEquipment = 9,
    StartFaceEquipment = 10,
    
    // Mods
    DamageMod = 0,
    PoisonMod = 6,
    
    // Items
    TestRelic = 1,
    HealthPotion = 7,
    
    // Resources
    MonsterResource = 2,
    PlantResource = 3,
    StoneResource = 4,
    WoodResource = 5,
}

namespace Models
{
    public class Item
    {
        public int Value => Type.GetValue();
        public ItemType Type;
        public IntReactiveProperty Stack = new();

        public Item(ItemType type, int stack)
        {
            Type = type;
            Stack.Value = stack;
        }
    }
}