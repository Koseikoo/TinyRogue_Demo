using System;
using UniRx;
using UnityEngine;

namespace Testing
{
    public class SwordViewTest : MonoBehaviour
    {
        public ReactiveProperty<Vector3> TouchPosition;
        public ReactiveProperty<Vector3> AttackPosition;
        public BoolReactiveProperty StartedAim;

        private void Update()
        {
            if (InputHelper.TouchStarted())
            {
                StartedAim.Value = true;
            }
            
            if (InputHelper.IsTouching())
            {
                TouchPosition.Value = UIHelper.Camera.GetExtendedPositionFromCamera(InputHelper.GetTouchPosition());
            }

            if (InputHelper.TouchEnded())
            {
                AttackPosition.Value = UIHelper.Camera.GetExtendedPositionFromCamera(InputHelper.GetTouchPosition());
                StartedAim.Value = false;
            }
        }
    }
}