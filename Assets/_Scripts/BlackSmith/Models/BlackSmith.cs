using System;
using System.Collections.Generic;
using UniRx;
using Random = UnityEngine.Random;

namespace Models
{
    public class BlackSmith
    {
        public const int NeededModsToUpgradePower = 5;
        
        public BoolReactiveProperty InTrade = new();

        public void StartTrade()
        {
            InTrade.Value = true;
        }

        public void EndTrade()
        {
            InTrade.Value = false;
        }
    }
}