using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Factories;
using Models;
using Zenject;
using Random = UnityEngine.Random;

class SegmentSpacingTest : MonoBehaviour
{
    private const int SegmentSpawnTries = 40;
    [Inject] private PolygonConfig _polygonConfig;
    
    private PolygonFactory _polygonFactory;
    private Vector3[] _polygon;
    private List<Segment> _segments = new();

    [SerializeField] private float renderRadius;

    private void Awake()
    {
        _polygonFactory = new();
    }

    private void Start()
    {
        List<Segment> segments = new List<Segment>
        {
            new Segment(default),
        };
        _polygon = _polygonFactory.Create(_polygonConfig);
        GetSegmentPositions(segments);
    }

    private void GetSegmentPositions(List<Segment> segmentPool)
    {
        List<Segment> spacedSegments = new();
        Segment startSegment = new Segment(default);
        spacedSegments.Add(startSegment);
        _segments.Add(startSegment);

        while (_segments.Count > 0)
        {
            List<Segment> newSegments = new();
            
            for (int i = 0; i < _segments.Count; i++)
            {
                for (int j = 0; j < SegmentSpawnTries; j++)
                {
                    Segment randomSegment = segmentPool.PickRandom();
                    Vector3 randomDirection = Random.onUnitSphere;
                    randomDirection.y = 0;
                    randomDirection.Normalize();
                    
                    Vector3 testPosition = _segments[i].Position + ((_segments[i].Radius + randomSegment.Radius + .5f) * randomDirection);
                    randomSegment.Position = testPosition;

                    bool isWithinPolygon = randomSegment.IsWithinPolygon(_polygon, testPosition);
                    bool isInisideSegment = randomSegment.IsInsideSegment(spacedSegments);
                    bool isInisidePolygon = testPosition.IsInsidePolygon(_polygon);
                    
                    if (isWithinPolygon && !isInisideSegment && isInisidePolygon)
                    {
                        var segment = new Segment(default);
                        newSegments.Add(segment);
                        spacedSegments.Add(segment);
                    }
                }
            }
            
            _segments.Clear();
            _segments.AddRange(new List<Segment>(newSegments));
            newSegments.Clear();
            
        }

        _segments = new(spacedSegments);
        
    }
    
    private void OnDrawGizmos()
    {
        if(_polygon == null)
            return;
        
        for (int i = 0; i < _polygon.Length; i++)
        {
            var position = _polygon[i];
            var nextPosition = i == _polygon.Length - 1 ? 
                _polygon[0] :
                _polygon[i+1];
            
            Gizmos.DrawLine(position, nextPosition);
        }

        for (int i = 0; i < _segments.Count; i++)
        {
            Gizmos.DrawSphere(_segments[i].Position, _segments[i].Radius * renderRadius);
        }
    }
}