using Installer;
using Models;
using TMPro;
using UnityEngine;
using Zenject;

namespace Views
{
    public class EnemyInfoModalView : MonoBehaviour
    {
        private GameUnit _gameUnit;

        [SerializeField] private TextMeshProUGUI enemyName;
        [SerializeField] private SlotUIRenderer[] modRenderer;
        
        [Inject] private ItemIconContainer _itemIconContainer;

        public GameUnit GameUnit => _gameUnit;
        
        public void Initialize(GameUnit gameUnit)
        {
            _gameUnit = gameUnit;
            RenderPortrait();
            
            DisableSlots();
            if(gameUnit is Enemy enemy)
            {
                RenderMods(enemy);
            }
        }

        public void DestroyModal()
        {
            Destroy(gameObject);
        }

        private void RenderPortrait()
        {
            enemyName.text = _gameUnit.Type.ToString();
            // TODO Get Portrait
        }

        private void DisableSlots()
        {
            foreach (SlotUIRenderer renderer in modRenderer)
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