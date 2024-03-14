using System;
using UniRx;

namespace Models
{
    public class Slot
    {
        public ReactiveProperty<Item> Item = new();

        public bool IsOccupied => Item.Value != null;
        public BoolReactiveProperty IsSelected = new();
        public BoolReactiveProperty IsLocked = new();

        public void SetItem(Item item)
        {
            if (Item.Value != null)
            {
                if (Item.Value.Type == item.Type)
                    Item.Value.Stack.Value += item.Stack.Value;
                else
                    throw new Exception("Slot Already Occupied");
            }
            else
            {
                Item.Value = item;
            }
        }
        
        public void RemoveAmount(int amount)
        {
            Item.Value.Stack.Value -= amount;

            if (Item.Value.Stack.Value < 0)
                throw new Exception("Slot Item Amount is Negative!");
            
            if (Item.Value.Stack.Value == 0)
                Item.Value = null;
        }

        public void TapSlot()
        {
            if(IsLocked.Value)
                return;
            
            var selectedSlot = GameStateContainer.SelectedSlot;
            if (selectedSlot == this)
            {
                UseItem();
                return;
            }

            if (selectedSlot == null)
            {
                if (IsOccupied)
                {
                    IsSelected.Value = true;
                    GameStateContainer.SelectedSlot = this;
                }
            }
            else
            {
                if (!IsOccupied)
                {
                    SetItem(GameStateContainer.SelectedSlot.Item.Value);
                    GameStateContainer.SelectedSlot.IsSelected.Value = false;
                    GameStateContainer.SelectedSlot.RemoveItem();
                }
                else
                {
                    IsSelected.Value = true;
                    GameStateContainer.SelectedSlot.IsSelected.Value = false;
                    GameStateContainer.SelectedSlot = this;
                }
            }
        }
        
        public void RemoveItem()
        {
            Item.Value = null;
        }

        private void UseItem()
        {
            Action useAction = Item.Value.Type.GetItemUseAction();
            if (Item.Value.Stack.Value > 0 && useAction != null)
            {
                useAction.Invoke();
                Item.Value.Stack.Value--;
            }
        }
    }
}