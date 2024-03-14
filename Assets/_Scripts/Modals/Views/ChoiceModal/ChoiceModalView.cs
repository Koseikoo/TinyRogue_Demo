using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class ChoiceModalView : MonoBehaviour
    {
        [SerializeField] private ChoiceRenderer[] choiceRenderer;
        [SerializeField] private Button closeButton;
        public void Initialize(List<Choice> choices, bool canClose)
        {
            if(!canClose)
                closeButton.enabled = false;
            
            
            for (int i = 0; i < choiceRenderer.Length; i++)
            {
                if (i >= choices.Count)
                    choiceRenderer[i].Disable();
                else
                {
                    Action logic = choices[i].Logic;
                    choices[i].Logic = () =>
                    {
                        logic();
                        DestroyModal();
                    };
                    choiceRenderer[i].Render(choices[i]);
                }
            }
        }

        public void DestroyModal()
        {
            Destroy(gameObject);
        }
    }

    public class Choice
    {
        public string Text;
        public Sprite Icon;
        public Action Logic;
    }
}