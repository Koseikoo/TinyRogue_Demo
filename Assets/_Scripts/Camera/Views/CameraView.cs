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
    [SerializeField] private AnimationCurve lookCurve;
    [SerializeField] private float lookDuration;
    [SerializeField] private float followDuration;
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

    private Camera _camera;

    public void Initialize(Player player)
    {
        _camera = Camera.main;
        
        cameraTransform = transform.GetChild(0);
        localStartRotation = cameraTransform.localRotation.eulerAngles;

        player.EnterIsland.Subscribe(_ => _camera.nearClipPlane = 10);
        player.EnterShip.Subscribe(_ => _camera.nearClipPlane = .1f);

        player.Tile.Where(tile => tile != null).Subscribe(tile => CameraLerp(tile.FlatPosition)).AddTo(this);
        _cameraModel.ForwardShakeCommand
            .Subscribe(_ => ShakeAnimation(GameStateContainer.Player.LookDirection.Value * scaleIntensity))
            .AddTo(this);

        _cameraModel.SideShakeCommand
            .Subscribe(_ => ShakeAnimation(cameraTransform.right))
            .AddTo(this);

        _cameraModel.LookCommand
            .Subscribe(LookLerp)
            .AddTo(this);
        
        _cameraModel.RotationShakeCommand.Subscribe(_ => RotationShakeAnimation()).AddTo(this);
        _cameraModel.DestroyCommand.Subscribe(_ => Destroy(gameObject)).AddTo(this);
    }
    
    private void CameraLerp(Vector3 targetPosition)
    {
        transform.DOMove(targetPosition, followDuration);
    }

    private void LookLerp(Vector3 direction)
    {
        Vector3 startForward = transform.forward;
        Quaternion startAngle = transform.rotation;
        float yAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        Quaternion endAngle = Quaternion.Euler(new Vector3(0f, yAngle, 0f));
        GameStateContainer.Player.AnchorYRotation.Value = yAngle;

        DOTween.To(() => 0f, t =>
        {
            float evalT = lookCurve.Evaluate(t);
            transform.rotation = Quaternion.Lerp(startAngle, endAngle, evalT);
            
        }, 1f, lookDuration);
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
            float tEval = rotationCurve.Evaluate(t);
            cameraTransform.localRotation =
                Quaternion.Euler(new Vector3(localStartRotation.x, 0f, tEval * rotationIntensity));
        }, 1f, shakeDuration);
    }
}
