# Project Gamble

A systems-driven survival game built in Godot 4 using C#. The project follows a strict decoupled architecture to keep logic clean, testable, and separated from visual/scene representation.

## Architectural Philosophy

This project avoids dumping logic into visual nodes. It is built around clear separation of concerns:

- **Game.Core vs Game.Bridge**: Pure C# logic and state live in `src/Core`. Godot nodes in `src/Bridge` act as visual representations and input bridges.
- **State Machines & Data-Driven Design**: Systems like farming, resources, and interactables manage their own state.
- **Tool/Weapon Abstraction**: Tools use bridge classes so `PlayerController` stays clean and modular.

## Current Features

- **Tool System** — 8 equippable tools (Weapon, Shovel, Seed, Watering Can, Scythe, Axe, Pickaxe, Fishing Rod)
- **Farming** — Tilling, planting, watering, harvesting with day simulation
- **Resource Gathering** — Chopping trees and mining rocks with quality/rarity system
- **Inventory + UI** — Functional inventory grid with item rarity display
- **Economy** — Money system with buying/selling at the market
- **Swimming** — Full swimming mechanics when entering water areas
- **Fishing** — Equip fishing rod in water to catch fish
- **Blackjack** — Fully playable 3D Blackjack table with betting and money integration
- **TV System** — Watchable video screens
- **CCTV System** — Multiple security cameras with live feed + channel switching on monitors
- **Interaction System** — Contextual `F` key + left-click tool usage via raycast

## Architecture Overview

- `src/Core` — Pure C# logic (no Godot dependencies)
- `src/Bridge` — Godot nodes that connect logic to the scene tree
- `src/Resources` — Item definitions, scenes, and assets
- Strong emphasis on `Core/Bridge` separation and reusable systems

## Development Setup

- **Engine**: Godot 4.6+ (C# / .NET)
- **Version Control**: Git
- `.godot/` and `.import/` folders are ignored

## Roadmap

- [ ] Player-placable Security Cameras
- [ ] Vehicle system (drivable vehicles)
- [ ] Improved water visuals and swimming polish
- [ ] More advanced AI / enemy behavior
- [ ] Expanded economy and trading
