using Container;
using Installer;
using UnityEngine;
using Zenject;

namespace _Scripts.Recipe.Installer
{
    [CreateAssetMenu(fileName = "RecipeInstaller", menuName = "Installer/RecipeInstaller")]
    public class RecipeInstaller : ScriptableObjectInstaller<UnitInstaller>
    {
        [SerializeField] private UnitRecipeDrop[] _unitRecipeDropDefinitions;
        public override void InstallBindings()
        {
            Container.Bind<UnitRecipeDropContainer>().FromInstance(new(_unitRecipeDropDefinitions)).AsSingle();
        }
    }
}