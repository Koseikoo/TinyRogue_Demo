using Factories;
using TinyRogue;
using UnityEngine;
using Zenject;

namespace Installer
{
    [CreateAssetMenu(menuName = "Installer/FeedbackInstaller", fileName = "FeedbackInstaller")]
    public class FeedbackInstaller : ScriptableObjectInstaller<FeedbackInstaller>
    {
        [SerializeField] private WeaponBoundFeedbackView _weaponBoundFeedbackPrefab;
        [SerializeField] private AttackFeedbackView _attackFeedbackPrefab;
        public override void InstallBindings()
        {
            Container.Bind<WeaponBoundFeedbackView>().FromInstance(_weaponBoundFeedbackPrefab).AsSingle();
            Container.Bind<AttackFeedbackView>().FromInstance(_attackFeedbackPrefab).AsSingle();
            Container.Bind<FeedbackFactory>().AsSingle();
        }
    }
}