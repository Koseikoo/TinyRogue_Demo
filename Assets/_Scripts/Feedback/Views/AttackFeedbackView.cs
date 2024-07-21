using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;

namespace TinyRogue
{
    public class AttackFeedbackView : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float showDuration;
        public void Initialize(List<Tile> tiles)
        {
            StartCoroutine(FeedbackRoutine(tiles));
        }

        private IEnumerator FeedbackRoutine(List<Tile> tiles)
        {
            yield return StartCoroutine(ShowRoutine(tiles));
            yield return StartCoroutine(DisappearRoutine());
        }
        
        private IEnumerator ShowRoutine(List<Tile> tiles)
        {
            yield return new WaitUntil(() => !GameStateContainer.Player.InAction);
            tiles.Insert(0, GameStateContainer.Player.Tile.Value);
            lineRenderer.positionCount = tiles.Count;
            lineRenderer.SetPositions(tiles.Select(tile => tile.WorldPosition).ToArray());
        }

        private IEnumerator DisappearRoutine()
        {
            float time = 0;
            float startWidth = lineRenderer.widthMultiplier;
            while (time < showDuration)
            {
                time += Time.deltaTime;
                float t = time / showDuration;
                lineRenderer.widthMultiplier = Mathf.Lerp(startWidth, 0f, t);
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}