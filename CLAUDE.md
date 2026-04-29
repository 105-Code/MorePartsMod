# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build

This is a .NET 4.8 class library (C#) mod for the game *Spaceflight Simulator*. There are no tests.

**Prerequisites:**
- Copy all `.dll` files from `<game install>/Spaceflight Simulator_Data/Managed/` into `Dependencies/`
- Copy `MoreParts/env.props.example` to `MoreParts/env.props` and set the paths for your system

**Build (Linux):**
```bash
cd MoreParts
msbuild
# Output: bin/Debug/MorePartsMod.dll
```

The post-build step automatically calls the `UpdatePackCli` tool to inject the new `.dll` into the installed `.pack` file and copies it to the Unity toolkit's `DLLs/` folder.

## Architecture

`MorePartsPack` (a Unity `ScriptableObject`) is the mod's entry point. On `OnEnable` it runs Harmony `patchAll`, registers scene lifecycle hooks, and exposes top-level references (`ColonyBuildingFactory`, `ColoniesInfo`, `SpawnPoint`, `AntennaPrefab`).

### Scene lifecycle

- **Hub scene** → loads `ColoniesInfo` from `Colonies.json` (world-persistent save).
- **World scene** → spawns a manager `GameObject` with two `MonoBehaviour` components: `ColonyManager` and `ResourcesManger`. Both are destroyed (and saved) on world unload.
- **Home scene** → clears `ColoniesInfo`.

### Managers

| Class | Responsibility |
|---|---|
| `ColonyManager` | Spawns/saves/deletes colony `GameObject`s; handles key input for open/insert/extract; owns the "Create Colony" button |
| `ResourcesManger` | Tracks resource deposit data per planet; detects when the player enters/exits a deposit radius |

Both expose a `Main` static singleton set in `Awake`.

### Colony data flow

`ColonyData` is the serializable data model (JSON-serialized to `Colonies.json`). `ColonyComponent` is the runtime `MonoBehaviour` wrapper around a `ColonyData`. Buildings inside a colony follow the same pattern: `Building` is the serializable struct (position + rotation), while actual Unity `GameObject`s are spawned/restored by `ColonyComponent.RestoreBuildings()`.

`ColonyBuildingFactory` (a plain serializable class set on `MorePartsPack` via Unity inspector) holds the Unity prefab references for each building type and their construction costs. Building names come directly from `prefab.name`.

### Dependency injection into buildings

`ColonyComponent` injects runtime references into building `GameObject`s using marker interfaces: `INJ_Rocket`, `INJ_Colony`, `INJ_Building`, `INJ_HasEnergy`, and `OnInit`. When a building's `GameObject` implements one of these interfaces, `ColonyComponent` sets the value automatically on spawn and on player change.

### ARPANET

`AntennaComponent` (a colony building spawned at the Space Center by `SpaceCenterPatcher`) owns an `ARPANET` graph. `ARPANET` is a simple linked-list connectivity graph of `Node`s. Each `Node` wraps a `WorldLocation`. Line-of-sight is computed geometrically (quadratic line/circle intersection). `TelecommunicationDishModule` (a rocket part) adds/removes itself as a node and checks connectivity every 3 seconds, toggling `rocket.hasControl`.

### Harmony patches

All patches are in `Patches/` and applied via `harmony.PatchAll()`:

| Patcher | Target | Purpose |
|---|---|---|
| `SpaceCenterPatcher` | `SpaceCenter.Start` (prefix) | Spawns the antenna prefab at the Space Center |
| `MapManagerPatcher` | `MapManager.DrawLandmarks` (postfix) | Draws colony/deposit/antenna landmarks and telecom lines on the map |
| `RocketManagerPatcher` | `RocketManager.SpawnBlueprint` (postfix) | Relocates the spawned rocket to a colony launch pad when `SpawnPoint` is set |
| `BuildManagerPatcher` | `BuildManager.Start` (postfix) + `BuildManager.Launch` (prefix) | Injects the "Colonies" button in the build screen; validates/deducts `Rocket_Material` before launch |

### Part modules

Custom part behaviours live in `Parts/`. They use SFS's `INJ_IsPlayer`, `INJ_Rocket`, `INJ_Location` interfaces for runtime injection. Key modules: `TelecommunicationDishModule`, `ExcavatorModule`, `ScannerModule` (GeoEye), `RotorModule`, `BallonModule`, `HingeModule`, `ContinuousTrackModule`.

### Persistence

`FileUtils` wraps `JsonWrapper` to read/write JSON into the game's world-persistent path (`<world>/Persistent/<filename>`). Colonies are saved as `Colonies.json`; planet resource deposits as `PlanetResources.json`. Save is triggered manually (never on a timer) after any mutation.

### Resource types

Defined as constants in `MorePartsTypes`: `Electronic_Component`, `Construction_Material`, `Material`, `Rocket_Material`. Only these four are accepted by `ColonyData.AddResource`/`TakeResource`.

### Key bindings

`KeySettings` extends `ModKeybindings`. Defaults: `Y` toggle antenna, `U` open colony menu, `I` toggle telecom lines, `J` insert resources, `K` extract resources.