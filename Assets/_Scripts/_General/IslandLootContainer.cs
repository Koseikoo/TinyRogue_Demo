using System;
using System.Collections.Generic;
using Factories;
using Models;
using UniRx;
using Zenject;

public static class IslandLootContainer
{
    public static List<Loot> DroppedLoot = new();
    public static ReactiveCommand<bool> DropLoot = new();

    public static void AddToLootDrops(Loot loot)
    {
        if (loot == null)
            throw new Exception($"Loot is Null");

        DroppedLoot.Add(loot);
    }
}