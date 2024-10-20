using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;

public static class GeneralExtensions
{
    public static T Random<T>(this ICollection<T> collection)
    {
        if (collection == null || collection.Count == 0)
        {
            throw new System.InvalidOperationException("Collection is Null or Empty");
        }

        int index = UnityEngine.Random.Range(0, collection.Count);
        int i = 0;
        foreach (T item in collection)
        {
            if (i == index)
            {
                return item;
            }

            i++;
        }

        throw new System.InvalidOperationException("This should never happen.");
    }

    public static void Remove<T>(this List<T> collection, List<T> toRemove)
    {
        List<T> temp = collection;
        for (int i = 0; i < toRemove.Count; i++)
        {
            if (temp.Contains(toRemove[i]))
            {
                temp.Remove(toRemove[i]);
            }
        }
    }

    public static string AddSpace(this string name)
    {
        List<int> capitalIndexList = new List<int>();
        for (int charIndex = 0; charIndex < name.Length; charIndex++)
        {
            if (Char.IsUpper(name[charIndex]))
            {
                capitalIndexList.Add(charIndex);
            }
        }
        
        capitalIndexList.RemoveAt(0);
        if (capitalIndexList.Count == 0)
        {
            return name;
        }

        for (int i = 0; i < capitalIndexList.Count; i++)
        {
            name = name.Insert(capitalIndexList[i], " ");
        }

        return name;
    }

    public static List<T> PickRandomUniqueCollection<T>(this ICollection<T> collection, int amount)
    {
        List<T> picked = new();
        while (picked.Count < amount)
        {
            T pick = collection.Random();
            if(!picked.Contains(pick))
            {
                picked.Add(pick);
            }
        }

        return picked;
    }

    public static Vector3 Truncate(this Vector3 vector, int hexTiles)
    {
        return (vector.magnitude - (Island.TileDistance * hexTiles)) * vector.normalized;
    }

    public static List<T> Truncate<T>(this List<T> collection, int amount)
    {
        List<T> truncatedCollection = new(collection);
        if (truncatedCollection.Count <= amount)
        {
            return new List<T>();
        }

        for (int i = truncatedCollection.Count - 1; i >= collection.Count - amount; i--)
            truncatedCollection.RemoveAt(i);

        return truncatedCollection;
    }
}