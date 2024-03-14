using Models;
using UnityEngine;
using Zenject;

namespace Factories
{
    public class FeedbackFactory
    {
        [Inject] private DiContainer _container;
        [Inject] private WeaponBoundFeedbackView _weaponBoundFeedbackPrefab;

        public WeaponBoundFeedbackView CreateWeaponBoundFeedback(Transform weaponView, Transform playerView)
        {
            WeaponBoundFeedbackView view = _container.InstantiatePrefab(_weaponBoundFeedbackPrefab)
                .GetComponent<WeaponBoundFeedbackView>();
            view.Initialize(weaponView, playerView);
            return view;
        }
    }
}