using System;
using Models;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class ActionIndicatorView : MonoBehaviour
    {
        public Tile CurrentTile { get; private set; }
        
        
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private UIPositioner positioner;
        [SerializeField] private Image bounceBackImage;

        [SerializeField] private Color killColor;
        [SerializeField] private Color bounceOffColor;
        [SerializeField] private Color invincibleColor;
        
        public void Initialize()
        {
            GameStateContainer.Player.IsDead.Where(b => b).Subscribe(_ => Destroy(gameObject)).AddTo(this);
        }
        public void Render(Tile tile)
        {
            CurrentTile = tile;
            positioner.SetPosition(tile.FlatPosition);
            int damage = GameStateContainer.Player.Weapon.Value.Damage;
            bool unitIsInvincible = tile.HasUnit && tile.Unit.Value.IsInvincible.Value;
            
            damageText.color = GetTextColor(tile);
            damageText.text = unitIsInvincible ? "0" : damage.ToString();

            bounceBackImage.enabled = tile.AttackBouncesFromTile;
            damageText.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (damageText.gameObject.activeSelf)
            {
                damageText.gameObject.SetActive(false);
                bounceBackImage.enabled = false;
            }
        }

        public void ResetView()
        {
            CurrentTile = null;
            gameObject.SetActive(false);
        }

        private Color GetTextColor(Tile tile)
        {
            GameUnit gameUnit = tile.Unit.Value;
            if (gameUnit == null)
            {
                return default;
            }

            if (gameUnit.IsInvincible.Value)
            {
                return invincibleColor;
            }

            if (tile.AttackBouncesFromTile)
            {
                return bounceOffColor;
            }

            return killColor;
        }
    }
}