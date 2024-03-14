using Models;

namespace Modals
{
    public class IslandInfo
    {
        private const int UnlockCostMult = 10;
        
        public int Level;
        public bool Unlocked;
        public int UnlockCost => Level * UnlockCostMult;

        public IslandInfo(int level)
        {
            Level = level;
        }
    }
}