**WFC Procedural City Generator — v0.0.1**
Wave Function Collapse Procedural City Generator for Unity 6 LTS

**What's Included**
This .unitypackage contains the core WFC city generation system, including:

Wave Function Collapse solver with constraint propagation and backtracking
CSV-driven tile configuration system (via StreamingAssets and TileCSVLoader)
CityTileSet ScriptableObject for tile and socket rule management
Weighted spawn probabilities and road density suppression
Zone seeding with grid-based and organic corridor spacing modes
Directional road tile variants (Road_NS, Road_EW, corners, T-junctions, crossroads)
BuildingVariantRandomiser prefab-level randomisation system
Custom 3D assets (roads, buildings, grass variants) exported from Blender as FBX


**Requirements**

Unity 6 LTS (6000.x)
Render Pipeline: Built-in (URP compatible with minor shader adjustments)


**Installation**

Download the .unitypackage file from this release
In Unity: Assets → Import Package → Custom Package
Select the downloaded file and import all assets
Ensure StreamingAssets/tiles.csv is present for runtime tile configuration
