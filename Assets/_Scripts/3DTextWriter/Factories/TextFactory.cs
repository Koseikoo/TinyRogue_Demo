using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;
using Views;
using Zenject;

namespace Factories
{
    public class TextFactory
    {
        private const float LetterDistance = .5f;
        
        [Inject] private TextView _textPrefab;
        [Inject] private TextWriterView _textWriterPrefab;
        private List<TextView> _textPool = new();
        [Inject] private DiContainer _container;
        
        public TextWriterView Create(Vector3 position, string text)
        {
            TextWriterModel textWriter = new(position, text);
            var writerView = _container.InstantiatePrefab(_textWriterPrefab).GetComponent<TextWriterView>();
            writerView.Initialize(textWriter);
            Vector3[] charPositions = GetTextPositions(textWriter);

            for (int i = 0; i < textWriter.Text.Value.Length; i++)
            {
                char c = textWriter.Text.Value[i];
                var view = PickCharacter(c, charPositions[i]);
                
                textWriter.IsDestroyed
                    .Where(b => b)
                    .Subscribe(_ => view.ResetView())
                    .AddTo(writerView);
            }
            return writerView;
        }

        private TextView PickCharacter(char character, Vector3 position)
        {
            TextView text = _textPool.FirstOrDefault(view => view.Character == character && !view.gameObject.activeSelf);
            if (text == null)
            {
                text = _container.InstantiatePrefab(_textPrefab).GetComponent<TextView>();
                text.Initialize(character, position);
                _textPool.Add(text);
            }

            return text;
        }

        private Vector3[] GetTextPositions(TextWriterModel writer)
        {
            Vector3[] positions = new Vector3[writer.Text.Value.Length];
            Vector3 currentPosition = writer.Position.Value + (Vector3.right * (writer.Text.Value.Length * .5f * LetterDistance));

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = currentPosition;
                currentPosition += Vector3.left * LetterDistance;
            }

            return positions;
        }
    }
}