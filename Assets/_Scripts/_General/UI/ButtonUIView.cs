using System;
using DG.Tweening;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views
{
    public class ButtonUIView : MonoBehaviour
    {
        private const float PressedPositionY = 0;
        private const float IdlePositionY = 25;

        [Inject] private CameraModel _cameraModel;
        
        public Button InteractionButton;
        
        [SerializeField] private GameObject _visual;
        [SerializeField] private TextMeshProUGUI _interactionText;
        [SerializeField] private Animator _animator;
        [SerializeField] private RectTransform _buttonRect;
        [SerializeField] private Image _innerBorder;
        [SerializeField] private float _buttonPressDuration;
        
        private Sequence _downSequence;
        private Sequence _upSequence;
        private bool _buttonPressed;
        
        private void Update()
        {
            float lerp = Mathf.InverseLerp(IdlePositionY, PressedPositionY, _buttonRect.anchoredPosition.y);
            _innerBorder.fillAmount = lerp;
            
            if(_buttonPressed)
                ClearOldTween();
        }

        public void SetText(string text)
        {
            _interactionText.text = text;
        }
        
        public void ToggleInteractionUI(bool show)
        {
            InteractionButton.interactable = show;
            _visual.gameObject.SetActive(show);
        }

        public void TriggerButtonEvent(Action buttonLogic)
        {
            ClearOldTween();
            if(_buttonPressed)
                return;
            _downSequence = DOTween.Sequence();
            
            
            
            _downSequence.Insert(0f, _buttonRect.DOAnchorPos(new Vector2(0f, PressedPositionY), _buttonPressDuration)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    _cameraModel.UnitDeathShakeCommand.Execute();
                    _animator.SetTrigger("Shrink");
                    _downSequence = null;
                    _buttonPressed = true;
                    
                }));
            _downSequence.AppendInterval(.18f)
                .OnComplete(() => buttonLogic?.Invoke());

        }

        public void EndButtonEvent()
        {
            ClearOldTween();
            if(_buttonPressed)
                return;
            
            _upSequence = DOTween.Sequence();
            _upSequence.Insert(0f, _buttonRect.DOAnchorPos(new Vector2(0f, IdlePositionY), _buttonPressDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    _upSequence = null;
                }));
        }

        private void ClearOldTween()
        {
            _downSequence?.Kill();
            _upSequence?.Kill();

            _downSequence = null;
            _upSequence = null;
        }
    }
}