# The Last Breath - Asset Documentation

## Overview
This document tracks all visual and audio assets used in The Last Breath project, including their sources, licenses, and current status (placeholder vs final).

## Art Style Direction
- **Style**: Dark, semi-realistic with worn, desolate tone (inspired by Kenshi but original)
- **Environment**: Barren deserts, ruined towns, fractured landscapes
- **Characters**: Humanoids with modular armor/clothing pieces (mix-and-match system)
- **Color Palette**: Muted earth tones, rust, weathered metals, faded fabrics

## Asset Categories

### 1. Environment Assets

#### Terrain & Landscape
| Asset Name | Source | License | Status | Notes |
|------------|--------|---------|--------|-------|
| Procedural Terrain | Unity Built-in | Unity License | ✅ Final | Generated via TerrainGenerator.cs |
| Desert Sand Texture | *Pending* | - | 🔄 Placeholder | Need high-res desert sand material |
| Rock Formations | *Pending* | - | 🔄 Placeholder | Various desert rock shapes needed |

#### Modular Building Kit
| Asset Name | Source | License | Status | Notes |
|------------|--------|---------|--------|-------|
| Wall Pieces | *Searching Asset Store* | - | 🔄 Placeholder | Modular walls for base construction |
| Floor Tiles | *Searching Asset Store* | - | 🔄 Placeholder | Various floor materials |
| Roof Sections | *Searching Asset Store* | - | 🔄 Placeholder | Damaged/weathered roofing |
| Doors & Windows | *Searching Asset Store* | - | 🔄 Placeholder | Broken/boarded variants |

#### Props & Decorations
| Asset Name | Source | License | Status | Notes |
|------------|--------|---------|--------|-------|
| Resource Nodes | *Pending* | - | 🔄 Placeholder | Iron ore, stone deposits, dead trees |
| Ruins & Debris | *Pending* | - | 🔄 Placeholder | Scattered building remains |
| Desert Vegetation | *Pending* | - | 🔄 Placeholder | Cacti, dead bushes, thorny plants |

### 2. Character Assets

#### Base Models
| Asset Name | Source | License | Status | Notes |
|------------|--------|---------|--------|-------|
| Humanoid Base | *Searching Asset Store* | - | 🔄 Placeholder | Unity Humanoid rig compatible |
| Player Character | Unity Capsule | Unity License | 🔄 Placeholder | Temporary capsule primitive |
| NPC Models | Unity Capsule | Unity License | 🔄 Placeholder | Temporary capsule primitives |

#### Armor & Clothing
| Asset Name | Source | License | Status | Notes |
|------------|--------|---------|--------|-------|
| Cloth Armor Set | *Searching Asset Store* | - | 🔄 Placeholder | Light, tattered clothing |
| Light Armor Set | *Searching Asset Store* | - | 🔄 Placeholder | Leather/scrap metal mix |
| Heavy Armor Set | *Searching Asset Store* | - | 🔄 Placeholder | Metal plates, worn condition |

### 3. Weapon Assets

#### Melee Weapons
| Asset Name | Source | License | Status | Notes |
|------------|--------|---------|--------|-------|
| Short Melee | *Searching Asset Store* | - | 🔄 Placeholder | Knife, short sword, club |
| Heavy Melee | *Searching Asset Store* | - | 🔄 Placeholder | Two-handed sword, axe, hammer |

#### Ranged Weapons
| Asset Name | Source | License | Status | Notes |
|------------|--------|---------|--------|-------|
| Bow/Crossbow | *Searching Asset Store* | - | 🔄 Placeholder | Makeshift ranged weapons |

### 4. UI Assets

#### Interface Elements
| Asset Name | Source | License | Status | Notes |
|------------|--------|---------|--------|-------|
| UI Skin | Unity Built-in | Unity License | 🔄 Placeholder | Default Unity GUI skin |
| Icons | *Pending* | - | 🔄 Placeholder | Inventory, status, action icons |

## Asset Pipeline Workflow

### 1. Asset Search Process
1. Search Unity Asset Store for suitable assets
2. Check license compatibility (free/paid)
3. Evaluate quality and style fit
4. Document findings in this file

### 2. Import Process
1. Import asset package
2. Test integration with existing systems
3. Document source and license
4. Commit changes with detailed message

### 3. Placeholder Creation
1. Create simple geometric placeholders when assets unavailable
2. Mark clearly as temporary
3. Document replacement requirements
4. Maintain functionality during development

## Current Priorities (Prototype 1.5)

### High Priority
- [ ] Modular building kit for base construction
- [ ] Character models with basic armor variations
- [ ] Basic weapon models (at least one per category)

### Medium Priority
- [ ] Environment props (rocks, ruins, vegetation)
- [ ] Additional armor/clothing variations
- [ ] UI icon set

### Low Priority
- [ ] Decorative props
- [ ] Advanced weapon variants
- [ ] Particle effects

## Asset Store Search Results

### Modular Building Systems
<mcreference link="https://assetstore.unity.com/packages/templates/systems/easy-build-system-modular-building-system-45394" index="1">1</mcreference> **Easy Build System - Modular Building System** ($45)
- Complete modular building system with snap-to-grid functionality
- Cross-platform support (Standalone, Mobile, WebGL, VR)
- Includes demo scenes and add-ons
- Compatible with Standard, URP, and HDRP render pipelines

