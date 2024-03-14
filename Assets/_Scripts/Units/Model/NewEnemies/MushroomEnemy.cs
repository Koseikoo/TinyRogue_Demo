namespace Models
{
    public class MushroomEnemy : Enemy
    {
        protected override void EnemyAction()
        {
            var path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value, 
                IsAttackPathTile);
            
            if (path.Count <= AttackRange)
            {
                this.TryAttackTarget(path);
            }
        }
    }
}