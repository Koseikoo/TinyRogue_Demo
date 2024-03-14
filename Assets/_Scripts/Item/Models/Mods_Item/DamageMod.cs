namespace Models
{
    public class DamageMod : Mod
    {
        public DamageMod(ItemType type, int stack, int power) : base(type, stack, power) {}
        
        public override void ApplyToUnit(Unit unit, Unit attacker)
        {
            unit.Damage(Power.Value, attacker);
        }
    }
}