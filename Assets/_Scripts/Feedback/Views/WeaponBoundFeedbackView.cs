using System;
using Factories;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Views;

public class WeaponBoundFeedbackView : MonoBehaviour
{
    [SerializeField] private float AngleOffset = 90;
    
    [SerializeField] private AnimationCurve baseCurve;
    [SerializeField] private AnimationCurve maxCurve;
    [SerializeField] private AnimationCurve snapCurve;
    [SerializeField] LineRenderer _playerLine;
    [SerializeField] LineRenderer _weaponLine;
    [SerializeField] private Image border;
    [SerializeField] private UITransformPositioner borderPositioner;

    [SerializeField] private Color stableColor; 
    [SerializeField] private Color unstableColor;

    [SerializeField] private int LinePoints = 5;
    [SerializeField] [Range(0, 1)] private float curveLerp;
    [SerializeField] private float lineWidth;
    [SerializeField] private float minLineWidth;
    [SerializeField] private float positionOffset;

    private Transform _weaponView;
    private Transform _playerView;
    private Vector3 _nextWeaponPosition;

    private bool Snapped()
    {
        float currentMagnitude = (_playerView.position - _nextWeaponPosition).magnitude;
        bool weaponOutsideRange = currentMagnitude > GameStateContainer.Player.Weapon.Range * Island.TileDistance;
        return weaponOutsideRange;
    }

    public void Initialize(Transform weaponView, Transform playerView)
    {
        _weaponView = weaponView;
        _playerView = playerView;
        borderPositioner.Initialize(playerView);
    }

    private void Update()
    {
        if(_playerView == null || _weaponView == null)
            return;

        bool previewWeaponBond = InputHelper.IsTouching() && !InputHelper.StartedOverUI &&
                                 !GameStateContainer.Player.Weapon.FixToHolster;
                                 
        if(previewWeaponBond)
            _nextWeaponPosition = UIHelper.Camera.GetExtendedPositionFromCamera(InputHelper.GetTouchPosition());
        else
            _nextWeaponPosition = _weaponView.position;
        
        Vector3[] playerPoints = new Vector3[LinePoints];
        Vector3[] weaponPoints = new Vector3[LinePoints];

        Vector3 weaponPosition = _nextWeaponPosition + (_weaponView.up * .5f);
        Vector3 playerPosition = _playerView.position;
        playerPosition.y = weaponPosition.y;
        Vector3 direction = (weaponPosition - playerPosition).normalized;

        Vector3 offsetWeaponPosition = weaponPosition - (positionOffset * direction);
        Vector3 offsetPlayerPosition = playerPosition + (positionOffset * direction);
        Vector3 midPosition = Vector3.Lerp(offsetPlayerPosition, offsetWeaponPosition, .5f);

        curveLerp = Mathf.InverseLerp(0f, GameStateContainer.Player.Weapon.Range * Island.TileDistance,
            (weaponPosition - playerPosition).magnitude);


        for (int i = 0; i < LinePoints; i++)
        {
            weaponPoints[i] = Vector3.Lerp(offsetWeaponPosition, Snapped() ? offsetWeaponPosition : midPosition, (float)i / (LinePoints - 1));
            playerPoints[i] = Vector3.Lerp(offsetPlayerPosition, Snapped() ? offsetPlayerPosition : midPosition, (float)i / (LinePoints - 1));
        }

        _playerLine.positionCount = LinePoints;
        _playerLine.SetPositions(playerPoints);
        _playerLine.widthCurve = GetLerpedCurve();
        _playerLine.widthMultiplier = lineWidth;
        _playerLine.colorGradient = GetGradient();
        
        _weaponLine.positionCount = LinePoints;
        _weaponLine.SetPositions(weaponPoints);
        _weaponLine.widthCurve = GetLerpedCurve();
        _weaponLine.widthMultiplier = lineWidth;
        _weaponLine.colorGradient = GetGradient();
    }

    private void SetBorderAngle()
    {
        Vector3 direction = _weaponView.position - _playerView.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        float angle = rotation.eulerAngles.y;
        border.material.SetFloat("_Angle", -angle + AngleOffset + GameStateContainer.Player.AnchorYRotation.Value);
        float stepLength = Mathf.Max(0, Mathf.Lerp(0, .2f, curveLerp) - .1f);
        border.material.SetFloat("_StepLength", stepLength);
    }

    private AnimationCurve GetLerpedCurve()
    {
        int keyCount = Mathf.Min(baseCurve.length, maxCurve.length);
        AnimationCurve lerpedCurve = new AnimationCurve();

        for (int i = 0; i < keyCount; i++)
        {
            // Get the key from each curve
            Keyframe keyA = baseCurve[i];
            Keyframe keyB = Snapped() ? snapCurve[i] : maxCurve[i];

            // Lerp between the values of the keys
            float inTangent = Mathf.Lerp(keyA.inTangent, keyB.inTangent, curveLerp);
            float outTangent = Mathf.Lerp(keyA.outTangent, keyB.outTangent, curveLerp);
            float inWeight = Mathf.Lerp(keyA.inWeight, keyB.inWeight, curveLerp);
            float outWeight = Mathf.Lerp(keyA.outWeight, keyB.outWeight, curveLerp);
            float time = Mathf.Lerp(keyA.time, keyB.time, curveLerp);
            float value = Mathf.Lerp(keyA.value, keyB.value, curveLerp);

            // Add the lerped key to the new curve
            Keyframe lerpedKey = new Keyframe(time, value);
            lerpedKey.inTangent = inTangent;
            lerpedKey.outTangent = outTangent;
            lerpedKey.inWeight = inWeight;
            lerpedKey.outWeight = outWeight;
            
            lerpedCurve.AddKey(lerpedKey);
        }

        return lerpedCurve;
    }

    private Gradient GetGradient()
    {
        Gradient gradient = _playerLine.colorGradient;
        Color color = Color.Lerp(stableColor, unstableColor, curveLerp);
        float aMult = GetAlphaMult();
        
        gradient.SetKeys(new []
        {
            new GradientColorKey(color, 0f),
            new GradientColorKey(color, 1f)
            
        }, new []
        {
            new GradientAlphaKey(aMult * 1, 0f),
            new GradientAlphaKey(aMult * 1, 1f),
            //new GradientAlphaKey(aMult * (1-curveLerp), 1f),
        });
        return gradient;
    }

    private float GetAlphaMult()
    {
        float distance = (_playerView.position - _nextWeaponPosition).magnitude; 
        if (distance < minLineWidth)
        {
            return 0;
        }
        
        float t = Mathf.InverseLerp(minLineWidth, minLineWidth + .4f, distance);
        return t;
    }
}

