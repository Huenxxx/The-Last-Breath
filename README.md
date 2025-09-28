# The Last Breath

A single-player, open-world, sandbox survival/RTS game inspired by emergent systems and squad management mechanics.

## Overview

The Last Breath is a survival RTS game featuring:
- Emergent NPC simulation with needs-based AI
- Squad-based character management
- Base building and resource management
- Limb-based combat system
- Procedural world generation
- Optional permadeath mechanics

## Current Status: Prototype 0

### Implemented Features
- [ ] Procedural terrain generation
- [ ] RTS-style camera system
- [ ] Player character controller
- [ ] NPC AI and wandering behavior
- [ ] Unit selection system (up to 3 units)
- [ ] Click-to-move pathfinding
- [ ] Basic interaction system
- [ ] Save/load functionality

## Project Structure

```
/Assets
  /Art          - Visual assets and materials
  /Audio        - Sound effects and music
  /Prefabs      - Unity prefab objects
  /Scenes       - Unity scene files
  /Scripts      - C# source code
    /Core       - Core game systems
    /Systems    - Modular game systems
    /Characters - Character-related scripts
    /AI         - AI behavior and decision making
    /UI         - User interface scripts
  /Editor       - Unity editor tools
/Docs           - Documentation and design documents
/Builds         - Compiled game builds
```

## Development Setup

### Requirements
- Unity 2022.3 LTS or later
- Visual Studio 2022 or VS Code
- Git for version control

### Getting Started
1. Clone the repository
2. Open the project in Unity
3. Load the main scene from `Assets/Scenes/`
4. Press Play to test current functionality

## Architecture

### Core Systems
- **World System**: Procedural terrain and POI generation
- **AI System**: NPC behavior and needs simulation
- **Combat System**: Limb-based damage and combat mechanics
- **Inventory System**: Grid-based item management
- **Base System**: Building placement and worker assignment
- **UI System**: User interface and HUD management

### Design Principles
- Data-driven design using ScriptableObjects
- Modular system architecture
- Event-driven communication between systems
- Emergent gameplay over scripted content

## Milestones

### Prototype 0 (Current)
- Basic terrain and camera
- Player and NPC characters
- Unit selection and movement
- Save/load system

### Prototype 1 (Next)
- Inventory and item system
- Combat mechanics
- Basic crafting
- Debug tools and overlays

### Prototype 2
- Advanced AI with needs system
- Faction relationships
- Trade mechanics

### Alpha
- Base building system
- Worker assignment
- Performance optimization
- 30+ concurrent NPCs

## Contributing

This is currently a solo development project. For questions or suggestions, please create an issue.

## License

All rights reserved. This project is for educational and portfolio purposes.

---

**Note**: This game draws inspiration from Kenshi's mechanics but uses completely original assets, lore, and implementation. No Kenshi assets or intellectual property are used in this project.