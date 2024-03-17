using System;
using System.Collections;
using System.Collections.Generic;
using Factories;
using Models;
using TMPro;
using UnityEngine;
using Views;
using Zenject;
using UniRx;

public class WaveSystem : MonoBehaviour
{
    [SerializeField] private EnemyDefinition waveEnemy;
    [SerializeField] private int enemiesPerWave;
    [SerializeField] private int wave;

    [Header("Visual")]
    [SerializeField] private TextMeshProUGUI waveProgress;
    [SerializeField] private TextMeshProUGUI waveText;
    
    
    [Inject] private UnitFactory _unitFactory;
    [Inject] private UnitUIFactory _unitUIFactory;
    [Inject] private UnitViewFactory _unitViewFactory;

    private Island _island;
    private Player _player;

    private Coroutine _waveRoutine;
    private IDisposable _unitsAddSubscription;
    private IDisposable _unitsRemoveSubscription;

    public void Initialize(Island island, Player player)
    {
        _player = player;
        _island = island;

        VisualWaveSubscriptions();
        _waveRoutine = StartCoroutine(WaveRoutine());
    }
    
    public void ResetSystem()
    {
        StopCoroutine(_waveRoutine);
        _unitsAddSubscription.Dispose();
        _unitsRemoveSubscription.Dispose();
    }

    private void VisualWaveSubscriptions()
    {
        _unitsAddSubscription = _island.Units.ObserveAdd().Subscribe(_ => VisualizeWave()).AddTo(this);
        _unitsRemoveSubscription = _island.Units.ObserveRemove().Subscribe(_ => VisualizeWave()).AddTo(this);
    }
    
    private void SpawnWave(Island island)
    {
        List<Tile> tiles = island.Tiles.PickRandomUniqueCollection(enemiesPerWave);

        foreach (var tile in tiles)
        {
            SpawnEnemy(tile);
        }
    }

    private void SpawnEnemy(Tile tileToSpawnOn)
    {
        Enemy enemy = _unitFactory.CreateEnemy(waveEnemy, tileToSpawnOn);
        EnemyView view = _unitViewFactory.CreateEnemyView(enemy, new(waveEnemy));
        _unitUIFactory.CreateUI(enemy, view.transform)
            .AddStateUI(enemy)
            .AddHealth(enemy)
            .AddTurnDelayUI(enemy);
    }

    private void VisualizeWave()
    {
        waveProgress.text = $"{_island.Units.Count} / {enemiesPerWave}";
        waveText.text = $"Wave {wave}";
    }

    private IEnumerator WaveRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => _island.Units.Count == 0);
            SpawnWave(_island);
            wave++;
        }
    }
    
    
}