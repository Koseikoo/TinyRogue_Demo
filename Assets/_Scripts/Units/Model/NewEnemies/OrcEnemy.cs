namespace Models
{
    public class OrcEnemy : Enemy
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
                this.TryAttackTarget(path, true);
                AnimationCommand.Execute(AnimationState.Attack1);
            }
            else
            {
                this.FollowTarget(path[0]);
            }
        }

        protected override bool IsAttackPathTile(Tile tile)
        {
            bool baseConditionMet = base.IsAttackPathTile(tile);
            var tileUnit = tile.Unit.Value;
            if (baseConditionMet ||
                tileUnit.Type == UnitType.CampWall || 
                tileUnit.Type == UnitType.CampFire);
                return false;
            return true;
        }
    }
}