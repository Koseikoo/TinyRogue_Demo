using UniRx;

namespace Models
{
    public class Mod : Item
    {
        public IntReactiveProperty Power = new();

        public Mod(ItemType type, int stack, int power) : base(type, stack)
        {
            Power.Value = power;
        }
        public virtual void ApplyToUnit(Unit unit, Unit attacker) {}

        public virtual int GetModDamage()
        {
            return Power.Value;
        }
    }
}