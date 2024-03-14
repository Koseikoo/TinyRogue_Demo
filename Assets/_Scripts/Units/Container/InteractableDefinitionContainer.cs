using System;

namespace Container
{
    public class InteractableDefinitionContainer
    {
        private InteractableDefinition _helm;
        private InteractableDefinition _chest;

        public InteractableDefinitionContainer(InteractableDefinition helm,
            InteractableDefinition chest)
        {
            _helm = new(helm);
            _chest = new(chest);
        }

        public InteractableDefinition GetInteractableDefinition(UnitType type)
        {
            return type switch
            {
                UnitType.HelmInteractable => _helm,
                UnitType.ChestInteractable => _chest,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"No Interactable definition for type {type}")
            };
        }
    }
}