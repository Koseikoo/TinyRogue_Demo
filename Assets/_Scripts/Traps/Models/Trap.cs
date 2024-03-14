using System.Collections.Generic;
using UniRx;

namespace Models
{
    public class Trap : IAttacker
    {
        private const int TrapMods = 3;
        public List<Slot> ModSlots { get; set; } = new();

        public Trap()
        {
            for (int i = 0; i < TrapMods; i++)
            {
                ModSlots.Add(new());
            }
        }
    }
}