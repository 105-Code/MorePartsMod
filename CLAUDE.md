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

The "ARPANET" feature implements a satellite-constellation control network. Unmanned rockets only retain `hasControl` while a `TelecommunicationDishModule` finds a line-of-sight relay path back to a ground station (Space Center or any colony). Manned rockets — any rocket containing a part named `Capsule` — bypass the system entirely; the dish module disables itself in `Start`.

**Topology**

A single `ARPANET` instance is owned by the singleton `AntennaComponent`. Three kinds of `Node`s exist in its flat `_nodes` list:
- **Space Center node** (origin): inserted by `AntennaComponent.Awake` from `Position`.
- **Colony nodes** (origin): every `ColonyComponent.Start` calls `AntennaComponent.main.AddNode(Location, true)`. Colonies act as ground stations even without any explicit "antenna building" — the colony itself is the relay.
- **Rocket dish nodes** (non-origin): added by `TelecommunicationDishModule.Start` (or `_toggle` on power-on), removed in `OnDestroy` and on toggle-off.

`Node.Next` is *not* an adjacency-list edge — it caches the next hop along a previously-found route back to an origin. Edges are computed on demand by `Node.IsAvailableTo`, so the graph is conceptually a fully-connected mesh filtered by line-of-sight at query time.

**Connectivity check (`AntennaComponent.IsConnected`)**

Called every 3s by each player-controlled dish in `FixedUpdate`. Two-stage:
1. **Fast path** — if `origin.Next != null`, walk the cached route via `ARPANET.CheckRoute`, re-validating each hop's line-of-sight. If still valid, return true without re-searching.
2. **Slow path** — if no cached route or it broke, run `ARPANET.IsConnected`, an iterative BFS over `_nodes`. The search terminates as soon as any node with `IsOrigin == true` is dequeued, guaranteeing the discovered route uses the fewest hops. On success, the BFS walks its `parent` map back to the start and writes `Next` pointers along that path, building the cached route from the rocket to an origin.

`AntennaComponent` stores the last successfully-routed origin in `_routeOrigin` so the map renderer and disconnection cleanup can find it. `ClearRoute` walks the `Next` chain and nulls each pointer.

**Line-of-sight (`Node.IsAvailableTo`)**

For each node pair, every loaded planet in `Base.planetLoader.planets.Values` is tested via segment-vs-circle: project the planet center onto the segment from origin to target; reject if the projection lies inside the segment (`0 < t < 1`) and within the planet's radius. Robust to vertical lines (no slope-intercept), and a third unrelated body now blocks LOS too.

**Disconnection conditions**

`TelecommunicationDishModule.DoDisconnection` clears `Rocket.hasControl` when:
- DFS finds no path to any origin.
- `WorldTime.timewarpSpeed > _maxTimeWarp` (3 by default; 5 on the difficulty whose `MaxPhysicsTimewarpIndex == 3`).
- Player toggles the dish off (`_toggle` / key `Y` / part-use action).

Status messages use `MsgDrawer.main.Log` ("Connected" / "No Connection" / "Telecommunication Dish On/Off") and are gated by `_notifyConnection` / `_notifyDisconnection` flags to avoid spamming.

**Map rendering**

`MapManagerPatcher` postfixes `MapManager.DrawLandmarks` and calls `AntennaComponent.DrawInMap`, which:
- Draws the Space Center landmark.
- If `ShowTelecommunicationLines` (set true on successful connection), `_enableTelecommunicationLines` (toggled by key `I`, persisted via `KeySettings.ToggleShowTelecommunicationLines`), and the player rocket has a dish, walks `_routeOrigin → Next → …` and renders the path with `Map.solidLine.DrawLine` in the Sun's reference frame.

**Persistence**

The ARPANET is rebuilt from scratch on every world load — node IDs are not stable across sessions and nothing in `Colonies.json` references them. The graph re-forms naturally as `AntennaComponent.Awake`, each `ColonyComponent.Start`, and each `TelecommunicationDishModule.Start` run during scene load.

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