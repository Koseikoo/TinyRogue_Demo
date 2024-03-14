using UniRx;

namespace Models
{
    public class HealthCell
    {
        public bool Regenerative;
        public int MaxHealth;
        public IntReactiveProperty Health = new();

        public HealthCell(int maxHealth)
        {
            MaxHealth = maxHealth;
            Health.Value = MaxHealth;
        }

        public int Damage(int damage)
        {
            int rest = damage - Health.Value;
            if (rest > 0)
            {
                Health.Value = 0;
                return rest;
            }
            Health.Value -= damage;
            return 0;
        }

        public void Regenerate()
        {
            Health.Value = MaxHealth;
        }
    }
}