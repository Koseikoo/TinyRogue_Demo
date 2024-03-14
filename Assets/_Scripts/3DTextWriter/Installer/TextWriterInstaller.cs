using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Factories;
using UnityEngine;
using Views;
using Zenject;

namespace Installer
{
    [CreateAssetMenu(fileName = "TextWriterInstaller", menuName = "Installer/TextWriterInstaller")]
    public class TextWriterInstaller : ScriptableObjectInstaller<TextWriterInstaller>
    {
        [SerializeField] private GameObject[] prefabs;
        [SerializeField] private TextView _textPrefab;
        [SerializeField] private TextWriterView _textWriterPrefab;
        public override void InstallBindings()
        {
            Container.Bind<TextView>().FromInstance(_textPrefab).AsSingle();
            Container.Bind<TextWriterView>().FromInstance(_textWriterPrefab).AsSingle();
            
            Container.Bind<TextFactory>().AsSingle();

            Container.Bind<TextContainer>().FromInstance(new(prefabs)).AsSingle();
        }
    }

    public class TextContainer
    {
        private List<GameObject> textPrefabs = new();
        private Regex _regex;
        public TextContainer(GameObject[] prefabs)
        {
            _regex = new Regex("([^_]+)$");
            textPrefabs = new(prefabs);
        }
        
        public GameObject GetTextPrefab(char character)
        {
            var g = textPrefabs.FirstOrDefault(g =>
            {
                Match match = _regex.Match(g.name);
                string c = match.Groups[1].Value;
                c = RenameSpecialCharacters(c);
                string reference = character.ToString();
                return string.Equals(c, reference, StringComparison.CurrentCultureIgnoreCase);
            });
            if (g != null)
                return g;
            throw new Exception($"No Prefab with Ending {character.ToString()}");
        }

        private string RenameSpecialCharacters(string character)
        {
            return character switch
            {
                "&" => "Ampersand",
                "'" => "Apostrophe",
                "*" => "Asterix",
                "@" => "AT",
                "^" => "Caret",
                "]" => "CloseBracket",
                ":" => "Colon",
                "," => "Comma",
                "$" => "Dollar",
                "!" => "ExclamationPoint",
                "." => "FullStop",
                ">" => "GreaterThan",
                "#" => "Hash",
                "<" => "LessThan",
                "-" => "Minus",
                "[" => "OpenBracket",
                "%" => "Percent",
                "+" => "Plus",
                "?" => "QuestionMark",
                ";" => "Semicolon",
                "/" => "Slash",
                _ => character
            };
        }
    }
}