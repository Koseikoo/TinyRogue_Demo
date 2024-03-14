using UniRx;
using UnityEngine;

namespace Models
{
    public class TextWriterModel
    {
        public ReactiveProperty<Vector3> Position = new();
        public StringReactiveProperty Text = new();
        public BoolReactiveProperty IsDestroyed = new();

        public TextWriterModel(Vector3 position, string text)
        {
            Position.Value = position;
            Text.Value = text;
        }
    }
}