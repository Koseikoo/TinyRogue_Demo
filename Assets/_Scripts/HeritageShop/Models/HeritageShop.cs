using UniRx;

namespace Models
{
    public class HeritageShop
    {
        private bool HasHeritage(int amount) => PersistentPlayerState.Heritage.Value >= amount;
    }
}