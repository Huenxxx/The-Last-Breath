# The Last Breath - Audio Asset Documentation

## Overview
This document tracks all audio assets used in The Last Breath project, including their sources, licenses, and current status (placeholder vs final).

## Audio Direction
- **Atmosphere**: Desolate, haunting, post-apocalyptic
- **Style**: Realistic with subtle ambient layers
- **Mood**: Tension, survival, isolation
- **Quality**: 44.1kHz, 16-bit minimum for final assets

## Audio Categories

### 1. Ambient Audio

#### Environmental Ambience
| Asset Name | Source | License | Status | Duration | Notes |
|------------|--------|---------|--------|----------|-------|
| Desert Wind Loop | *Searching Asset Store* | - | 🔄 Placeholder | 30-60s | Constant low wind, sand movement |
| Ruins Ambience | *Searching Asset Store* | - | 🔄 Placeholder | 30-60s | Creaking, distant echoes |
| Town Ambience | *Searching Asset Store* | - | 🔄 Placeholder | 30-60s | Subtle activity, wind through buildings |
| Night Ambience | *Searching Asset Store* | - | 🔄 Placeholder | 30-60s | Insects, distant sounds |

#### Weather & Atmosphere
| Asset Name | Source | License | Status | Duration | Notes |
|------------|--------|---------|--------|----------|-------|
| Sandstorm | *Pending* | - | 🔄 Placeholder | Variable | Intense wind, sand particles |
| Light Breeze | *Pending* | - | 🔄 Placeholder | Loop | Gentle wind variation |

### 2. Character Audio

#### Footsteps
| Asset Name | Source | License | Status | Variations | Notes |
|------------|--------|---------|--------|------------|-------|
| Sand Footsteps | *Searching Asset Store* | - | 🔄 Placeholder | 4-6 | Walking on sand/dirt |
| Stone Footsteps | *Searching Asset Store* | - | 🔄 Placeholder | 4-6 | Walking on stone/concrete |
| Metal Footsteps | *Searching Asset Store* | - | 🔄 Placeholder | 4-6 | Armored character steps |

#### Character Actions
| Asset Name | Source | License | Status | Variations | Notes |
|------------|--------|---------|--------|------------|-------|
| Breathing | *Pending* | - | 🔄 Placeholder | 3-4 | Idle, tired, wounded |
| Cloth Rustle | *Pending* | - | 🔄 Placeholder | 3-4 | Movement, equipment |
| Armor Clink | *Pending* | - | 🔄 Placeholder | 3-4 | Metal armor movement |

### 3. Combat Audio

#### Weapon Sounds
| Asset Name | Source | License | Status | Variations | Notes |
|------------|--------|---------|--------|------------|-------|
| Sword Swing | *Searching Asset Store* | - | 🔄 Placeholder | 3-4 | Whoosh sounds |
| Sword Hit | *Searching Asset Store* | - | 🔄 Placeholder | 4-6 | Metal on metal, flesh |
| Bow Draw | *Searching Asset Store* | - | 🔄 Placeholder | 2-3 | String tension |
| Arrow Release | *Searching Asset Store* | - | 🔄 Placeholder | 2-3 | String snap |
| Arrow Impact | *Searching Asset Store* | - | 🔄 Placeholder | 4-6 | Various materials |

#### Combat Feedback
| Asset Name | Source | License | Status | Variations | Notes |
|------------|--------|---------|--------|------------|-------|
| Block/Parry | *Pending* | - | 🔄 Placeholder | 3-4 | Weapon deflection |
| Critical Hit | *Pending* | - | 🔄 Placeholder | 2-3 | Enhanced impact |

### 4. Interaction Audio

#### Object Interactions
| Asset Name | Source | License | Status | Variations | Notes |
|------------|--------|---------|--------|------------|-------|
| Item Pickup | *Searching Asset Store* | - | 🔄 Placeholder | 2-3 | Small objects |
| Container Open | *Searching Asset Store* | - | 🔄 Placeholder | 2-3 | Chests, boxes |
| Door Open/Close | *Searching Asset Store* | - | 🔄 Placeholder | 2-3 | Creaky, worn doors |

