namespace Models
{
    public class GhostEnemy : Enemy
    {
        protected override void EnemyAction()
        {
            var path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value, 
                IsAttackPathTile);
            
            if(path == null)
                return;

            if (path.Count <= AttackRange)
            {
                this.TryAttackTarget(path);
                AnimationCommand.Execute(AnimationState.Attack1);
                Damage(Health.Value);
            }
            else
            {
                this.FollowTarget(path[0]);
            }
        }
    }
}