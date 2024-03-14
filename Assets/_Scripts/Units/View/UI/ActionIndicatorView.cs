using System;
using Models;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Views
{
    public class ActionIndicatorView : MonoBehaviour
    {
        public Tile CurrentTile { get; private set; }
        
        
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private UIPositioner positioner;

        public void Initialize()
        {
            GameStateContainer.Player.IsDead.Where(b => b).Subscribe(_ => Destroy(gameObject)).AddTo(this);
        }
        public void Render(Tile tile)
        {
            CurrentTile = tile;
            positioner.SetPosition(tile.WorldPosition);
            int damage = GameStateContainer.Player.Weapon.CalculateDamage(CurrentTile);
            damageText.text = damage.ToString();
            damageText.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if(damageText.gameObject.activeSelf)
                damageText.gameObject.SetActive(false);
        }

        public void ResetView()
        {
            CurrentTile = null;
            gameObject.SetActive(false);
        }
    }
}