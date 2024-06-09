using System.Collections;
using System.Security.Cryptography;
using Models;
using TinyRogue;
using TMPro;
using UnityEngine;

namespace Views
{
    public class WeaponDataModalView : MonoBehaviour, IView<WeaponData>
    {
        [SerializeField]
        private TextMeshProUGUI weaponNameText;
        [SerializeField]
        private TextMeshProUGUI weaponDamage;

        private Tile _tile;
        private Coroutine _positionRoutine;
        
        public void Initialize(WeaponData model)
        {
            gameObject.SetActive(false);
            weaponNameText.text = model.Name.ToString().AddSpace();
            weaponDamage.text = model.Damage.ToString();
        }

        public void SetPosition(Tile tile)
        {
            _tile = tile;
            gameObject.SetActive(true);
            _positionRoutine = StartCoroutine(UpdatePosition());
            
            tile.AddSingleExecutionLogic(unit =>
            {
                if (unit is Player)
                {
                    Destroy();
                }
            });
        }

        private void Destroy()
        {
            Destroy(gameObject);
        }

        private IEnumerator UpdatePosition()
        {
            Camera camera = Camera.main;
            while (true)
            {
                if (camera != null)
                {
                    Vector3 screenPosition = camera.WorldToScreenPoint(_tile.WorldPosition);
                    transform.position = screenPosition;
                }
                yield return null;
                
            }
        }
        
    }
}