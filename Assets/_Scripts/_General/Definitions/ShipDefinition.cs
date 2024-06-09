using Models;
using Views;

[System.Serializable]
public class ShipDefinition
{
    public TileView[] shipViews;
    public TileView StartTile;
    public TileView HelmTile;
    public TileView MerchantTile;
    public TileView ModSmithTile;

    public Ship GetShipInstance()
    {
        Ship ship = new();

        foreach (var view in shipViews)
        {
            Tile tile = new(view.transform.position, null);
            view.Initialize(tile);
            ship.Tiles.Add(tile);

            if (view == StartTile)
                ship.StartTile = tile;
            if (view == HelmTile)
                ship.HelmTile = tile;
            if (view == MerchantTile)
                ship.MerchantTile = tile;
            if (view == ModSmithTile)
                ship.ModSmithTile = tile;
        }

        return ship;
    }
}