#### Resource Gathering
| Asset Name | Source | License | Status | Variations | Notes |
|------------|--------|---------|--------|------------|-------|
| Mining Sounds | *Pending* | - | 🔄 Placeholder | 3-4 | Pickaxe on stone/metal |
| Wood Chopping | *Pending* | - | 🔄 Placeholder | 3-4 | Axe on wood |

### 5. UI Audio

#### Interface Sounds
| Asset Name | Source | License | Status | Variations | Notes |
|------------|--------|---------|--------|------------|-------|
| Button Click | *Searching Asset Store* | - | 🔄 Placeholder | 1-2 | Subtle, non-intrusive |
| Menu Open | *Searching Asset Store* | - | 🔄 Placeholder | 1 | Inventory, menus |
| Menu Close | *Searching Asset Store* | - | 🔄 Placeholder | 1 | Soft close sound |
| Error Sound | *Searching Asset Store* | - | 🔄 Placeholder | 1 | Invalid action feedback |
| Success Sound | *Searching Asset Store* | - | 🔄 Placeholder | 1 | Positive feedback |

### 6. System Audio

#### Game Events
| Asset Name | Source | License | Status | Variations | Notes |
|------------|--------|---------|--------|------------|-------|
| Save Game | *Pending* | - | 🔄 Placeholder | 1 | Confirmation sound |
| Level Up | *Pending* | - | 🔄 Placeholder | 1 | Character progression |
| Quest Complete | *Pending* | - | 🔄 Placeholder | 1 | Achievement sound |

## Audio Implementation

### Unity Audio System
- **Audio Source**: 3D positioned sources for world sounds
- **Audio Mixer**: Separate groups for SFX, Ambient, UI
- **Audio Listener**: Attached to main camera
- **Spatial Blend**: 3D for world sounds, 2D for UI

### Volume Categories
- **Master Volume**: Overall game volume
- **SFX Volume**: Sound effects and interactions
- **Ambient Volume**: Environmental and atmospheric sounds
- **UI Volume**: Interface and menu sounds

## Current Priorities (Prototype 1.5)

### High Priority
- [ ] Basic footstep sounds (sand, stone)
- [ ] Essential UI sounds (click, open, close)
- [ ] Desert wind ambient loop
- [ ] Basic weapon sounds (sword swing/hit)

### Medium Priority
- [ ] Character breathing and movement sounds
- [ ] Container and door interaction sounds
- [ ] Additional ambient variations
- [ ] Combat feedback sounds

### Low Priority
- [ ] Advanced weather sounds
- [ ] Resource gathering audio
- [ ] System event sounds
- [ ] Voice/vocal sounds

## Asset Store Search Results

### Ambient & Environment Audio
<mcreference link="https://assetstore.unity.com/packages/audio/sound-fx/wind-sound-171345" index="4">4</mcreference> **Wind Sound** ($15)
- 94 seamless loops of wind sounds
- Desert Wind: 20 loops (10 outside, 10 inside)
- Sand Storm: 20 loops (10 outside, 10 inside)
- Forest Wind: 30 loops (15 outside, 15 inside)
- Winter Wind & Blizzard: 24 loops total
- Loop length: 0.26-0.32 seconds

<mcreference link="https://assetstore.unity.com/packages/audio/ambient/noise/wind-sounds-317497" index="2">2</mcreference> **Wind Sounds** (€7.35)
- Collection of seamless wind sound loops
- 497.0 MB file size
- High-quality ambient wind effects

<mcreference link="https://assetstore.unity.com/packages/audio/music/the-last-post-apocalyptic-ambient-music-asset-pack-201838" index="5">5</mcreference> **The Last Post-apocalyptic/ambient Music Asset Pack** (FREE)
- 47 high-quality loopable music tracks
- 42 one-shot versions included
- Inspired by The Last of Us atmosphere
- Perfect for dark atmosphere games
- 121.0 MB file size

