using Game;
using Models;
using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller>
{
    public override void InstallBindings()
    {
        Container.Bind<GameSetup>().AsSingle();
        Container.Bind<GameAreaManager>().AsSingle();
        Container.Bind<PlayerFeedbackManager>().AsSingle();
        Container.Bind<TurnManager>().AsSingle();
        Container.Bind<IntuitiveInputManager>().AsSingle();
        Container.Bind<MoveInputTracker>().AsSingle();
        Container.Bind<PlayerManager>().AsSingle();
    }
}