using System.Linq;
using Models;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

namespace Views
{
    public class HealthRenderer : MonoBehaviour
    {
        [SerializeField] float MinWidth = 140f;
        [SerializeField] float MaxWidth = 13.7f;
        
        [FormerlySerializedAs("healthImages")] [SerializeField] private HeartUIView[] healthViews;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color inactiveColor;
        [SerializeField] private Color poisonColor;
        
        [SerializeField] private float dropIntensity;
        [SerializeField] private float dropSpray;
        [SerializeField] private float dropDuration;
        [SerializeField] private AnimationCurve dropCurve;
        [SerializeField] private AnimationCurve scaleCurve;
        
        private float _cellWidth;

        private void Awake()
        {
            _cellWidth = GetComponent<RectTransform>().sizeDelta.x;
        }

        public void Render(GameUnit gameUnit)
        {
            for (int i = 0; i < healthViews.Length; i++)
            {
                //SetRectSize(healthImages[i], unit.MaxHealth);
                //healthImages[i].color = activeColor;

                bool disableCell = i >= gameUnit.MaxHealth && healthViews[i].gameObject.activeSelf;
                bool showCell = i < gameUnit.MaxHealth && !healthViews[i].gameObject.activeSelf;
                bool dropCellAnimation = i >= gameUnit.Health.Value && healthViews[i].gameObject.activeSelf && gameUnit.Health.Value > 1;
                
                healthViews[i].Health.gameObject.SetActive(gameUnit.Health.Value > i);

                if (disableCell)
                {
                    healthViews[i].gameObject.SetActive(false);
                }
                else if (showCell)
                {
                    healthViews[i].gameObject.SetActive(true);
                }
                else if (dropCellAnimation)
                {
                    DropCellAnimation(healthViews[i].Health);
                }
            }
            
            RenderPoison(gameUnit);
        }

        private void DropCellAnimation(Image image)
        {
            Sequence sequence = DOTween.Sequence();
            Vector3 randomOffset = Random.value * Vector3.right * dropSpray;
            Vector3 heightOffset = Vector3.up * dropIntensity;
            
            Transform imageTransform = image.transform;
            if(imageTransform == null || image == null)
            {
                return;
            }

            Vector3 startLocalPosition = imageTransform.localPosition;
            
            sequence.Insert(0f, DOTween.To(() => 0f, t =>
            {
                imageTransform.localPosition =
                    Vector3.Lerp(startLocalPosition, startLocalPosition + heightOffset, dropCurve.Evaluate(t)) +
                    Vector3.Lerp(startLocalPosition, startLocalPosition + randomOffset, t);

                imageTransform.localScale = Vector3.LerpUnclamped(Vector3.one, Vector3.zero, scaleCurve.Evaluate(t));
            }, 1f, dropDuration)
                .OnComplete(() =>
                {
                    image.gameObject.SetActive(false);
                    imageTransform.localPosition = startLocalPosition;
                    imageTransform.localScale = Vector3.one;
                }));
        }

        private void RenderPoison(GameUnit gameUnit)
        {
            StatusEffect poisonEffect = gameUnit.ActiveStatusEffects.FirstOrDefault(e => e is PoisonEffect);
            if (poisonEffect != null && poisonEffect is PoisonEffect poison)
            {
                int duration = poison.Duration.Value;
                for (int i = healthViews.Length - 1; i >= 0; i--)
                {
                    if(i >= gameUnit.Health.Value)
                    {
                        continue;
                    }

                    if (duration > 0)
                    {
                        healthViews[i].PoisonOverlay.SetActive(gameUnit.Health.Value > i);
                        duration--;
                    }
                }
            }
        }
    }
}