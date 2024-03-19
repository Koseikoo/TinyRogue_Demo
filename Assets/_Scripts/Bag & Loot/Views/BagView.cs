using System;
using System.Collections.Generic;
using Models;
using UniRx;
using UnityEngine;
using DG.Tweening;
using Factories;
using Installer;
using UnityEngine.Serialization;
using Zenject;
using Random = UnityEngine.Random;

namespace Views
{
    public class BagView : MonoBehaviour
    {
        [SerializeField] private Transform lootTransform;
        [SerializeField] private Transform bagVisual;
        [SerializeField] private AnimationCurve bagScaleCurve;
        [SerializeField] private AnimationCurve claimCurve;
        [SerializeField] private float maxBagScale;
        [SerializeField] private float scaleDuration;
        [SerializeField] private float claimMult;
        [SerializeField] private float arcSize;
        [Range(0, 180)][SerializeField] private float angleRange;
        [Range(0, 1)][SerializeField] private float anchorPoint;
        [SerializeField] private float lootAnimationDelay;
        [SerializeField] private float anchorPointDistance;
        [SerializeField] private ParticleSystem lootFX;
        [SerializeField] private float lootSequenceDuration;
        
        [Inject] private LootFactory _lootFactory;

        private Bag _bag;

        public void Initialize(Bag bag)
        {
            _bag = bag;
            _bag.SetBagPoint(lootTransform);

            WorldLootContainer.DropLoot.Subscribe(_ =>
            {
                List<Loot> toRemove = new();
                
                foreach (Loot loot in WorldLootContainer.DroppedLoot)
                {
                    if (loot.RewardedTo == _bag.Owner)
                    {
                        VisualLootClaim(loot);
                        toRemove.Add(loot);
                    }
                }
                WorldLootContainer.DroppedLoot.Remove(toRemove);
                
            }).AddTo(this);

            if (_bag.Owner is Player)
            {
                WorldLootContainer.ClaimMerchantCoins.Subscribe(_ => MerchantGoldClaim()).AddTo(this);
            }
            
            
        }

        private void MerchantGoldClaim()
        {
            List<GoldCoinView> droppedCoins = _lootFactory.GetDroppedGoldCoin();
            
            float delay = 0;
            float delayAdd = lootSequenceDuration / droppedCoins.Count;

            Tween bagTween = null;
            
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < droppedCoins.Count; i++)
            {
                droppedCoins[i].MerchantDrop = false;
                Vector3 coinPosition = droppedCoins[i].transform.position;
                
                Vector3 bagPosition = lootTransform.position;
                Vector3 anchorPointPosition = Vector3.Lerp(coinPosition, bagPosition, anchorPoint);
                Vector3 baseArcVector = (Vector3.up * arcSize) * anchorPointDistance;
                float claimDuration = claimMult;
                
                Vector3 randomArcVector = baseArcVector.RotateVector((bagPosition - coinPosition).normalized, Random.Range(-60f, 60f));
                int index = i;
                sequence.Insert(delay, DOTween.To(() => 0f, t =>
                    {
                        var position = MathHelper.BezierLerp(coinPosition,
                            anchorPointPosition + randomArcVector,
                            bagPosition, claimCurve.Evaluate(t));
                        droppedCoins[index].transform.position = position;
                    }, 1f, claimDuration)
                    .OnComplete(() =>
                    {
                        droppedCoins[index].ResetView();
                        lootFX.Play();
                        _bag.AddGold(1);
                        droppedCoins[index].ResetView();

                        if (bagTween != null)
                            bagTween.Kill();
                        
                        bagTween = bagVisual.DOScale(maxBagScale, claimDuration)
                            .From(Vector3.one)
                            .SetEase(bagScaleCurve);
                        
                    }));
                delay += delayAdd;
            }
        }

        private void VisualLootClaim(Loot loot)
        {
            List<LootView> lootViews = GetLootViews(loot);
            
            Vector3 bagPosition = lootTransform.position;
            Vector3 anchorPointPosition = Vector3.Lerp(loot.DropPosition, bagPosition, anchorPoint);
            Vector3 baseArcVector = (Vector3.up * arcSize) * anchorPointDistance;
            float claimDuration = claimMult;
            float delay = 0;
            float delayAdd = lootSequenceDuration / lootViews.Count;
            
            Tween bagTween = null;
            
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < lootViews.Count; i++)
            {
                Vector3 randomArcVector = baseArcVector.RotateVector((bagPosition - loot.DropPosition).normalized, Random.Range(-60f, 60f));
                int index = i;
                Item item = GetLootItemBasedOnIndex(loot, index);
                sequence.Insert(delay, DOTween.To(() => 0f, t =>
                    {
                        var position = MathHelper.BezierLerp(loot.DropPosition,
                            anchorPointPosition + randomArcVector,
                            bagPosition, claimCurve.Evaluate(t));
                        lootViews[index].transform.position = position;
                    }, 1f, claimDuration)
                    .OnComplete(() =>
                    {
                        lootViews[index].ResetView();
                        lootFX.Play();
                        AddItemToBag(item);
                        
                        if (bagTween != null)
                            bagTween.Kill();
                        
                        bagTween = bagVisual.DOScale(maxBagScale, claimDuration)
                            .From(Vector3.one)
                            .SetEase(bagScaleCurve);
                    }));
                delay += delayAdd;
            }
        }

        private void AddItemToBag(Item item)
        {
            if(item == null)
                _bag.AddGold(1);
            else if(item is Mod mod)
                _bag.AddMod(mod);
            else if(item is Resource resource)
                _bag.AddResource(resource);
            else if(item is Equipment equipment)
                _bag.AddEquipment(equipment);
            
            _bag.AddItem(item);
        }

        private Item GetLootItemBasedOnIndex(Loot loot, int index)
        {
            int goldCoins = loot.Gold;
            int mods = loot.Mods.Count;
            int items = loot.Items.Count;
            int resources = loot.Resources.Count;

            if (index < goldCoins)
            {
                return null;
            }

            if (index < goldCoins + mods)
            {
                return loot.Mods[index - goldCoins];
            }

            if (index < goldCoins + mods + items)
            {
                return loot.Items[index - (goldCoins + mods)];
            }
            
            if (index < goldCoins + mods + items + resources)
            {
                return loot.Resources[index - (goldCoins + mods + items)];
            }

            throw new Exception("Get Loot based on Index Overflow");


        }

        private List<LootView> GetLootViews(Loot loot)
        {
            List<LootView> lootViews = new();
            
            for (int i = 0; i < loot.Gold; i++)
            {
                lootViews.Add(_lootFactory.CreateLootView(LootType.Gold, loot.DropPosition));
            }

            for (int i = 0; i < loot.Mods.Count; i++)
            {
                lootViews.Add(_lootFactory.CreateLootView(LootType.Mod, loot.DropPosition));
            }
            
            for (int i = 0; i < loot.Items.Count; i++)
            {
                lootViews.Add(_lootFactory.CreateLootView(LootType.Item, loot.DropPosition));
            }
            
            for (int i = 0; i < loot.Resources.Count; i++)
            {
                lootViews.Add(_lootFactory.CreateLootView(LootType.Resource, loot.DropPosition));
            }

            return lootViews;
        }
    }
}