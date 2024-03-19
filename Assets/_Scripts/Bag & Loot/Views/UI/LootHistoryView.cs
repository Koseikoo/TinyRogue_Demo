using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using UniRx;
using UnityEngine;

namespace Views
{
    public class LootHistoryView : MonoBehaviour
    {
        [SerializeField] private SlotUIRenderer[] renderSlots;

        private List<Item> _itemQueue = new();
        private Bag _playerBag;
        
        public void Initialize(Player player)
        {
            _playerBag = player.Bag;
            //_playerBag.OnLootAdded.Subscribe(_ => AddToLootQueue).AddTo(this);
            StartCoroutine(QueueVisualization());
        }

        private void Update()
        {
            
        }

        private void AddToLootQueue(Loot loot)
        {
            for (int i = 0; i < loot.Items.Count; i++)
                _itemQueue.Add(loot.Items[i]);

            for (int i = 0; i < loot.Mods.Count; i++)
                _itemQueue.Add(loot.Mods[i]);
            
            for (int i = 0; i < loot.Resources.Count; i++)
                _itemQueue.Add(loot.Resources[i]);
        }

        private IEnumerator QueueVisualization()
        {
            RenderItems();
            while (true)
            {
                yield return new WaitUntil(() => _itemQueue.Count > 0);
                RenderItems();
                
                yield return new WaitForSeconds(.6f);
                
                
                    
                _itemQueue.RemoveAt(0);
                RenderItems();
            }
        }

        private void RenderItems()
        {
            for (int i = 0; i < renderSlots.Length; i++)
            {
                if (i >= _itemQueue.Count)
                {
                    renderSlots[i].RenderEmpty();
                    continue;
                }
                        
                var item = _itemQueue[i];

                renderSlots[i].RenderItem(item.Type, item.Stack.Value);
            }
        }
    }
}