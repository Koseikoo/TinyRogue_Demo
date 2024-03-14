using Installer;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Views
{
    public class LootView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        
        [Inject] private LootMaterialContainer _materialContainer;
        public void Initialize(LootType type)
        {
            meshRenderer.material = _materialContainer.GetMaterial(type);
            gameObject.SetActive(true);
        }

        public void ResetView()
        {
            gameObject.SetActive(false);
        }
    }
}