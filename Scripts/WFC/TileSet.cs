using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace WFC
{
    [CreateAssetMenu(fileName = "New TileSet", menuName = "WFC/TileSet")]
    public class TileSet : ScriptableObject
    {
        public List<Tile> tiles = new List<Tile>();

        public Tile GetTileByName(string name)
        {
            return tiles.Find(t => t.tileName == name);
        }

    }
}

