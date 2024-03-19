using UniRx;
using UnityEngine;

namespace Models
{
    public class CameraModel
    {
        public ReactiveCommand DestroyCommand = new();
        
        public ReactiveCommand ForwardShakeCommand = new();
        public ReactiveCommand RotationShakeCommand = new();
        public ReactiveCommand SideShakeCommand = new();
        public ReactiveProperty<Vector3> WorldPosition = new();
    }
}