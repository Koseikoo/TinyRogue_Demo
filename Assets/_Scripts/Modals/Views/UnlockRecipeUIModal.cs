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

        private Tween outerRotation;
        private Tween innerRotation;
        private Tween outerScale;
        private Tween innerScale;
        private Tween unlockSlot;

        public void Initialize(ItemType unlockedItem)
        {
            GameStateContainer.Player.IsDead.Where(b => b).Subscribe(_ => DestroyModal()).AddTo(this);
            
            unlockText.text = $"Unlocked {unlockedItem.ToString()}!";
            unlockedRecipeImage.sprite = _itemIconContainer.GetItemIcon(unlockedItem);
            TriggerAnimation();
        }

        public void DestroyModal()
        {
            Destroy(gameObject);
            
            outerRotation?.Kill();
            innerRotation?.Kill();
            outerScale?.Kill();
            innerScale?.Kill();
            unlockSlot?.Kill();
        }

        private void TriggerAnimation()
        {
            outerRotation = outerHighlight.transform.DORotate(Vector3.forward * 180, outerRotationSpeed).SetEase(Ease.Linear).SetLoops(-1);
            innerRotation = innerHighlight.transform.DORotate(Vector3.forward * 180, innerRotationSpeed).SetEase(Ease.Linear).SetLoops(-1);
            
            outerScale = outerHighlight.transform.DOScale(Vector3.one * scaleValues.y, scaleDuration).From(Vector3.one * scaleValues.x).SetEase(SlotScaleCurve).SetLoops(-1);
            innerScale = innerHighlight.transform.DOScale(Vector3.one * scaleValues.y, scaleDuration).From(Vector3.one * scaleValues.x).SetEase(SlotScaleCurve).SetLoops(-1);
            unlockSlot = UnlockSlot.DOScale(Vector3.one * scaleValues.y, scaleDuration).From(Vector3.one * scaleValues.x).SetEase(SlotScaleCurve).SetLoops(-1);
        }
    }
}