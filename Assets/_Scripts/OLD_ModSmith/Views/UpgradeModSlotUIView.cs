using System;
using System.Collections.Generic;
using Installer;
using Models;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Views
{
    public class UpgradeModSlotUIView : MonoBehaviour
    {
        [SerializeField] private SlotUIRenderer weaponModRenderer;
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeText;
        
        private UITransformPositioner _transformPositioner; 
        private BlackSmith _blackSmith;
        private int _slotIndex;

        public Vector3 WorldPosition => _transformPositioner.WorldPosition;

        private Mod WeaponMod => GameStateContainer.Player.Weapon.ModSlots.GetMods()[_slotIndex];
        
        private void Awake()
        {
            _transformPositioner = GetComponent<UITransformPositioner>();
            _slotIndex = transform.GetSiblingIndex() + 1;
        }

        public void Initialize(BlackSmith backSmith)
        {
            _blackSmith = backSmith;
        }

        private void Update()
        {
            var item = GameStateContainer.Player.Weapon.ModSlots[_slotIndex].Item.Value;
            if(item == null)
                RenderEmpty();
            else
                Render(item as Mod);
        }

        public void TapSlot()
        {
            GameStateContainer.Player.Weapon.ModSlots[_slotIndex].TapSlot();
        }

        public void UpgradeMod()
        {
            List<Mod> matches = GetModsToUpgrade();

            if (matches.Count >= BlackSmith.NeededModsToUpgradePower)
            {
                for (int i = 0; i < BlackSmith.NeededModsToUpgradePower; i++)
                {
                    GameStateContainer.Player.Bag.RemoveItem(matches[i].Type);
                }

                WeaponMod.Power.Value++;
                Render(WeaponMod);
            }
        }
        
        public void Render(Mod mod)
        {
            string coloredString = GetColoredString(GetModsToUpgrade().Count);
            
            weaponModRenderer.RenderMod(mod.Type, mod.Power.Value);
            upgradeText.text = $"{coloredString} / {BlackSmith.NeededModsToUpgradePower}";
            upgradeButton.SetActive(true);
        }

        public void RenderEmpty()
        {
            weaponModRenderer.DisableSlot();
            upgradeButton.SetActive(false);
        }

        private List<Mod> GetModsToUpgrade()
        {
            return GameStateContainer.Player.Bag.Mods.MatchingMods(mod =>
                mod.Type == WeaponMod.Type &&
                mod.Power.Value == WeaponMod.Power.Value);
        }

        private string GetColoredString(int matches)
        {
            if(matches >= BlackSmith.NeededModsToUpgradePower)
                return $"<color=#94f549>{matches}</color>";
            return $"<color=#f55b49>{matches}</color>";
        }
    }
}