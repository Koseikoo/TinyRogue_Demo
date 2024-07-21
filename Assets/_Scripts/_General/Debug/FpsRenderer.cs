using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FpsRenderer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;

    private List<int> entries = new();
    [SerializeField] private int entriesStored = 20;

    private void Update()
    {
        entries.Add(Mathf.RoundToInt(1 / Time.deltaTime));
        if(entries.Count > entriesStored)
        {
            entries.RemoveAt(0);
        }
        fpsText.text = $"F/s: {(int)entries.Average()}";
    }
}