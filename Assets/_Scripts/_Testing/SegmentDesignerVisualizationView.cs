using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Views;

namespace _Testing
{
    [System.Serializable]
    public class SegmentDesignerVisualLink
    {
        public GameObject Prefab;
        public UnitType Type;
    }
    
    [ExecuteInEditMode]
    public class SegmentDesignerVisualizationView : MonoBehaviour
    {
        [SerializeField] private SegmentDesignerVisualLink[] Destructibles;
        [SerializeField] private SegmentDesignerVisualLink[] Enemies;

        private List<GameObject> _currentPrefabs;

        public void UpdateVisualization(SegmentView view)
        {
            DestroyPrevious();
            SpawnVisuals(view.SegmentUnitDefinitions);
        }

        public void ClearVisualization()
        {
            DestroyPrevious();
        }

        private void DestroyPrevious()
        {
            for (int i = 0; i < _currentPrefabs.Count; i++)
            {
                DestroyImmediate(_currentPrefabs[i]);
            }
            
            _currentPrefabs.Clear();
        }

        private void SpawnVisuals(SegmentUnitDefinition[] definitions)
        {
            for (int i = 0; i < definitions.Length; i++)
            {
                int index = i;
                var link = GetLink(definitions[index].Type);

                if (link == null)
                    throw new Exception($"No Prefab Link for Type {definitions[i].Type.ToString()}");

                GameObject instance = PrefabUtility.InstantiatePrefab(link.Prefab, transform) as GameObject;
                instance.transform.position = definitions[i].Point.position;
                _currentPrefabs.Add(instance);
            }
        }

        private SegmentDesignerVisualLink GetLink(UnitType type)
        {
            var link = Destructibles.FirstOrDefault(v => v.Type == type);
            
            if(link == null)
                link = Enemies.FirstOrDefault(v => v.Type == type);

            if (link != null)
                return link;
            throw new Exception($"No Prefab Link for Type {type.ToString()}");
        }
    }
}