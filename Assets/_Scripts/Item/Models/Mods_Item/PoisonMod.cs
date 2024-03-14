using System.Linq;

namespace Models
{
    public class PoisonMod : Mod
    {
        public PoisonMod(ItemType type, int stack, int power) : base(type, stack, power) {}
        
        public override void ApplyToUnit(Unit unit, Unit attacker)
        {
            var poisonEffect = unit.ActiveStatusEffects.FirstOrDefault(effect => effect is PoisonEffect);
            if (poisonEffect == null)
            {
                PoisonEffect effect = new PoisonEffect(unit, attacker, Power.Value);
                unit.ActiveStatusEffects.Add(effect);
            }
            else
            {
                poisonEffect.ResetEffect();
            }
        }

        public override int GetModDamage()
        {
            return 0;
        }
    }
}