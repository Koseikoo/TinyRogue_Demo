using System;
using System.Collections.Generic;
using Container;
using Factories;
using Models;
using UnityEngine;
using UnityEngine.Serialization;
using Views;
using Zenject;

namespace Installer
{
    [CreateAssetMenu(fileName = "CameraInstaller", menuName = "Installer/CameraInstaller")]
    public class CameraInstaller : ScriptableObjectInstaller<IslandInstaller>
    {
        [SerializeField] private CameraView _cameraViewPrefab;
        [SerializeField] private CameraRotationUIView _cameraRotationUIPrefab;
        public override void InstallBindings()
        {
            Container.Bind<CameraModel>().AsSingle();
            Container.Bind<CameraView>().FromInstance(_cameraViewPrefab).AsSingle();
            Container.Bind<CameraRotationUIView>().FromInstance(_cameraRotationUIPrefab).AsSingle();

            Container.Bind<CameraFactory>().AsSingle();
        }

    }
}