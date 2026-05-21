# Project Gamble

A scalable, systems-driven game built in Godot, utilizing a decoupled "DayZ-style" architecture to separate core logic from visual representation.

## Architectural Philosophy

This project strictly avoids "spaghetti code" by refusing to dump game logic directly into the `PlayerController` or visual nodes. It is built on three core pillars:

1. **Game.Core vs Game.Bridge:** Logic and state management are isolated from the Godot Scene Tree wherever possible. Nodes act as "Bridges" that visually represent the underlying data state.
2. **State Machines:** Environmental interactables (like soil, doors, etc.) manage their own state internally.
3. **Command/Bridge Pattern:** Player tools and weapons use abstract bridge classes to execute logic, keeping the player input clean and modular.

## Core Systems

### 1. The Interaction Pipeline
Input is strictly routed based on context, keeping the input map minimal and predictable:
* **Left Click (Active Action):** Triggers `CurrentWeapon.PerformAttack()`. Used for destructive or transformative actions (e.g., swinging a weapon, digging with a shovel).
* **'F' Key (Contextual Action):** Triggers `HandleInteraction()` via raycast. Used for constructive actions (e.g., planting seeds, harvesting, picking up items, opening doors).

### 2. Farming & Tool System
The farming pipeline uses a strict Separation of Concerns:
* **`SoilPatch.cs` (Data Container):** Attached to static bodies. Acts as a state machine tracking `Empty`, `Tilled`, or `Planted` states.
* **`ToolBridge.cs` (Interaction Base):** Abstract class handling tool usage.
* **`ShovelBridge.cs`:** A specific tool implementation that calls `TryDig()` on a targeted `SoilPatch`.

### 3. UI & Menu Management
* UI is handled via decoupled `CanvasLayer` scenes (HUD and Escape Menu).
* Pause logic and input management prevent the game and UI from fighting over input control (e.g., proper Escape key routing).

## Development Setup

* **Engine:** Godot 4.x (C# / .NET)
* **Version Control:** Git. Note: The `.godot/` cache and `.import/` folders are strictly ignored via `.gitignore` to prevent repository bloat.

## Immediate Roadmap

- [ ] Implement `SoilPatch` State Machine with debug console outputs.
- [ ] Create base `ToolBridge` and `ShovelBridge` implementation.
- [ ] Connect Left-Click active tool usage to `SoilPatch.TryDig()` logic.
- [ ] Connect 'F' contextual interaction to `SoilPatch.TryPlant()` logic.
