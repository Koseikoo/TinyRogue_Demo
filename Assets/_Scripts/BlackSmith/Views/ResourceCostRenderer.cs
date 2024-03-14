using System;
using System.Collections.Generic;
using Installer;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views
{
    public class ResourceCostRenderer : MonoBehaviour
    {
        [SerializeField] private Image resourceIcon;
        [SerializeField] private TextMeshProUGUI resourceAmount;
        
        [Inject] private ItemIconContainer _itemIconContainer;

        public void Render(ResourceCost cost)
        {
            resourceIcon.sprite = _itemIconContainer.GetItemIcon(cost.Type);
            int currentAmount = GetCurrentResources(cost.Type);
            resourceAmount.text = $"{cost.Amount} | {GetColoredString(cost.Amount, currentAmount)}";
        }

        private int GetCurrentResources(ItemType type)
        {
            int amount = 0;
            var matches = GameStateContainer.Player.Bag.Resources.MatchingItems(item => item.Type == type);
            foreach (Item match in matches)
            {
                amount += match.Stack.Value;
            }

            return amount;
        }

        private string GetColoredString(int requiredAmount, int currentAmount)
        {
            if(currentAmount >= requiredAmount)
                return $"<color=#94f549>{currentAmount}</color>";
            return $"<color=#f55b49>{currentAmount}</color>";
        }
    }
}