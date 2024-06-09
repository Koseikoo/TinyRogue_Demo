using System;
using System.Collections.Generic;
using DG.Tweening;
using Models;
using UnityEngine;

namespace TinyRogue
{
    public class WeaponView : MonoBehaviour, IView<WeaponData>
    {
        [SerializeField] private GameObject baseSword;
        [SerializeField] private GameObject baseBow;
        [SerializeField] private GameObject baseHammer;
        [SerializeField] private GameObject baseShield;
        [SerializeField] private AnimationCurve dropArc;
        [SerializeField] private float arcMaxHeight;
        [SerializeField] private float dropDuration;
        [SerializeField] float rotationSpeed = .2f;


        private Dictionary<WeaponName, GameObject> _visualDict;

        private WeaponData _model;
        public void Initialize(WeaponData model)
        {
            _model = model;
            SetupDictionary();
            UpdateVisual();
        }

        private void Update()
        {
            transform.Rotate(Vector3.up * rotationSpeed);
        }

        private void SetupDictionary()
        {
            _visualDict = new()
            {
                [WeaponName.BaseSword] = baseSword,
                [WeaponName.BaseBow] = baseBow,
                [WeaponName.BaseHammer] = baseHammer,
                [WeaponName.BaseShield] = baseShield
            };
        }

        private void UpdateVisual()
        {
            foreach (KeyValuePair<WeaponName,GameObject> kvp in _visualDict)
            {
                kvp.Value.SetActive(false);
            }
            
            if(_visualDict.TryGetValue(_model.Name, out GameObject visual))
            {
                visual.SetActive(true);
            }
        }
        
        public Tile DropWeapon(Tile dropTile)
        {
            Tile endTile = dropTile.GetFirstEmptyTile();
            Vector3 startPosition = dropTile.FlatPosition;
            Vector3 endPosition = endTile.FlatPosition;
            DOTween.To(() => 0, t =>
            {
                Vector3 vector = Vector3.Lerp(startPosition, endPosition, t);
                transform.position = new Vector3(vector.x, arcMaxHeight * dropArc.Evaluate(t), vector.z);
            }, 1, dropDuration);
            
            endTile.AddSingleExecutionLogic(Pickup);

            return endTile;
        }

        private void Pickup(Unit unit)
        {
            if (unit is Player)
            {
                GameStateContainer.Player.Weapon.Value = _model;
                Destroy(gameObject);
            }
        }
    }
}