### Character & Combat Audio
<mcreference link="https://assetstore.unity.com/packages/audio/sound-fx/foley/footsteps-essentials-189879" index="5">5</mcreference> **Footsteps - Essentials** (FREE)
- 479 high-quality individual sounds
- 13 different surfaces: DirtyGround, Grass, Gravel, Leaves, Metal, Mud, Rock, Sand, Snow, Snow Hard, Tile, Water, Wood
- Mono, 48kHz / 24bits
- Recorded outside for authentic sound

<mcreference link="https://discussions.unity.com/t/horror-game-sound-pack-weapons-impact-footsteps-ambience-music/1526229" index="2">2</mcreference> **Ethereal Terror - Horror Game Sound Pack**
- Loopable ambience tracks for seamless backgrounds
- Original composed music for horror atmosphere
- Footstep sounds on multiple surfaces (carpet, concrete, grass, metal, tiles, wood)
- Hand-designed weapon sounds for impactful gameplay
- Professionally recorded foley sounds
- Demo scene with models, animations, and scripts

### UI & Interface Audio
<mcreference link="https://assetstore.unity.com/packages/audio/sound-fx/free-ui-click-sound-pack-244644" index="1">1</mcreference> **Free UI Click Sound Pack** (FREE)
- 100 sound effects for UI interactions
- Button, keyboard, and mouse clicks
- Toggle switches, menus, dropdowns
- Alerts and notifications
- Multiple styles: Liquid, Pop, Crispy, Organic, Mechanical, Sci-Fi, Metal, Wooden, Plastic
- 100% royalty-free

<mcreference link="https://assetstore.unity.com/packages/audio/sound-fx/button-click-sounds-pack-137076" index="2">2</mcreference> **Button Click Sounds Pack**
- 150 button click sounds
- Designed for UI and other buttons
- WAV 16-bit format

<mcreference link="https://assetstore.unity.com/packages/audio/sound-fx/game-menu-button-click-sounds-308241" index="3">3</mcreference> **Game Menu Button Click Sounds**
- Hi-Tech Button Click Sounds: 30 sounds (mechanical + digital blend)
- Mechanical Button Click Sounds: 40 sounds (pure mechanical clicks)
- Hover Popups: 31 sounds for mouse-over effects
- Compatible with Built-in, URP, and HDRP render pipelines

<mcreference link="https://assetstore.unity.com/packages/audio/sound-fx/ui-menu-sounds-56347" index="5">5</mcreference> **UI & Menu Sounds**
- 50 different sounds in two categories: Single and Multi
- Single sounds: played with single note
- Multi sounds: played with multiple notes
- Available in both .wav and .ogg formats

## Asset Store Search Keywords
- "desert ambient"
- "post apocalyptic audio"
- "survival game sounds"
- "medieval weapons audio"
- "UI sound pack"
- "footsteps collection"

## Technical Requirements

### File Formats
- **Preferred**: WAV (uncompressed)
- **Acceptable**: OGG Vorbis (compressed)
- **Avoid**: MP3 (licensing issues)

### Quality Standards
- **Sample Rate**: 44.1kHz minimum
- **Bit Depth**: 16-bit minimum
- **Channels**: Mono for most SFX, Stereo for ambient

### Optimization
- Compress ambient loops with OGG Vorbis
- Keep UI sounds small and uncompressed
- Use audio compression for mobile builds

## License Compliance
All audio assets must be compatible with:
- Commercial use
- Modification rights
- Distribution in builds
- No attribution required (preferred)

## Integration Checklist
For each new audio asset:
- [ ] Import at correct settings
- [ ] Test in-game volume levels
- [ ] Assign to appropriate Audio Mixer group
- [ ] Document source and license
- [ ] Test on different audio hardware
- [ ] Commit with detailed description

## Version History
- **v1.0** (Current) - Initial documentation structure
- Audio search and integration pending

## Notes
- All placeholder audio should be replaced before Alpha release
- Maintain consistent audio quality across all assets
- Regular audio mixing sessions needed for balance
- Consider procedural audio generation for repetitive sounds
- Test audio on various hardware (headphones, speakers, mobile)