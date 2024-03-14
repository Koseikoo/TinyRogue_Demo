using DG.Tweening;
using Installer;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views
{
    public class UnlockRecipeUIModal : MonoBehaviour
    {
        [SerializeField] private float innerRotationSpeed;
        [SerializeField] private float outerRotationSpeed;
        [SerializeField] private float scaleDuration;
        [SerializeField] private Vector2 scaleValues;
        [SerializeField] private Image innerHighlight;
        [SerializeField] private Image outerHighlight;
        [SerializeField] private AnimationCurve SlotScaleCurve;
        [SerializeField] private Transform UnlockSlot;
        [SerializeField] private TextMeshProUGUI unlockText;
        [SerializeField] private Image unlockedRecipeImage;

        [Inject] private ItemIconContainer _itemIconContainer;

        public void Initialize(ItemType unlockedItem)
        {
            unlockText.text = $"Unlocked {unlockedItem.ToString()}!";
            unlockedRecipeImage.sprite = _itemIconContainer.GetItemIcon(unlockedItem);
            TriggerAnimation();
        }

        public void DestroyModal()
        {
            Destroy(gameObject);
        }

        private void TriggerAnimation()
        {
            outerHighlight.transform.DORotate(Vector3.forward * 180, outerRotationSpeed).SetEase(Ease.Linear).SetLoops(-1);
            innerHighlight.transform.DORotate(Vector3.forward * 180, innerRotationSpeed).SetEase(Ease.Linear).SetLoops(-1);
            
            outerHighlight.transform.DOScale(Vector3.one * scaleValues.y, scaleDuration).From(Vector3.one * scaleValues.x).SetEase(SlotScaleCurve).SetLoops(-1);
            innerHighlight.transform.DOScale(Vector3.one * scaleValues.y, scaleDuration).From(Vector3.one * scaleValues.x).SetEase(SlotScaleCurve).SetLoops(-1);
            UnlockSlot.DOScale(Vector3.one * scaleValues.y, scaleDuration).From(Vector3.one * scaleValues.x).SetEase(SlotScaleCurve).SetLoops(-1);
        }
    }
}