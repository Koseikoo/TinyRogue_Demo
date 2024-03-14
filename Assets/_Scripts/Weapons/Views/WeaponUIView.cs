using Models;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace Views
{
    public class WeaponUIView : MonoBehaviour
    {
        [SerializeField] private GameObject modUI;
        [SerializeField] private SlotUIView[] weaponModSlots;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI nextLevelText;
        [SerializeField] private TextMeshProUGUI levelProgressText;
        [SerializeField] private ProceduralImage progressImage;
        [SerializeField] private Image TEMP_inWeaponAttackFilter;
        
        private Weapon _weapon;

        private Vector2 _progressRectSize;

        public void Initialize(Weapon weapon)
        {
            _weapon = weapon;
            _progressRectSize = progressImage.rectTransform.sizeDelta;

            for (int i = 0; i < _weapon.ModSlots.Count; i++)
            {
                weaponModSlots[i].Initialize(_weapon.ModSlots[i]);
            }
            
            GameStateContainer.CloseOpenUIElements.Subscribe(_ => HideWeaponUI()).AddTo(this);
            _weapon.Xp.Subscribe(_ => RenderWeaponInfo()).AddTo(this);

            _weapon.Owner.InAttackMode.Subscribe(inAttackMode => TEMP_inWeaponAttackFilter.enabled = inAttackMode)
                .AddTo(this);
        }

        public void ShowWeaponUI()
        {
            if (modUI.activeSelf)
            {
                HideWeaponUI();
                return;
            }
            
            modUI.SetActive(true);
            GameStateContainer.OpenUIElements.Add(gameObject);
        }

        public void HideWeaponUI()
        {
            modUI.SetActive(false);
            GameStateContainer.OpenUIElements.Remove(gameObject);
        }

        private void RenderWeaponInfo()
        {
            int damage = (_weapon.ModSlots[0].Item.Value as Mod).Power.Value;
            damageText.text = $"DMG: {damage}";
            levelText.text = _weapon.Level.ToString();
            nextLevelText.text = (_weapon.Level.Value + 1).ToString();
            levelProgressText.text = $"{_weapon.Xp} / {WeaponHelper.GetLevelXp(_weapon.Level.Value + 1)}";
            progressImage.rectTransform.sizeDelta = new Vector2(_weapon.Progress() * _progressRectSize.x, _progressRectSize.y);
                
        }
    }
}