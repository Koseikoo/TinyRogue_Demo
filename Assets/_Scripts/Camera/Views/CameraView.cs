using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Models;
using UniRx;
using UnityEngine;
using Zenject;

public class CameraView : MonoBehaviour
{
    [Header("Spring Shake")]
    [SerializeField] private float scaleDuration;
    [SerializeField] private float scaleIntensity;
    [SerializeField] private AnimationCurve scaleCurve;
    
    [Header("Rotation Shake")]
    [SerializeField] private float shakeDuration;
    [SerializeField] private float rotationIntensity;
    [SerializeField] private AnimationCurve rotationCurve;
    
    private Tween attackTween;
    private Tween rotationTween;
    private Transform cameraTransform;

    private Vector3 localStartRotation;

    [Inject] private CameraModel _cameraModel;

    public void Initialize(Player player)
    {
        cameraTransform = transform.GetChild(0);
        localStartRotation = cameraTransform.localRotation.eulerAngles;

        player.Tile.Where(tile => tile != null).Subscribe(tile => CameraLerp(tile.FlatPosition)).AddTo(this);
        _cameraModel.ForwardShakeCommand
            .Subscribe(_ => ShakeAnimation(GameStateContainer.Player.Weapon.AttackDirection.Value * scaleIntensity))
            .AddTo(this);

        _cameraModel.SideShakeCommand
            .Subscribe(_ => ShakeAnimation(cameraTransform.right))
            .AddTo(this);
        
        _cameraModel.RotationShakeCommand.Subscribe(_ => RotationShakeAnimation()).AddTo(this);
        _cameraModel.DestroyCommand.Subscribe(_ => Destroy(gameObject)).AddTo(this);
        player.Weapon.Level.SkipLatestValueOnSubscribe().Subscribe(_ => RotationShakeAnimation()).AddTo(this);
    }
    
    private void CameraLerp(Vector3 targetPosition)
    {
        transform.DOMove(targetPosition, .3f);
    }

    private void ShakeAnimation(Vector3 direction)
    {
        Vector3 startPosition = GameStateContainer.Player.Tile.Value.FlatPosition;
        
        attackTween?.Kill();
        attackTween = transform
            .DOMove(startPosition + (direction * scaleIntensity), scaleDuration)
            .From(startPosition)
            .SetEase(scaleCurve);
    }

    private void RotationShakeAnimation()
    {
        rotationTween?.Kill();
        rotationTween = DOTween.To(() => 0f, t =>
        {
            var tEval = rotationCurve.Evaluate(t);
            cameraTransform.localRotation =
                Quaternion.Euler(new Vector3(localStartRotation.x, 0f, tEval * rotationIntensity));
        }, 1f, shakeDuration);
    }
}
