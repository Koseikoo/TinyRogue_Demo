using System;
using System.Collections.Generic;
using Factories;
using Models;
using UniRx;
using UnityEngine;
using Zenject;

public static class WorldLootContainer
{
    public static List<Loot> DroppedLoot = new();
    public static ReactiveCommand DropLoot = new();
    public static ReactiveCommand ClaimMerchantCoins = new();

    public static void AddToLootDrops(Loot loot)
    {
        if (loot == null)
            throw new Exception($"Loot is Null");

        DroppedLoot.Add(loot);
    }
}