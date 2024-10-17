namespace Models
{
    public class DamageMod : Mod
    {
        public DamageMod(ItemType type, int stack, int power) : base(type, stack, power) {}
        
        public override void ApplyToUnit(GameUnit gameUnit, GameUnit attacker)
        {
            gameUnit.Damage(Power.Value, attacker);
        }
    }
}