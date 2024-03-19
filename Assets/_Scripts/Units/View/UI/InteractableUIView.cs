using System;
using Views;
using DG.Tweening;
using Models;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views
{
    public class InteractableUIView : MonoBehaviour
    {
        private Interactable _interactable;
        
        [SerializeField] private ButtonUIView _buttonUIView;

        public void Initialize(Interactable interactable)
        {
            _interactable = interactable;
            _interactable.InInteractionRange
                .Subscribe(_buttonUIView.ToggleInteractionUI)
                .AddTo(this);

            _interactable.IsDead.Where(b => b).Subscribe(_ => Destroy(gameObject)).AddTo(this);
            _interactable.IsDestroyed.Where(b => b).Subscribe(_ => Destroy(gameObject)).AddTo(this);
            
            _buttonUIView.Initialize(() => _interactable.InteractionLogic?.Invoke(_interactable),
                _interactable.InteractButtonText);
        }

    }
}