Procedural City Generator — Wave Function Collapse (Unity 6 LTS)
GT601 Extended Project | BSc (Hons) Games Technologies (Top-Up) | Newcastle College Group
Author: Tymon Bulawa-Sobolewski
Engine: Unity 6 LTS | Language: C# | S2362615

**Overview**

This project is a procedural city generator built in Unity 6 LTS, implementing a custom Wave Function Collapse (WFC) algorithm to generate coherent urban layouts at runtime. The system places road networks, buildings, and green space across a configurable grid using constraint-based propagation — ensuring adjacency rules are always satisfied between tile types without manual level design.
The generator was developed as the practical artefact for the GT601 Extended Project module, with a focus on research-informed technical implementation, modular system architecture, and portfolio-quality output. All 3D assets (roads, buildings, grass variants) were custom-modelled in Blender and exported as FBX into Unity.

**Key Features**

Wave Function Collapse algorithm — entropy-guided tile selection with constraint propagation across a 2D grid
Socket-based adjacency system — directional socket matching (North/South/East/West) ensuring valid tile connections
Directional road variants — Road_NS, Road_EW, Road_Corner (×4 rotations), Road_TJunction (×4), Road_Cross; all oriented correctly at spawn via Blender-exported root wrapper pattern
Runtime prefab randomisation — BuildingVariantRandomiser and Grass_Randomiser scripts select from weighted variant pools at spawn, keeping the CSV configuration lean
CSV-driven tile configuration — tile weights, socket strings, and zone parameters loaded at runtime from StreamingAssets/tiles.csv via TileCSVLoader
CityTileSet ScriptableObject — centralised tile registry decoupled from generator logic
Zone seeding — grid-based and organic corridor spacing modes for varied city layouts
Road density suppression — weighted probability adjustments prevent over-generation of road tiles in non-corridor zones
Custom Blender assets — all road types, building variants, and grass variants modelled and textured from scratch


**Requirements**

Unity 6 LTS (6000.0.x) — earlier versions are not guaranteed to work
Unity Input System package (installed via Package Manager)
No additional third-party packages required
Runs on Windows (tested); Mac/Linux not verified


**Getting Started**

Two options are available depending on how you want to work with the project.

**Option A** — Unity Package (Recommended)

*The packaged .unitypackage file is the simplest way to get everything into an existing Unity project.*

Create a new Unity 6 LTS project (3D Core template).

In the menu bar, go to Assets → Import Package → Custom Package.

Select the .unitypackage file from this repository.

Import all assets when prompted.

Open the scene at Assets/Scenes/CityGenerator.unity.

Press Play. The generator will run automatically.

**Option B** — *Manual Folder Import*

If you prefer to bring the folders in directly:

Create a new Unity 6 LTS project (3D Core template).
Clone or download this repository.
Copy the following folders into your project's Assets/ directory:

Art
Misc
Scenes
Scripts
StreamingAssets
Tiles


Unity will reimport all assets automatically.

**Usage**

Open Assets/Scenes/CityGenerator.unity.
Press Play in the Unity Editor. Then:

1. In the hierarchy, find WFC_Manager
2. In the inspector, find the WFC Grid Manager Script Component
3. Play around with the settings, such as the zone seeding, organic layout, and grid size
4. When ready, right-click the component, and press generate all
5. If you'd like to change the settings again, right-click Clear Grid on the script component, change the settings, and click Generate All again


**Configuration**

All tile behaviour is driven by the CSV file at:
Assets/StreamingAssets/tiles.csv
Each row defines a tile type with the following columns:

ColumnDescriptionTileNameMust match the prefab name exactly (e.g. Road_Straight)

WeightRelative spawn probability (higher = more frequent)

Socket NSocket identifier for the North face

Socket SSocket identifier for the South face

Socket ESocket identifier for the East face

Socket WSocket identifier for the West

faceZoneTypeAssigns tile to a zone category (Road / Building / Green)

Socket values must match symmetrically — if tile A has SocketN = road, the tile placed to its north must have SocketS = road.
To modify tile weights or add variants, edit tiles.csv directly. No recompilation is needed; the loader reads from StreamingAssets at runtime.

**Swapping or Replacing Models**

All visual assets are decoupled from the generator logic. The WFC system operates entirely on prefab references and socket data — it has no dependency on mesh geometry — so replacing a model is non-destructive and requires no code changes.
To replace a model:

Import your new .fbx into Assets/Art/Models/.
Open the corresponding prefab in Assets/Misc/Prefabs/ (e.g. Road_Straight).
Replace the mesh on the child object (the mesh child, not the root wrapper) with your new model.
Ensure the root GameObject remains at position (0, 0, 0) with no rotation. Any orientation correction should be applied to the mesh child only, not the root — the generator writes spawnRotation to the root at placement time.
Reassign or recreate materials as needed in the Inspector.
Save the prefab. The change will apply to all future generated cities automatically.

For building and grass variants specifically, the BuildingVariantRandomiser and Grass_Randomiser scripts select from a pool of child GameObjects at runtime. To add a new variant, duplicate an existing child under the randomiser prefab, swap its mesh, and it will automatically enter the weighted selection pool with no CSV or script edits required.

Important: Road prefabs use a root wrapper pattern to preserve correct spawn orientation. Do not apply Y-axis rotation to the root — always rotate the mesh child instead. Collapsing the hierarchy will break directional placement.

**How the WFC Algorithm Works**

Initialisation — every cell in the grid is assigned the full set of possible tile types (superposition).
Entropy selection — the cell with the lowest entropy (fewest remaining possibilities) is selected for collapse.
Collapse — a tile is chosen from that cell's remaining options using weighted random selection.
Propagation — neighbouring cells have their possibility sets reduced based on the socket constraints of the collapsed tile. This cascades outward until no further reductions can be made.
Repeat — steps 2–4 continue until all cells are collapsed or a contradiction is reached.
Contradiction handling — if a cell reaches zero valid options, the generator resets and retries from a new seed.

Zone seeding pre-collapses certain cells before the main loop begins, biasing the output toward structured corridor layouts.

**Asset Credits**
All 3D models were created by the author in Blender 4.x and exported as .fbx into Unity. No third-party or store-purchased models are used. Textures are either procedural (Unity materials) or author-created.

**Licence**
This project is submitted for academic assessment. All original code and assets are the work of the author. Redistribution or reuse requires written permission.
