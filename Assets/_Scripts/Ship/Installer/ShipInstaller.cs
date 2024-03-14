using Factories;
using UnityEngine;
using Views;
using Zenject;

namespace Installer
{
    [CreateAssetMenu(fileName = "ShipInstaller", menuName = "Installer/ShipInstaller")]
    public class ShipInstaller : ScriptableObjectInstaller<ShipInstaller>
    {
        [SerializeField] private ShipView shipPrefab;
        public override void InstallBindings()
        {
            Container.Bind<ShipFactory>().AsSingle();
            Container.Bind<ShipView>().FromInstance(shipPrefab).AsSingle();
        }
    }
}