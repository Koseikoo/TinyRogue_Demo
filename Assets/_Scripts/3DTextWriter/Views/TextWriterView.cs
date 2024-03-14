using System;
using Factories;
using Models;
using UniRx;
using UnityEngine;
using Zenject;

namespace Views
{
    public class TextWriterView : MonoBehaviour
    {
        private TextWriterModel _textWriter;

        public void Initialize(TextWriterModel textWriter)
        {
            _textWriter = textWriter;
            transform.position = textWriter.Position.Value;

            _textWriter.IsDestroyed
                .Where(b => b)
                .Subscribe(_ => DestroyText())
                .AddTo(this);
        }

        private void DestroyText()
        {
            Destroy(gameObject);
        }
    }
}