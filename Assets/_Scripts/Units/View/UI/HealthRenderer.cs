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

        public void RenderNew(Unit unit)
        {
            for (int i = 0; i < healthViews.Length; i++)
            {
                //SetRectSize(healthImages[i], unit.MaxHealth);
                //healthImages[i].color = activeColor;

                bool disableCell = i >= unit.MaxHealth && healthViews[i].gameObject.activeSelf;
                bool showCell = i < unit.Health.Value && !healthViews[i].gameObject.activeSelf;
                bool dropCellAnimation = i >= unit.Health.Value && healthViews[i].gameObject.activeSelf;

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
            
            RenderPoison(unit);
        }

        private void DropCellAnimation(Image image)
        {
            Sequence sequence = DOTween.Sequence();
            Vector3 randomOffset = Random.value * Vector3.right * dropSpray;
            Vector3 heightOffset = Vector3.up * dropIntensity;
            Transform imageTransform = image.transform;
            Vector3 startPosition = imageTransform.position;
            
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

        private void SetRectSize(Image image, int maxHealth)
        {
            var rect = image.transform.parent.GetComponent<RectTransform>();
            var size = rect.sizeDelta;
            size.x = MinWidth / maxHealth;
            rect.sizeDelta = size;
        }

        public void Render(Unit unit)
        {
            gameObject.SetActive(true);

            for (int i = 0; i < healthViews.Length; i++)
            {
                if (i < unit.MaxHealth)
                {
                    healthViews[i].Health.enabled = unit.Health.Value > i;

                    var rect = healthViews[i].GetComponent<RectTransform>();
                    var size = rect.sizeDelta;
                    size.x = MinWidth / unit.MaxHealth;
                    rect.sizeDelta = size;
                    
                    healthViews[i].gameObject.SetActive(true);

                    float scale = (i + 1 - unit.Health.Value) / (float)unit.MaxHealth;
                    if (unit.Health.Value == 0)
                        scale = 1;
                    float endScale = unit.Health.Value > i ? 1 : 1-scale;
                    
                    healthViews[i].transform.DOScale(Vector3.one * endScale, .3f);
                }
                else
                {
                    healthViews[i].gameObject.SetActive(false);
                }
            }
            
            RenderPoison(unit);
        }

        public void ResetRenderer()
        {
            gameObject.SetActive(false);
            DisableSprites();
        }

        private void DisableSprites()
        {
            for (int i = 0; i < healthViews.Length; i++)
            {
                healthViews[i].gameObject.SetActive(false);
            }
        }

        private void RenderPoison(Unit unit)
        {
            var poisonEffect = unit.ActiveStatusEffects.FirstOrDefault(e => e is PoisonEffect);
            if (poisonEffect != null && poisonEffect is PoisonEffect poison)
            {
                int duration = poison.Duration.Value;
                for (int i = healthViews.Length - 1; i >= 0; i--)
                {
                    if(i >= unit.Health.Value)
                        continue;

                    if (duration > 0)
                    {
                        healthViews[i].PoisonOverlay.SetActive(unit.Health.Value > i);
                        duration--;
                    }
                }
            }
        }
    }
}