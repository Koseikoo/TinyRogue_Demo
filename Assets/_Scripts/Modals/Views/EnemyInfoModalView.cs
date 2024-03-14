using Installer;
using Models;
using TMPro;
using UnityEngine;
using Zenject;

namespace Views
{
    public class EnemyInfoModalView : MonoBehaviour
    {
        private Unit _unit;

        [SerializeField] private TextMeshProUGUI enemyName;
        [SerializeField] private SlotUIRenderer[] modRenderer;
        
        [Inject] private ItemIconContainer _itemIconContainer;

        public Unit Unit => _unit;
        
        public void Initialize(Unit unit)
        {
            _unit = unit;
            RenderPortrait();
            
            DisableSlots();
            if(unit is Enemy enemy)
                RenderMods(enemy);
        }

        public void DestroyModal()
        {
            Destroy(gameObject);
        }

        private void RenderPortrait()
        {
            enemyName.text = _unit.Type.ToString();
            // TODO Get Portrait
        }

        private void DisableSlots()
        {
            foreach (var renderer in modRenderer)
            {
                renderer.DisableSlot();
            }
        }

        private void RenderMods(Enemy enemy)
        {
            for (int i = 0; i < modRenderer.Length; i++)
            {
                if (enemy.ModSlots[i].IsOccupied)
                {
                    Mod mod = enemy.ModSlots[i].Item.Value as Mod;
                    modRenderer[i].RenderMod(mod.Type, mod.Power.Value);
                }
                else
                {
                    modRenderer[i].RenderEmpty();
                }
            }
        }
    }
}