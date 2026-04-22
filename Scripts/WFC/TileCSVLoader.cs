using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace WFC
{
    public class TileCSVLoader : MonoBehaviour
    {
        [Header("CSV Settings")]
        [Tooltip("Filename inside StreamingAssets, e.g. TileConfig.csv")]
        public string csvFileName = "TileConfig.csv";

        [Header("References")]
        public TileSet tileSet;

        [Tooltip("All prefabs the CSV can reference by name")]
        public List<GameObject> prefabLibrary = new List<GameObject>();

        // Call this before WFCGridManager.InitializeGrid()
        public void LoadFromCSV()
        {
            string path = Path.Combine(Application.streamingAssetsPath, csvFileName);
            Debug.Log($"Looking for CSV at: {Path.Combine(Application.streamingAssetsPath, csvFileName)}");

            if (!File.Exists(path))
            {
                Debug.LogError($"TileCSVLoader: CSV not found at {path}");
                return;
            }

            string[] lines = File.ReadAllLines(path);

            if (lines.Length < 2)
            {
                Debug.LogError("TileCSVLoader: CSV has no data rows.");
                return;
            }

            tileSet.tiles.Clear();

            // Skip header row (line 0), parse from line 1 onward
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Skip empty lines or comment lines starting with #
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                string[] cols = line.Split(',');

                if (cols.Length < 8)
                {
                    Debug.LogWarning($"TileCSVLoader: Row {i} has fewer than 8 columns, skipping.");
                    continue;
                }

                Tile tile = new Tile(cols[0].Trim(), null);
                tile.prefab        = FindPrefab(cols[1].Trim());
                tile.northSocket   = cols[2].Trim();
                tile.eastSocket    = cols[3].Trim();
                tile.southSocket   = cols[4].Trim();
                tile.westSocket    = cols[5].Trim();
                tile.weight        = float.Parse(cols[6].Trim());
                tile.spawnRotation = float.Parse(cols[7].Trim());

                tileSet.tiles.Add(tile);
                Debug.Log($"TileCSVLoader: Loaded tile '{tile.tileName}'");
            }

            Debug.Log($"TileCSVLoader: {tileSet.tiles.Count} tiles loaded from {csvFileName}");
        }

        GameObject FindPrefab(string prefabName)
        {
            GameObject found = prefabLibrary.Find(p => p.name == prefabName);
            if (found == null)
                Debug.LogWarning($"TileCSVLoader: Prefab '{prefabName}' not found in library.");
            return found;
        }
    }
}