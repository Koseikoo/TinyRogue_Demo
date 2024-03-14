using System;
using Models;
using UnityEngine;

namespace Views
{
    using UniRx;
    public class WeaponChargesUIView : MonoBehaviour
    {
        [SerializeField] private ChargeUIView[] _attackChargeViews;
        [SerializeField] private ChargeUIView[] _moveChargeViews;

        private Weapon _weapon;
        private IDisposable _attackChargeSubscription;
        private IDisposable _moveChargeSubscription;

        public void Initialize(Weapon weapon)
        {
            _weapon = weapon;
            ResetSubscriptions();
            _attackChargeSubscription = weapon.AttackCharges.Subscribe(UpdateAttackCharges);
            _moveChargeSubscription = weapon.MoveCharges.Subscribe(UpdateMoveCharges);
        }

        private void ResetSubscriptions()
        {
            _attackChargeSubscription?.Dispose();
            _moveChargeSubscription?.Dispose();
        }

        private void UpdateAttackCharges(int charges)
        {
            ResetChargeViews(_attackChargeViews);
            ShowAttackChargeViews();
        }
        
        private void UpdateMoveCharges(int charges)
        {
            ResetChargeViews(_moveChargeViews);
            ShowMoveChargeViews();
        }

        private void ResetChargeViews(ChargeUIView[] chargeUIViews)
        {
            foreach (ChargeUIView view in chargeUIViews)
            {
                view.gameObject.SetActive(false);
            }
        }

        private void ShowAttackChargeViews()
        {
            for (int i = 0; i < _weapon.MaxAttackCharges; i++)
            {
                if (_attackChargeViews.Length <= i)
                    throw new Exception("Weapon Charges exceed _chargeViews!");
                
                _attackChargeViews[i].ShowView(_weapon.AttackCharges.Value > i);
            }
        }
        
        private void ShowMoveChargeViews()
        {
            for (int i = 0; i < _weapon.MaxMoveCharges; i++)
            {
                if (_moveChargeViews.Length <= i)
                    throw new Exception("Weapon Charges exceed _chargeViews!");
                
                _moveChargeViews[i].ShowView(_weapon.MoveCharges.Value > i);
            }
        }
    }
}