<mcreference link="https://assetstore.unity.com/packages/templates/systems/ubuild-in-game-modular-building-system-75709" index="2">2</mcreference> **uBuild: In-game modular building system** ($25)
- Layer-based building system with furniture placement
- 19 example prefabs (walls, floors, stairs, doors)
- Auto-saving functionality
- Third-person and build mode switching

<mcreference link="https://assetstore.unity.com/packages/3d/environments/industrial/modular-building-set-40117" index="5">5</mcreference> **Modular Building Set** ($15)
- 50 industrial building parts
- Factory, warehouse, garage components
- Two window types, doors, roof pieces
- Includes demo scene and example prefabs

### Character Models
<mcreference link="https://assetstore.unity.com/packages/3d/characters/humanoids/post-apocalyptic-survival-character-127468/reviews" index="3">3</mcreference> **Post Apocalyptic Survival Character** ($9.19)
- High, medium, and low poly versions
- Humanoid rigged character
- Note: Some users report material issues (pink model)

<mcreference link="https://unityassetcollection.com/%D1%81ustomized-post-apocalyptic-%D1%81haracter-free-download/" index="5">5</mcreference> **Customized Post-Apocalyptic Character** (Free/Educational)
- 45 modular parts for customization
- 5 pants, 5 shoes, 5 body clothes, 5 backpacks
- 3 heads, 2 glasses, 4 hats, 12 masks
- Includes 8 weapons (guns, melee weapons)
- PBR textures (4096x4096)

### Weapon Packs
<mcreference link="https://assetstore.unity.com/packages/3d/props/weapons/bows-and-crossbows-medieval-weapons-pack-153495" index="1">1</mcreference> **Bows and CrossBows - Medieval Weapons Pack** ($20)
- 2 Long Bows, 2 Shortbows, 2 Crossbows
- Shooting animations and rigged bones
- Arrow and bolt with 2 LOD levels
- 2048 PBR materials

<mcreference link="https://assetstore.unity.com/packages/3d/props/weapons/dark-fantasy-weapons-weapons-weapon-medieval-weapons-fantasy-wea-276793" index="2">2</mcreference> **Dark Fantasy Weapons** ($25)
- 17 unique meshes including bow, crossbow, swords, axes
- VR support and blood vertex painting
- Ranged system works out of the box
- 1024 textures (4K available separately)

<mcreference link="https://assetstore.unity.com/packages/3d/props/weapons/post-apocalyptic-weapon-pack-198562" index="4">4</mcreference> **Post Apocalyptic Weapon Pack** ($23)
- Tin grenades, hydraulic melee weapons
- Pipe hammers, piston hammer, self-made gun
- Clean and bloody variations
- High quality PBR textures

<mcreference link="https://assetstore.unity.com/packages/3d/props/weapons/free-pack-of-medieval-weapons-136607" index="5">5</mcreference> **Free Pack of Medieval Weapons** (Free)
- One-handed and double-handed swords
- Single and double-sided axes
- Wooden bow with tension animation
- Arrow included

### Environment Packs
<mcreference link="https://assetstore.unity.com/packages/3d/props/post-apocalyptic-props-desert-maps-73860" index="1">1</mcreference> **Post Apocalyptic Props & Desert Maps** ($15)
- Alcohol machine, pump house, generator
- Wheels, diesel generator, flag
- Barrels, bricks, fireplaces, bottles, boxes
- PBR textures with various resolutions

<mcreference link="https://assetstore.unity.com/packages/3d/environments/desert-terrain-apocalyptic-wasteland-310912" index="2">2</mcreference> **Desert Terrain: Apocalyptic Wasteland** ($50)
- 76 modular assets (ruined structures, debris)
- Immersive VFX suite (thunder, fire, smoke)
- 5 HDRI skyboxes
- HDRP optimized (requires HDRP)

<mcreference link="https://unityassets4free.com/post-apocalyptic-town-town-modular-town-post-apocalypse-sci-fi-wasteland/" index="3">3</mcreference> **Post Apocalyptic Town** (Premium/Educational)
- Over 200 unique meshes
- Fully enterable buildings with interiors
- Rideable bike included
- Modular kit for creating towns

<mcreference link="https://syntystore.com/products/polygon-apocalypse-wasteland" index="4">4</mcreference> **POLYGON Apocalypse Wasteland** ($400)
- 1,600+ premium prefabs
- 23 unique vehicles and 23 characters
- Fully modular wasteland weaponry
- Vast wasteland demo scene

## Asset Store Search Keywords
- "modular building kit"
- "post-apocalyptic character"
- "medieval weapons pack"
- "desert environment"
- "survival game assets"
- "humanoid character models"
- "base building system"

## License Compliance
All assets must be compatible with:
- Commercial use
- Modification rights
- Distribution in builds

## Version History
- **v1.0** (Current) - Initial documentation structure
- Asset search and integration pending

## Notes
- All placeholder assets should be replaced before Alpha release
- Maintain consistent art style across all assets
- Document any custom modifications made to imported assets
- Regular reviews needed to ensure style coherence