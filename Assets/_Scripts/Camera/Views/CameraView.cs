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

        player.Tile.Where(tile => tile != null).Subscribe(tile => CameraLerp(tile.WorldPosition)).AddTo(this);
        _cameraModel.AttackShakeCommand.Subscribe(_ => TriggerAttackShake()).AddTo(this);
        _cameraModel.UnitDeathShakeCommand.Subscribe(_ => TriggerUnitDeathShake()).AddTo(this);
        _cameraModel.DestroyCommand.Subscribe(_ => Destroy(gameObject)).AddTo(this);
        player.Weapon.Level.SkipLatestValueOnSubscribe().Subscribe(_ => TriggerUnitDeathShake()).AddTo(this);
    }
    
    private void CameraLerp(Vector3 targetPosition)
    {
        transform.DOMove(targetPosition, .3f);
    }

    private void TriggerAttackShake()
    {
        Vector3 startPosition = GameStateContainer.Player.Tile.Value.WorldPosition;
        Vector3 direction = GameStateContainer.Player.Weapon.AttackDirection.Value * scaleIntensity;
        
        attackTween?.Kill();
        attackTween = transform
            .DOMove(startPosition + direction, scaleDuration)
            .From(startPosition)
            .SetEase(scaleCurve);
    }

    private void TriggerUnitDeathShake()
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
