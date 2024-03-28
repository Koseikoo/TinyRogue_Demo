using UniRx;
using UnityEngine;
using DG.Tweening;
using Models;

namespace Views
{
    public class WeaponView : MonoBehaviour
    {
        [SerializeField] private Vector3 offset;
    
        private Weapon _weapon;

        public void Initialize(Weapon weapon)
        {
            _weapon = weapon;
        }

        private void UpdatePosition(Tile tile)
        {
            transform.DOMove(tile.FlatPosition + offset, Weapon.AttackAnimationDuration)
                .SetEase(Ease.Linear);
        }
    }
}