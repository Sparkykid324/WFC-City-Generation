using UnityEngine;

namespace WFC
{
    [System.Serializable]
    public class Tile
    {
        public string tileName;
        public GameObject prefab;

        // Single socket per direction (loaded from CSV)
        public string northSocket;
        public string eastSocket;
        public string southSocket;
        public string westSocket;

        // Weight for random selection
        public float weight = 1f;

        // Y-axis rotation applied on spawn (0, 90, 180, 270)
        public float spawnRotation = 0f;

        public Tile(string name, GameObject tilePrefab)
        {
            tileName = name;
            prefab = tilePrefab;
        }

        public bool CanConnectNorth(Tile other)
        {
            if (northSocket == null || other.southSocket == null) return false;
            return northSocket == other.southSocket;
        }

        public bool CanConnectEast(Tile other)
        {
            if (eastSocket == null || other.westSocket == null) return false;
            return eastSocket == other.westSocket;
        }

        public bool CanConnectSouth(Tile other)
        {
            if (southSocket == null || other.northSocket == null) return false;
            return southSocket == other.northSocket;
        }

        public bool CanConnectWest(Tile other)
        {
            if (westSocket == null || other.eastSocket == null) return false;
            return westSocket == other.eastSocket;
        }
    }
}