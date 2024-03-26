using System;
using System.Collections.Generic;
using System.Linq;
using Installer;
using UnityEditor;
using UnityEngine;
using Views;

namespace _Testing
{
#if UNITY_EDITOR
    public class SegmentDesignerVisualizationView : MonoBehaviour
    {
        private const string AssetContainerPath = "Assets/Installer/SOs/UnitInstaller.asset";

        private UnitInstaller _assets;
        UnitInstaller Assets
        {
            get
            {
                if (_assets == null)
                    _assets = GetAssets();
                return _assets;
            }
        }

        private List<UnitView> _currentPrefabs = new();

        public void UpdateVisualization(SegmentView view)
        {
            ClearVisualization();
            SpawnVisuals(view.SegmentUnitDefinitions);
        }

        public void ClearVisualization()
        {
            _currentPrefabs.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        private UnitInstaller GetAssets()
        {
            UnitInstaller scriptableObject = AssetDatabase.LoadAssetAtPath<UnitInstaller>(AssetContainerPath);
            return scriptableObject;
        }

        private void DestroyPrevious()
        {
            for (int i = 0; i < _currentPrefabs.Count; i++)
            {
                if(_currentPrefabs[i] != null)
                    DestroyImmediate(_currentPrefabs[i].gameObject);
            }
            
            _currentPrefabs.Clear();
        }

        private void SpawnVisuals(SegmentUnitDefinition[] definitions)
        {
            for (int i = 0; i < definitions.Length; i++)
            {
                int index = i;
                var prefab = GetPrefab(definitions[index].Type);

                if (prefab == null)
                    throw new Exception($"No Prefab Link for Type {definitions[i].Type.ToString()}");

                var instance = PrefabUtility.InstantiatePrefab(prefab, transform) as UnitView;
                instance.transform.position = definitions[i].Point.position;
                _currentPrefabs.Add(instance);
            }
        }

        private UnitView GetPrefab(UnitType type)
        {
            var definition = Assets.Units.FirstOrDefault(v => v.Type == type);
            
            if(definition == null)
                definition = Assets.Enemies.FirstOrDefault(v => v.Type == type);

            if (definition != null)
            {
                
                return definition.Prefab;
            }
            throw new Exception($"No Prefab Link for Type {type.ToString()}");
        }
    }
    #endif
}