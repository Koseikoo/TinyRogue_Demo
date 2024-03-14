using UniRx;
using UnityEngine;

namespace Models
{
    public class CameraModel
    {
        public ReactiveCommand AttackShakeCommand = new();
        public ReactiveCommand UnitDeathShakeCommand = new();
        public ReactiveCommand DestroyCommand = new();
        public ReactiveProperty<Vector3> WorldPosition = new();
    }
}