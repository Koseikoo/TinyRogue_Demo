using System.Collections.Generic;
using Models;
using UniRx;
using UnityEngine;
using DG.Tweening;
using Factories;
using Installer;
using UnityEngine.Serialization;
using Zenject;

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
        
        [Inject] private LootFactory _lootFactory;

        private Bag _bag;

        public void Initialize(Bag bag)
        {
            _bag = bag;
            _bag.SetBagPoint(lootTransform);

            IslandLootContainer.DropLoot.Subscribe(_ =>
            {
                List<Loot> toRemove = new();
                
                foreach (Loot loot in IslandLootContainer.DroppedLoot)
                {
                    if (loot.RewardedTo == _bag.Owner)
                    {
                        NewVisualLootClaim(loot);
                        toRemove.Add(loot);
                    }
                }
                IslandLootContainer.DroppedLoot.Remove(toRemove);
                
            }).AddTo(this);
        }

        private void NewVisualLootClaim(Loot loot)
        {
            List<LootView> lootViews = GetLootViews(loot);
            Vector3 bagPosition = lootTransform.position;
            Vector3 anchorPointPosition = Vector3.Lerp(loot.DropPosition, bagPosition, anchorPoint);
            Vector3 baseArcVector = (Vector3.up * arcSize) * anchorPointDistance;
            float claimDuration = claimMult * (bagPosition - loot.DropPosition).magnitude;
            
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < lootViews.Count; i++)
            {
                Vector3 randomArcVector = baseArcVector.RotateVector((bagPosition - loot.DropPosition).normalized, Random.Range(-60f, 60f));
                int index = i;
                sequence.Insert(0, DOTween.To(() => 0f, t =>
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
                    }));
            }
            
            sequence.Insert(claimDuration, DOTween.To(() => 0f, t =>
            {
                bagVisual.localScale = Mathf.Lerp(1f, maxBagScale, bagScaleCurve.Evaluate(t)) * Vector3.one;
            }, 1f, claimDuration));
            
            sequence.InsertCallback(claimDuration, () =>
            {
                foreach (var view in lootViews)
                    view.ResetView();

                _bag.AddLoot(loot);
                
            });
        }

        private void VisualizeLootClaim(Loot loot)
        {
            List<LootView> lootViews = GetLootViews(loot);
            List<float> lootAngles = CalculateLootAngles(lootViews);

            var startPosition = loot.DropPosition;
            var endPosition = lootTransform.position;
            
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < lootViews.Count; i++)
            {
                int index = i;
                sequence.Insert(0f, DOTween.To(() => 0f, t =>
                {
                    Debug.Break();
                    var position = MathHelper.BezierLerp(loot.DropPosition, CalculateArcPosition(lootAngles[index]),
                        lootTransform.position, t);
                    lootViews[index].transform.position = position;
                }, 1f, .5f));
                
                //sequence.Insert(0f, lootViews[i].transform.DOMove(lootTransform.position, .5f));
            }

            sequence.OnComplete(() =>
            {
                foreach (var view in lootViews)
                    view.ResetView();

                _bag.AddLoot(loot);
                lootFX.Play();
            });
            
            Vector3 CalculateArcPosition(float angle)
            {
                var arcPivot = Vector3.Lerp(startPosition, endPosition, anchorPoint);
                var arcPoint = arcPivot + (Vector3.up * arcSize);
                var arcAxis = (endPosition - startPosition).normalized;

                var rotatedPosition = MathHelper.RotatePointAroundPoint(arcPoint, arcPivot, arcAxis, angle);
                return rotatedPosition;
            }
        }

        private List<float> CalculateLootAngles(List<LootView> lootViews)
        {
            List<float> angles = new();
            var anglePerItem = angleRange / lootViews.Count;
            var currentAngle = (anglePerItem * .5f) * (lootViews.Count - 1) - (angleRange * .5f);

            for (int i = 0; i < lootViews.Count; i++)
            {
                angles.Add(currentAngle);
                currentAngle += anglePerItem;
            }

            return angles;
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