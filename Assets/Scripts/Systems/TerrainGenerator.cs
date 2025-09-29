using UnityEngine;

namespace TheLastBreath.Systems
{
    /// <summary>
    /// Generates procedural terrain using Perlin noise for heightmaps
    /// </summary>
    public class TerrainGenerator : MonoBehaviour
    {
        [Header("Terrain Settings")]
        [SerializeField] private int terrainWidth = 256;
        [SerializeField] private int terrainHeight = 256;
        [SerializeField] private int terrainDepth = 20;
        [SerializeField] private float detailScale = 25.0f;
        
        [Header("Noise Settings")]
        [SerializeField] private float noiseScale = 0.01f;
        [SerializeField] private int octaves = 4;
        [SerializeField] private float persistence = 0.5f;
        [SerializeField] private float lacunarity = 2.0f;
        [SerializeField] private Vector2 offset = Vector2.zero;
        
        [Header("Materials")]
        [SerializeField] private Material terrainMaterial;
        
        // Seed for random generation
        [Header("Generation Settings")]
        [SerializeField] private int seed = 0;
        
        private Terrain terrain;
        private TerrainData terrainData;
        
        // Public properties for SaveLoadSystem
        public int Seed => seed;
        public Vector2 TerrainSize => new Vector2(terrainWidth, terrainHeight);
        
        // Public methods for SaveLoadSystem
        public void SetSeed(int newSeed)
        {
            seed = newSeed;
            Random.InitState(seed);
        }
        
        public void SetTerrainSize(Vector2 size)
        {
            terrainWidth = (int)size.x;
            terrainHeight = (int)size.y;
        }
        
        /// <summary>
        /// Initialize and generate the terrain on start
        /// </summary>
        void Start()
        {
            GenerateTerrain();
        }
        
        /// <summary>
        /// Generates the complete terrain with heightmap and textures
        /// </summary>
        public void GenerateTerrain()
        {
            CreateTerrainData();
            GenerateHeightmap();
            ApplyTerrainTextures();
            CreateTerrainGameObject();
        }
        
        /// <summary>
        /// Creates the terrain data object with specified dimensions
        /// </summary>
        private void CreateTerrainData()
        {
            terrainData = new TerrainData();
            terrainData.heightmapResolution = terrainWidth + 1;
            terrainData.size = new Vector3(terrainWidth, terrainDepth, terrainHeight);
        }
        
        /// <summary>
        /// Generates heightmap using multi-octave Perlin noise
        /// </summary>
        private void GenerateHeightmap()
        {
            float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
            
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                for (int y = 0; y < terrainData.heightmapResolution; y++)
                {
                    heights[x, y] = CalculateHeight(x, y);
                }
            }
            
            terrainData.SetHeights(0, 0, heights);
        }
        
        /// <summary>
        /// Calculates height at given coordinates using fractal noise
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Normalized height value</returns>
        private float CalculateHeight(int x, int y)
        {
            float height = 0;
            float amplitude = 1;
            float frequency = noiseScale;
            
            for (int i = 0; i < octaves; i++)
            {
                float sampleX = (x + offset.x) * frequency;
                float sampleY = (y + offset.y) * frequency;
                
                float noiseValue = Mathf.PerlinNoise(sampleX, sampleY);
                height += noiseValue * amplitude;
                
                amplitude *= persistence;
                frequency *= lacunarity;
            }
            
            return Mathf.Clamp01(height);
        }
        
        /// <summary>
        /// Applies basic texture to the terrain
        /// </summary>
        private void ApplyTerrainTextures()
        {
            // Create a simple texture array for now
            // This will be expanded later with multiple terrain types
            TerrainLayer[] terrainLayers = new TerrainLayer[1];
            terrainLayers[0] = new TerrainLayer();
            
            // Create a simple grass texture procedurally if no material is assigned
            if (terrainMaterial == null)
            {
                terrainLayers[0].diffuseTexture = CreateSimpleTexture(Color.green, 64, 64);
            }
            else
            {
                terrainLayers[0].diffuseTexture = terrainMaterial.mainTexture as Texture2D;
            }
            
            terrainLayers[0].tileSize = new Vector2(detailScale, detailScale);
            terrainData.terrainLayers = terrainLayers;
        }
        
        /// <summary>
        /// Creates a simple solid color texture
        /// </summary>
        /// <param name="color">Color of the texture</param>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <returns>Generated texture</returns>
        private Texture2D CreateSimpleTexture(Color color, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        /// <summary>
        /// Creates the terrain GameObject and assigns the terrain data
        /// </summary>
        private void CreateTerrainGameObject()
        {
            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrain = terrainObject.GetComponent<Terrain>();
            
            // Position the terrain at the origin
            terrainObject.transform.position = Vector3.zero;
            terrainObject.name = "Generated Terrain";
            
            // Add terrain collider for physics interactions
            TerrainCollider terrainCollider = terrainObject.GetComponent<TerrainCollider>();
            if (terrainCollider != null)
            {
                terrainCollider.terrainData = terrainData;
            }
        }
        
        /// <summary>
        /// Regenerates the terrain with current settings (for editor use)
        /// </summary>
        [ContextMenu("Regenerate Terrain")]
        public void RegenerateTerrain()
        {
            if (terrain != null)
            {
                DestroyImmediate(terrain.gameObject);
            }
            GenerateTerrain();
        }
        
        /// <summary>
        /// Gets the terrain height at world position
        /// </summary>
        /// <param name="worldPosition">World position to sample</param>
        /// <returns>Terrain height at position</returns>
        public float GetHeightAtPosition(Vector3 worldPosition)
        {
            if (terrain == null) return 0f;
            
            return terrain.SampleHeight(worldPosition);
        }
    }
}