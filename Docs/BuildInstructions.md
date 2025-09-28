# Build Instructions - The Last Breath

## Windows Build Generation

### Prerequisites
- Unity 2022.3 LTS or later
- Windows 10/11 development environment
- Git for version control

### Building from Unity Editor

1. **Open the Project**
   - Launch Unity Hub
   - Click "Open" and navigate to the project folder
   - Select the project root directory containing `Assets` folder

2. **Verify Scene Setup**
   - Ensure `Assets/Scenes/Prototype0.unity` is the active scene
   - Check that GameManager is present in the scene hierarchy

3. **Build Settings**
   - Go to `File > Build Settings`
   - Ensure "PC, Mac & Linux Standalone" is selected
   - Set Target Platform to "Windows"
   - Set Architecture to "x86_64"
   - Add `Prototype0.unity` to "Scenes In Build" if not already present

4. **Player Settings**
   - Click "Player Settings" in Build Settings window
   - Set Company Name: "The Last Breath Team"
   - Set Product Name: "The Last Breath - Prototype 0"
   - Set Version: "0.1.0"
   - Set Default Icon (optional)

5. **Generate Build**
   - Click "Build" in Build Settings
   - Choose destination folder (recommended: `Builds/Windows/Prototype0/`)
   - Wait for build completion

### Command Line Build (Advanced)

For automated builds, you can use Unity's command line interface:

```bash
# Navigate to Unity installation directory
cd "C:\Program Files\Unity\Hub\Editor\2022.3.x\Editor"

# Execute build command
Unity.exe -batchmode -quit -projectPath "C:\Users\xx\Unity Games\The-Last-Breath" -buildWindows64Player "C:\Users\xx\Unity Games\The-Last-Breath\Builds\Windows\TheLastBreath.exe"
```

### Build Output

The build will generate:
- `TheLastBreath.exe` - Main executable
- `TheLastBreath_Data/` - Game data folder
- `UnityCrashHandler64.exe` - Crash handler
- `UnityPlayer.dll` - Unity runtime

### Testing the Build

1. **Launch the Game**
   - Double-click `TheLastBreath.exe`
   - The game should start in windowed mode

2. **Verify Core Features**
   - Terrain should generate automatically
   - Camera controls should work (WASD, mouse)
   - Player character (blue capsule) should be visible
   - 2 NPC characters (red capsules) should be wandering
   - 5 loot items (yellow cubes) should be scattered
   - Unit selection should work (click and drag)
   - Click-to-move should function
   - Interaction should work (E key near loot items)
   - Save/Load should work (F5/F9 keys)

3. **Debug Information**
   - Press F1 to toggle debug overlay
   - Debug panel shows FPS, selected units, and controls

### Known Issues

- NavMesh may not be baked automatically in builds
- Some materials may appear pink if shaders are missing
- Audio is not implemented in Prototype 0

### Troubleshooting

**Build Fails:**
- Check Unity console for errors
- Ensure all scripts compile without errors
- Verify all required packages are installed

**Game Doesn't Start:**
- Check Windows Event Viewer for crash logs
- Ensure DirectX and Visual C++ redistributables are installed
- Try running as administrator

**Missing Features:**
- Ensure all scripts are properly assigned in the scene
- Check that GameManager has autoInitialize enabled
- Verify NavMesh is baked for the terrain

### Distribution

For distribution, zip the entire build folder:
1. Select all files in the build output directory
2. Create ZIP archive named `TheLastBreath-Prototype0-Windows.zip`
3. Include this README in the archive root

### Next Steps

After successful build and testing:
1. Create GitHub release with build artifacts
2. Generate demo video/GIF showing features
3. Document any issues found during testing
4. Plan next milestone features

## Build Checklist

- [ ] Project opens without errors
- [ ] All scripts compile successfully
- [ ] Prototype0 scene loads correctly
- [ ] Build completes without errors
- [ ] Executable launches successfully
- [ ] All core features work as expected
- [ ] Debug overlay functions properly
- [ ] Save/Load system works
- [ ] Performance is acceptable (>30 FPS)
- [ ] Build is packaged for distribution