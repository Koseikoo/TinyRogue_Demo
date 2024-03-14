using System.Collections.Generic;
using UniRx;

namespace Models
{
    public class ModSmith
    {
        public const int ModSellSlots = 3;
        
        
        public ReactiveCollection<Slot> SellSlots = new();
        public BoolReactiveProperty InTrade = new();
        
        private bool _sellModsCreated;


        public ModSmith()
        {
            for (int i = 0; i < ModSellSlots; i++)
                SellSlots.Add(new());
        }

        public void StartTrade(Player player)
        {
            GetRandomItems();
            InTrade.Value = true;
        }

        public void EndTrade()
        {
            InTrade.Value = false;
        }

        private void GetRandomItems()
        {
            if(_sellModsCreated)
                return;

            _sellModsCreated = true;
            for (int i = 0; i < SellSlots.Count; i++)
            {
                ItemType modType = PersistentPlayerState.UnlockedEquipmentRecipes.PickRandom().Output;
                SellSlots[i].SetItem(modType.GetModInstance(1));
            }
        }
    }
}