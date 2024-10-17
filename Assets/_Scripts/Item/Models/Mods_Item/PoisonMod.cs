using System.Linq;

namespace Models
{
    public class PoisonMod : Mod
    {
        public PoisonMod(ItemType type, int stack, int power) : base(type, stack, power) {}
        
        public override void ApplyToUnit(GameUnit gameUnit, GameUnit attacker)
        {
            StatusEffect poisonEffect = gameUnit.ActiveStatusEffects.FirstOrDefault(effect => effect is PoisonEffect);
            if (poisonEffect == null)
            {
                PoisonEffect effect = new PoisonEffect(gameUnit, attacker, Power.Value);
                gameUnit.ActiveStatusEffects.Add(effect);
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