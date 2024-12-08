using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    //coordinates in the board space
    public int x;
    public int y;

    private Item _item;

    public Item Item
    {
        get => _item;

        set
        {
            if (_item == value) return;
            _item = value;
            icon.sprite = _item.sprite;
        }
    }

    public Image icon;
    public Button button;

    public Tile GetLeft => x > 0 ? BoardManager.Instance.tiles[x - 1, y] : null;

    public Tile GetTop => y > 0 ? BoardManager.Instance.tiles[x, y - 1] : null;

    public Tile GetRight => x < BoardManager.Instance.width - 1 ? BoardManager.Instance.tiles[x + 1, y] : null;

    public Tile GetBottom => y < BoardManager.Instance.height - 1 ? BoardManager.Instance.tiles[x, y + 1] : null;

    public Tile[] Neighbours => new[]
    {
        GetLeft,
        GetTop,
        GetRight,
        GetBottom,
    };


    private void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        BoardManager.Instance.Select(this);
    }

    public List<Tile> GetConnectedTiles(List<Tile> exclude = null)
    {
        var result = new List<Tile> { this, };

        if (exclude == null)
        {
            exclude = new List<Tile> { this, };
        }
        else 
        {
            exclude.Add(this);
        }

        foreach (var neighbour in Neighbours)
        {
            if (neighbour == null || exclude.Contains(neighbour) || neighbour.Item != Item) continue;

            result.AddRange(neighbour.GetConnectedTiles(exclude));
        }

        return result;
    }
}
