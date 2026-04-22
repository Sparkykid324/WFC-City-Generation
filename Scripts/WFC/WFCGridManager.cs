using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace WFC
{
    public class WFCGridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int gridWidth = 5;
        public int gridHeight = 5;
        public float cellSize = 1f;
        
        [Header("Tile Data")]
        public TileSet tileSet;
        
        [Header("Road Density Control")]
        [Range(1, 4)]
        public int maxRoadNeighbours = 1;
        
        [Header("Zone Seeding")]
        public bool useZoneSeeding = true;
        [Range(3, 15)]
        public int blockSize = 6; // Distance between seeded road corridors
        
        [Header("Layout Style")]
        public bool organicLayout = false;
        [Range(1, 5)]
        public int organicVariance = 2;  // How much corridor spacing varies
        [Range(0f, 0.4f)]
        public float organicGapChance = 0.15f; // Chance of a gap in a corridor
        
        // Internal grid data
        private Cell[,] grid;
        
        void Start()
        {
            //Load tile data from CSV before initialising the grid
            TileCSVLoader loader = GetComponent<TileCSVLoader>();
            if (loader != null)
            {
                loader.LoadFromCSV();
                Debug.Log($"Tiles loaded: {tileSet.tiles.Count}");
                foreach (Tile t in tileSet.tiles) 
                    Debug.Log($"Loaded tile: '{t.tileName}'");
            }
            DebugTileCompatibility();
            InitializeGrid();
        }
        
        void DebugTileCompatibility()
        {
            Tile grass = tileSet.GetTileByName("Grass");
            Tile building = tileSet.GetTileByName("Building");
    
            if (grass == null || building == null)
            {
                Debug.LogError("Couldn't find Grass or Building tile — check names match CSV exactly");
                return;
            }
    
            Debug.Log($"Grass sockets  — N:{grass.northSocket} E:{grass.eastSocket} S:{grass.southSocket} W:{grass.westSocket}");
            Debug.Log($"Building sockets — N:{building.northSocket} E:{building.eastSocket} S:{building.southSocket} W:{building.westSocket}");
    
            Debug.Log($"Can Grass connect North to Building? {grass.CanConnectNorth(building)}");
            Debug.Log($"Can Grass connect East to Building?  {grass.CanConnectEast(building)}");
            Debug.Log($"Can Building connect North to Grass? {building.CanConnectNorth(grass)}");
            Debug.Log($"Can Building connect East to Grass?  {building.CanConnectEast(grass)}");
        }
        
        void InitializeGrid()
        {
            grid = new Cell[gridWidth, gridHeight];
            
            // Create each cell with all possible tiles
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x, y] = new Cell();
                    grid[x, y].possibleTiles = new List<Tile>(tileSet.tiles);
                    grid[x, y].position = new Vector2Int(x, y);
                }
            }
            
            Debug.Log($"Grid initialized: {gridWidth}x{gridHeight}");
        }
        
        // Run one iteration of WFC
        [ContextMenu("Generate Step")]
        public void GenerateStep()
        {
            if (grid == null)
            {
                InitializeGrid();
            }
            
            // Find cell with lowest entropy (fewest options)
            Cell cellToCollapse = GetLowestEntropyCell();
            
            if (cellToCollapse == null)
            {
                Debug.Log("Generation complete!");
                return;
            }
            
            // Collapse the cell to one tile
            CollapseCell(cellToCollapse);
            
            // Propagate constraints to neighbors
            PropagateConstraints(cellToCollapse);
            
            // Spawn the visual tile
            SpawnTile(cellToCollapse);
        }
        
        [ContextMenu("Generate All")]
        public void GenerateAll()
        {
            ClearGrid();
            InitializeGrid();

            // Seed road corridors before WFC runs
            if (useZoneSeeding)
                SeedRoadCorridors();

            int maxIterations = gridWidth * gridHeight + 10;
            int iterations = 0;

            while (iterations < maxIterations)
            {
                Cell cellToCollapse = GetLowestEntropyCell();

                if (cellToCollapse == null)
                {
                    Debug.Log($"Generation complete in {iterations} iterations!");
                    break;
                }

                CollapseCell(cellToCollapse);
                PropagateConstraints(cellToCollapse);
                SpawnTile(cellToCollapse);

                iterations++;
            }

            if (iterations >= maxIterations)
                Debug.LogWarning("Hit max iterations - possible contradiction in grid");
        }
        
        [ContextMenu("Clear Grid")]
        public void ClearGrid()
        {
            // Destroy all child GameObjects
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            
            grid = null;
        }
        
        Cell GetLowestEntropyCell()
        {
            Cell lowestEntropyCell = null;
            int lowestEntropy = int.MaxValue;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Cell cell = grid[x, y];
                    
                    // Skip already collapsed cells
                    if (cell.isCollapsed) continue;
                    
                    int entropy = cell.possibleTiles.Count;
                    
                    if (entropy < lowestEntropy && entropy > 0)
                    {
                        lowestEntropy = entropy;
                        lowestEntropyCell = cell;
                    }
                }
            }
            
            return lowestEntropyCell;
        }
        
        void CollapseCell(Cell cell)
        {
            if (cell.possibleTiles.Count == 0)
            {
                Debug.LogError($"Cell at {cell.position} has no valid tiles!");
                return;
            }

            // Only suppress roads if non-road alternatives actually exist
            int roadNeighbours = CountCollapsedRoadNeighbours(cell.position);
            if (roadNeighbours >= maxRoadNeighbours)
            {
                List<Tile> nonRoadTiles = cell.possibleTiles
                    .Where(t => !t.tileName.StartsWith("Road"))
                    .ToList();
            
                // Only apply suppression if it leaves valid options
                if (nonRoadTiles.Count > 0)
                    cell.possibleTiles = nonRoadTiles;
                // Otherwise leave possibleTiles alone - constraints have forced a road here
            }

            // Weighted random selection
            float totalWeight = cell.possibleTiles.Sum(t => t.weight);
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            Tile selectedTile = cell.possibleTiles[0];

            foreach (Tile tile in cell.possibleTiles)
            {
                currentWeight += tile.weight;
                if (randomValue <= currentWeight)
                {
                    selectedTile = tile;
                    break;
                }
            }

            cell.collapsedTile = selectedTile;
            cell.possibleTiles.Clear();
            cell.possibleTiles.Add(selectedTile);
            cell.isCollapsed = true;
        }
        
        void PropagateConstraints(Cell cell)
        {
            Stack<Vector2Int> cellsToCheck = new Stack<Vector2Int>();
            cellsToCheck.Push(cell.position);
            
            while (cellsToCheck.Count > 0)
            {
                Vector2Int currentPos = cellsToCheck.Pop();
                Cell currentCell = grid[currentPos.x, currentPos.y];
                
                // Check all four neighbors
                CheckNeighbor(currentPos, Vector2Int.up, currentCell, cellsToCheck);    // North
                CheckNeighbor(currentPos, Vector2Int.right, currentCell, cellsToCheck); // East
                CheckNeighbor(currentPos, Vector2Int.down, currentCell, cellsToCheck);  // South
                CheckNeighbor(currentPos, Vector2Int.left, currentCell, cellsToCheck);  // West
            }
        }
        
        void CheckNeighbor(Vector2Int pos, Vector2Int direction, Cell sourceCell, Stack<Vector2Int> cellsToCheck)
        {
            Vector2Int neighborPos = pos + direction;
            
            // Check bounds
            if (neighborPos.x < 0 || neighborPos.x >= gridWidth ||
                neighborPos.y < 0 || neighborPos.y >= gridHeight)
                return;
            
            Cell neighborCell = grid[neighborPos.x, neighborPos.y];
            
            // Skip collapsed cells
            if (neighborCell.isCollapsed) return;
            
            // Remove tiles from neighbor that can't connect to source
            List<Tile> validTiles = new List<Tile>();
            
            foreach (Tile neighborTile in neighborCell.possibleTiles)
            {
                bool isValid = false;
                
                foreach (Tile sourceTile in sourceCell.possibleTiles)
                {
                    // Check connection based on direction
                    if (direction == Vector2Int.up && sourceTile.CanConnectNorth(neighborTile))
                        isValid = true;
                    else if (direction == Vector2Int.right && sourceTile.CanConnectEast(neighborTile))
                        isValid = true;
                    else if (direction == Vector2Int.down && sourceTile.CanConnectSouth(neighborTile))
                        isValid = true;
                    else if (direction == Vector2Int.left && sourceTile.CanConnectWest(neighborTile))
                        isValid = true;
                    
                    if (isValid) break;
                }
                
                if (isValid)
                {
                    validTiles.Add(neighborTile);
                }
            }
            
            // If we removed tiles, update neighbor and add to check stack
            if (validTiles.Count < neighborCell.possibleTiles.Count)
            {
                neighborCell.possibleTiles = validTiles;
                cellsToCheck.Push(neighborPos);
            }
        }
        
        void SpawnTile(Cell cell)
        {
            if (cell.collapsedTile == null || cell.collapsedTile.prefab == null)
                return;

            Vector3 worldPosition = new Vector3(
                cell.position.x * cellSize,
                0,
                cell.position.y * cellSize
            );

            Quaternion rotation = Quaternion.Euler(0, cell.collapsedTile.spawnRotation, 0);
            GameObject tileObject = Instantiate(cell.collapsedTile.prefab, worldPosition, rotation);
            tileObject.transform.parent = this.transform;
            tileObject.name = $"{cell.collapsedTile.tileName}_{cell.position.x}_{cell.position.y}";
        }
        int CountCollapsedRoadNeighbours(Vector2Int pos)
        {
            int count = 0;
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbourPos = pos + dir;
        
                if (neighbourPos.x < 0 || neighbourPos.x >= gridWidth ||
                    neighbourPos.y < 0 || neighbourPos.y >= gridHeight)
                    continue;
            
                Cell neighbour = grid[neighbourPos.x, neighbourPos.y];
        
                if (neighbour.isCollapsed && neighbour.collapsedTile != null)
                {
                    string name = neighbour.collapsedTile.tileName;
                    if (name.StartsWith("Road") || name.StartsWith("Building"))
                        count++;
                }
            }
    
            return count;
        }
        void SeedRoadCorridors()
        {
            if (organicLayout)
                SeedOrganicCorridors();
            else
                SeedGridCorridors();

            // Propagate all seeded cells
            for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                if (grid[x, y].isCollapsed)
                    PropagateConstraints(grid[x, y]);

            Debug.Log("Zone seeding complete.");
        }
        
        void SeedGridCorridors()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    bool onVertical   = (x % blockSize == 0);
                    bool onHorizontal = (y % blockSize == 0);

                    if (onVertical && onHorizontal)
                        ForceCollapse(x, y, "Road_Cross");
                    else if (onVertical)
                        ForceCollapse(x, y, "Road_NS");
                    else if (onHorizontal)
                        ForceCollapse(x, y, "Road_EW");
                }
            }
        }
        
        void SeedOrganicCorridors()
        {
            // Build vertical corridors at irregular intervals
            List<int> verticalCorridors  = new List<int>();
            List<int> horizontalCorridors = new List<int>();

            // Start at a random offset, then step by blockSize +/- variance
            int x = Random.Range(1, blockSize);
            while (x < gridWidth)
            {
                verticalCorridors.Add(x);
                x += blockSize + Random.Range(-organicVariance, organicVariance + 1);
            }

            int y = Random.Range(1, blockSize);
            while (y < gridHeight)
            {
                horizontalCorridors.Add(y);
                y += blockSize + Random.Range(-organicVariance, organicVariance + 1);
            }

            // Seed the corridors — but organic roads have gaps
            foreach (int cx in verticalCorridors)
            {
                for (int cy = 0; cy < gridHeight; cy++)
                {
                    // Skip cells randomly to create road breaks
                    if (Random.value < organicGapChance) continue;
            
                    if (cx < gridWidth)
                    {
                        if (horizontalCorridors.Contains(cy))
                            ForceCollapse(cx, cy, "Road_Cross");
                        else
                            ForceCollapse(cx, cy, "Road_NS");
                    }
                }
            }

            foreach (int cy in horizontalCorridors)
            {
                for (int cx = 0; cx < gridWidth; cx++)
                {
                    if (Random.value < organicGapChance) continue;
            
                    if (grid[cx, cy].isCollapsed) continue; // Already seeded as cross
            
                    if (cy < gridHeight)
                        ForceCollapse(cx, cy, "Road_EW");
                }
            }
        }
        
        void ForceCollapse(int x, int y, string tileName)
        {
            Tile tile = tileSet.GetTileByName(tileName);

            if (tile == null)
            {
                Debug.LogWarning($"ZoneSeeding: Tile '{tileName}' not found in TileSet.");
                return;
            }

            Cell cell = grid[x, y];
            cell.collapsedTile = tile;
            cell.possibleTiles.Clear();
            cell.possibleTiles.Add(tile);
            cell.isCollapsed = true;

            SpawnTile(cell);
        }
    }
    
    
    // Helper class to represent each grid cell
    [System.Serializable]
    public class Cell
    {
        public Vector2Int position;
        public List<Tile> possibleTiles;
        public Tile collapsedTile;
        public bool isCollapsed = false;
    }
}