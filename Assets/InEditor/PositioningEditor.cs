using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PositioningEditor : MonoBehaviour
{
    [SerializeField] private Transform[] Reference;
    [SerializeField] private Transform[] NewTiles;
    [SerializeField] private bool UpdateNewTiles;

    private void OnValidate()
    {
        if (UpdateNewTiles)
        {
            UpdateNewTiles = false;
            UpdateTilePositions();
        }
    }

    private void UpdateTilePositions()
    {
        for (int i = 0; i < NewTiles.Length; i++)
        {
            NewTiles[i].position = Reference[i].position;
        }
    }
}
