namespace Models
{
    public enum StatusEffectType
    {
        Poison,
        Bleeding
    }
    
    public abstract class StatusEffect
    {
        protected Unit _target;
        protected Unit _caster;
        
        public StatusEffect(Unit target, Unit caster)
        {
            _target = target;
            _caster = caster;
            
            SubscribeToUpdateCondition();
        }

        public abstract void ResetEffect();
        protected abstract void SubscribeToUpdateCondition();
            //Poison: Every New Turn | Bleeding: On Second Hit
            
        public abstract void ProgressLogic();
            // Poison: Damage Unit | Bleeding: Add to Bleeding Buildup

        public abstract void EndLogic();
            // Poison: Nothing | Bleeding: Damage Unit 
            // Clear Subscription & Remove from Unit
    }
}