using System;
using Container;
using Models;
using UnityEngine;

public enum GrassType
{
    None,
    Base,
    Surface,
    Board,
    Bend60,
    Straight,
    End
}

public enum BoardType
{
    None,
    Chiseled,
    Stone,
    Metal
}

public enum BridgeType
{
    None,
    Bridge,
    Broken
}

public enum TerrainType
{
    None,
    Top,
    Weak,
    Surface
}

public class TileActionContainer
{
    public Action<Unit> NextIslandAction;
    public Action<Unit> IslandEndAction;
}

public class UnitActionContainer
{
    public Action<Interactable> OpenModSmithUI;
    public Action<Interactable> SelectIslandAction;
    public Action<Interactable> OpenChest;
    public Action<Interactable> LevelUp;

    public Action<Interactable> GetUnitAction(UnitType type)
    {
        return type switch
        {
            UnitType.HelmInteractable => SelectIslandAction,
            UnitType.ChestInteractable => OpenChest,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public class GrassContainer
{
    private GameObject _base;
    private GameObject _board;
    private GameObject _surface;
    private GameObject _pathBend60;
    private GameObject _pathStraight;
    private GameObject _pathEnd;

    public GrassContainer(
        GameObject baseGrass,
        GameObject board,
        GameObject surface,
        GameObject bend60,
        GameObject straight,
        GameObject end
    )
    {
        _base = baseGrass;
        _board = board;
        _surface = surface;
        _pathBend60 = bend60;
        _pathStraight = straight;
        _pathEnd = end;
    }

    public GameObject GetGrass(GrassType type)
    {
        return type switch
        {
            GrassType.Base => _base,
            GrassType.Surface => _surface,
            GrassType.Board => _board,
            GrassType.Bend60 => _pathBend60,
            GrassType.Straight => _pathStraight,
            GrassType.End => _pathEnd,
            _ => null
        };
    }
}

public class BoardContainer
{
    public BoardContainer(
        GameObject metalBorder,
        GameObject stoneChiseled,
        GameObject[] stoneVariants)
    {
        _metalBorder = metalBorder;
        _stoneChiseled = stoneChiseled;
        _stoneVariants = stoneVariants;
    }

    private GameObject _metalBorder;
    private GameObject _stoneChiseled;
    private GameObject[] _stoneVariants;

    public GameObject GetBoard(BoardType type)
    {
        return type switch
        {
            BoardType.Stone => _stoneVariants.Random(),
            BoardType.Chiseled => _stoneChiseled,
            BoardType.Metal => _metalBorder,
            _ => null
        };
    }
}

public class BridgeContainer
{
    public BridgeContainer(
        GameObject bridge,
        GameObject bridgeBroken)
    {
        _bridge = bridge;
        _bridgeBroken = bridgeBroken;
    }

    private GameObject _bridge;
    private GameObject _bridgeBroken;

    public GameObject GetBridge(BridgeType type)
    {
        return type switch
        {
            BridgeType.Bridge => _bridge,
            BridgeType.Broken => _bridgeBroken,
            _ => null
        };
    }
}

public class TerrainContainer
{
    private GameObject _top;
    private GameObject _topSurface;
    private GameObject _topWeak;

    public TerrainContainer(
        GameObject top,
        GameObject topSurface,
        GameObject topWeak)
    {
        _top = top;
        _topSurface = topSurface;
        _topWeak = topWeak;
    }

    public GameObject GetTerrain(TerrainType type)
    {
        return type switch
        {
            TerrainType.Top => _top,
            TerrainType.Surface => _topSurface,
            TerrainType.Weak => _topWeak,
            _ => null
        };
    }
}