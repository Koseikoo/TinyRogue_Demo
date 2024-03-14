using Factories;
using UnityEngine;
using Zenject;

namespace Installer
{
    [CreateAssetMenu(menuName = "Installer/FeedbackInstaller", fileName = "FeedbackInstaller")]
    public class FeedbackInstaller : ScriptableObjectInstaller<FeedbackInstaller>
    {
        [SerializeField] private WeaponBoundFeedbackView _weaponBoundFeedbackPrefab;
        public override void InstallBindings()
        {
            Container.Bind<WeaponBoundFeedbackView>().FromInstance(_weaponBoundFeedbackPrefab).AsSingle();
            Container.Bind<FeedbackFactory>().AsSingle();
        }
    }
}