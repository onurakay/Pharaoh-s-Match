using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public sealed class BoardManager : MonoBehaviour
{
    // Singleton
    public static BoardManager Instance { get; private set; }

    // Serialized fields
    public Row[] rows;

    public Tile[,] tiles { get; private set; }

    // Board dimensions
    public int width => tiles.GetLength(0);
    public int height => tiles.GetLength(1);

    private readonly List<Tile> selection = new List<Tile>();

    private const float TweenDuration = 0.25f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        // Populating the tiles matrix
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++) // Corrected loop condition
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                // Initialize item outside the loop if possible for optimization
                tile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];

                tiles[x, y] = tile;
            }
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.A)) return;

        foreach (var connectedTile in tiles[0, 0].GetConnectedTiles())
        {
            connectedTile.icon.transform.DOScale(1.25f, TweenDuration).Play();
        }
    }

    // Selection Mechanism
    public async void Select(Tile tile)
    {
        bool isTileInSelection = selection.Contains(tile);

        if (!isTileInSelection)
        {
            selection.Add(tile);
        }

        // Only allow 2 selections
        if (selection.Count < 2) return;

        Debug.Log($"({selection[0].x}, {selection[0].y}) and ({selection[1].x}, {selection[1].y})");

        await Swap(selection[0], selection[1]);

        if (CanPop())
        {
            await PopAsync(); // Await the pop to ensure it's completed before proceeding
            // After popping, you might want to check for cascading pops
            // For simplicity, we'll assume one pop per swap
        }
        else
        {
            await Swap(selection[0], selection[1]); // Swap back if no pop
        }

        selection.Clear();
    }

    // Swapping Mechanism
    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, TweenDuration)).Join(icon2Transform.DOMove(icon1Transform.position, TweenDuration));

        await sequence.Play().AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.Item;

        tile1.Item = tile2.Item;
        tile2.Item = tile1Item;
    }

    // Determines if any pop can occur (requires 3 or more connected tiles)
    private bool CanPop()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++) // Corrected loop condition
            {
                // Skip the first tile and check if there are at least 2 more connected tiles (total 3)
                if (tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Popping Tiles Asynchronously
    private async Task PopAsync()
    {
        // To avoid multiple pops of the same group, keep track of already processed tiles
        bool popOccurred = false;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = tiles[x, y];
                var connectedTiles = tile.GetConnectedTiles();

                if (connectedTiles.Count < 3) // Changed condition to require 3 or more
                {
                    continue;
                }

                // Check if this group has already been processed
                if (connectedTiles.Any(t => t.icon.transform.localScale == Vector3.zero))
                {
                    continue;
                }

                popOccurred = true;

                var deflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles)
                {
                    deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration));
                }

                await deflateSequence.Play().AsyncWaitForCompletion();

                // Update score
                int totalValue = connectedTiles.Sum(t => t.Item.value);
                ScoreManager.Instance.Score += totalValue;

                // Replace items and inflate
                var inflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles)
                {
                    connectedTile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
                    connectedTile.icon.sprite = connectedTile.Item.sprite; // Ensure sprite is updated

                    inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration));
                }

                await inflateSequence.Play().AsyncWaitForCompletion();
            }
        }

        if (popOccurred)
        {
            // Optionally, check for cascading pops
            if (CanPop())
            {
                await PopAsync();
            }
        }
    }
}
