namespace Container
{
    public class TrapDefinitionContainer
    {
        private TrapDefinition _trapDefinition;

        public TrapDefinitionContainer(TrapDefinition trapDefinition)
        {
            _trapDefinition = new(trapDefinition);
        }

        public TrapDefinition GetTrapDefinition()
        {
            return _trapDefinition;
        }
    }
}