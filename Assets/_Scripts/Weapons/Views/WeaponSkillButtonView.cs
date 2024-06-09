using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using Models;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponSkillButtonView : MonoBehaviour
{
    private const float DescriptionXOffset = 200;
    private const float HoldDurationInSeconds = .8f;
    
    [SerializeField] private Button button;
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image unlockedBorder;
    [SerializeField] private Image unlockableBorder;
    [SerializeField] private Image lockedBorder;
    [SerializeField] private GameObject UnlockObject;
    [SerializeField] private TextMeshProUGUI UnlockCostText;
    [SerializeField] private RectTransform description;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    private WeaponSkill skill;
    private WeaponData _weapon;
    private bool _descriptionOpened;
    private float _holdDuration;

    private Coroutine holdRoutine;
    
    private bool IsUnlocked => _weapon.UnlockedSkills.Contains(skill);
    private bool IsNextInTree => _weapon.UnlockedSkills.FirstOrDefault(s => s.ConnectedSkills.Contains(skill)) != null;
    private bool CanAffordSkill => GameStateContainer.Player.AvailableUnlockPoints >= skill.UnlockCost;
    public bool IsUnlockable => IsNextInTree && CanAffordSkill;
    public void Initialize(WeaponSkill model, WeaponData weapon)
    {
        skill = model;
        _weapon = weapon;
        skillIcon.sprite = skill.Sprite;
        UpdateBorder();
    }

    public void AddButtonEvent(Action action)
    {
        button.onClick.AddListener(() => action());
        button
            .OnPointerDownAsObservable()
            .Subscribe(OnPointerDown)
            .AddTo(this);
        
        button
            .OnPointerUpAsObservable()
            .Subscribe(OnPointerUp)
            .AddTo(this);
    }

    IEnumerator ShowDescriptionRoutine()
    {
        while (_holdDuration < HoldDurationInSeconds)
        {
            _holdDuration += Time.deltaTime;
            yield return null;
        }
        ShowDescription();
    }

    private void OnPointerDown(PointerEventData data)
    {
        if (!_descriptionOpened)
        {
            if(holdRoutine != null)
            {
                StopCoroutine(holdRoutine);
            }

            holdRoutine = StartCoroutine(ShowDescriptionRoutine());
        }
        Debug.Log("Pointer Down");
    }

    private void OnPointerUp(PointerEventData data)
    {
        if(holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
        }
        
        _holdDuration = 0;
        HideDescription();
    }

    private void ShowDescription()
    {
        _descriptionOpened = true;
        descriptionText.text = skill.Description;
        description.DOAnchorPos(new(DescriptionXOffset, 0f), .3f);
    }

    private void HideDescription()
    {
        description.DOAnchorPos(new(0f, 0f), .3f);
    }

    public void OnButtonPressed()
    {
        if (IsUnlockable && !_descriptionOpened)
        {
            // Unlock Skill
            _weapon.UnlockSkill(skill);
            Debug.Log("Unlock Skill");
        }
        _descriptionOpened = false;
    }

    public void UpdateBorder()
    {
        if (IsUnlocked)
        {
            ShowBorder(unlockedBorder);
            ToggleUnlockCost(false);
        }
        else if (IsNextInTree)
        {
            ShowBorder(unlockableBorder);
            ToggleUnlockCost(true);
        }
        else
        {
            ShowBorder(lockedBorder);
            ToggleUnlockCost(true);
        }
        
    }

    private void ShowBorder(Image image)
    {
        unlockedBorder.enabled = image == unlockedBorder;
        unlockableBorder.enabled = image == unlockableBorder;
        lockedBorder.enabled = image == lockedBorder;
    }

    private void ToggleUnlockCost(bool show)
    {
        UnlockObject.SetActive(show);
        if (show)
        {
            UnlockCostText.text = skill.UnlockCost.ToString();
            UnlockCostText.color = CanAffordSkill
                ? Color.black
                : Color.red;
        }
    }
    
}