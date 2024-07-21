using System.Collections.Generic;
using Models;
using TinyRogue;
using UnityEngine;
using Zenject;

namespace Factories
{
    public class FeedbackFactory
    {
        [Inject] private DiContainer _container;
        [Inject] private WeaponBoundFeedbackView _weaponBoundFeedbackPrefab;
        [Inject] private AttackFeedbackView _attackFeedbackPrefab;

        public WeaponBoundFeedbackView CreateWeaponBoundFeedback(Transform weaponView, Transform playerView)
        {
            WeaponBoundFeedbackView view = _container.InstantiatePrefab(_weaponBoundFeedbackPrefab)
                .GetComponent<WeaponBoundFeedbackView>();
            view.Initialize(weaponView, playerView);
            return view;
        }

        public AttackFeedbackView CreateAttackFeedback(List<Tile> aimedTiles)
        {
            AttackFeedbackView view = _container.InstantiatePrefab(_attackFeedbackPrefab).GetComponent<AttackFeedbackView>();
            view.Initialize(aimedTiles);
            return view;
        }
    }
}