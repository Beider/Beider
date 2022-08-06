using Godot;
using System;
using System.Collections.Generic;

public class GameManager : Node
{
    public static GameManager Instance;

    public const int TEXTURE_SIZE = 1024;

    public const int TILE_SIZE = 64;

    /// <summary>
    /// Texture passed to tilemap shader containing all ground textures
    /// </summary>
    public ImageTexture MegaTexture { get; private set; } = null;

    /// <summary>
    /// Texture used in tilemap shader for tile blending 
    /// </summary>
    public Texture TileBlendTexture { get; private set; } = null;

    public NoiseTexture MapNoise;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
        CreateMegaTexture();
        CreateTileBlendTexture();
        InitMapNoise();
    }

    // Using a seamless noise map to let it go infinite
    private void InitMapNoise(int seed = 1337)
    {
        NoiseTexture noise = new NoiseTexture();
        noise.Seamless = true;
        noise.Width = 128;
        noise.Height = 128;
        OpenSimplexNoise oNoise = new OpenSimplexNoise();
        noise.Noise = oNoise;
        MapNoise = noise;
    }

    private List<Texture> GetGroundTextures()
    {
        List<Texture> textureList = new List<Texture>();
        textureList.Add(ResourceLoader.Load("res://Assets/Textures/free_water.png") as Texture);
        textureList.Add(ResourceLoader.Load("res://Assets/Textures/free_grass.png") as Texture);
        textureList.Add(ResourceLoader.Load("res://Assets/Textures/free_sand.png") as Texture);
        textureList.Add(ResourceLoader.Load("res://Assets/Textures/free_rock.png") as Texture);

        return textureList;
    }

    private NoiseTexture GetNoiseGen(int seed = 1337)
    {
        NoiseTexture noise = new NoiseTexture();
        noise.Seamless = true;
        noise.Width = 1024;
        noise.Height = 1024;
        OpenSimplexNoise oNoise = new OpenSimplexNoise();
        oNoise.Seed = 4;
        oNoise.Period = 56f;
        oNoise.Persistence = 0;
        oNoise.Lacunarity = 0.1f;
        noise.Noise = oNoise;
        return noise;
    }

    private void CreateTileBlendTexture(float smoothEdge = 24f, float smoothCorner = 16f,
                                        float edgePercentage = 50, float cornerPercentage = 25, int textureSize = 16, bool debug = true)
    {
        if (TileBlendTexture != null)
        {
            return;
        }
        // Noise (Same seed always to get same result always)
        NoiseTexture noiseX = GetNoiseGen(1234);
        NoiseTexture noiseY = GetNoiseGen(4321);
        NoiseTexture noiseC = GetNoiseGen(1243);

        //TileBlendTexture
        int halfTile = TILE_SIZE / 2;

        Image blendImage = new Image();
        blendImage.Create(TILE_SIZE * textureSize, TILE_SIZE * textureSize, false, Image.Format.Rgbaf);
        blendImage.Lock();

        Image testImageR = null;
        Image testImageG = null;
        Image testImageB = null;
        Image testImageA = null;

        if (debug)
        {
            testImageR = new Image();
            testImageR.Create(TILE_SIZE * textureSize, TILE_SIZE * textureSize, false, Image.Format.Rgbaf);
            testImageR.Lock();

            testImageG = new Image();
            testImageG.Create(TILE_SIZE * textureSize, TILE_SIZE * textureSize, false, Image.Format.Rgbaf);
            testImageG.Lock();

            testImageB = new Image();
            testImageB.Create(TILE_SIZE * textureSize, TILE_SIZE * textureSize, false, Image.Format.Rgbaf);
            testImageB.Lock();

            testImageA = new Image();
            testImageA.Create(TILE_SIZE * textureSize, TILE_SIZE * textureSize, false, Image.Format.Rgbaf);
            testImageA.Lock();
        }

        // Loop through all pixels in the image and create colors
        // We make 10 rows and columns
        for (int row = 0; row < textureSize; row++)
        {
            for (int column = 0; column < textureSize; column++)
            {
                for (int x = 0; x < TILE_SIZE; x++)
                {
                    for (int y = 0; y < TILE_SIZE; y++)
                    {
                        int imageXPos = x + (column * TILE_SIZE);
                        int imageYPos = y + (row * TILE_SIZE);

                        // Find closest edges
                        float xEdge = x < halfTile ? 0 : TILE_SIZE - 1;
                        float yEdge = y < halfTile ? 0 : TILE_SIZE - 1;

                        // Get distance to closest edges
                        float xDist = new Vector2(x, 0).DistanceTo(new Vector2(xEdge, 0));
                        float yDist = new Vector2(0, y).DistanceTo(new Vector2(0, yEdge));
                        float cornerDist = new Vector2(x, y).DistanceTo(new Vector2(xEdge, yEdge));

                        // Calculate smoothing percentages to all edges (rounded)
                        float xSmooth = Mathf.Round((1f - Mathf.SmoothStep(0, smoothEdge, Mathf.Min(xDist, smoothEdge))) * edgePercentage);
                        float ySmooth = Mathf.Round((1f - Mathf.SmoothStep(0, smoothEdge, Mathf.Min(yDist, smoothEdge))) * edgePercentage);
                        float cornerSmooth = Mathf.Round((1f - Mathf.SmoothStep(0, smoothCorner, Mathf.Min(cornerDist, smoothCorner))) * cornerPercentage);

                        // Add noise based on distance from edge
                        // Noise count more closer to the center
                        float noiseValX = noiseX.Noise.GetNoise2d(imageXPos, imageYPos) * ((xDist / smoothEdge));
                        float noiseValY = noiseY.Noise.GetNoise2d(imageXPos, imageYPos) * ((yDist / smoothEdge));
                        float noiseValC = noiseC.Noise.GetNoise2d(imageXPos, imageYPos) * ((cornerDist / smoothCorner));

                        xSmooth = Mathf.Min(xSmooth * (1f - noiseValX), edgePercentage);
                        ySmooth = Mathf.Min(ySmooth * (1f - noiseValY), edgePercentage);
                        cornerSmooth = Mathf.Min(cornerSmooth * (1f - noiseValC), cornerPercentage);

                        // Subtract corner input from both X / Y and clamp to 0
                        xSmooth = Mathf.Max(0f, xSmooth - cornerSmooth);
                        ySmooth = Mathf.Max(0f, ySmooth - cornerSmooth);

                        // Calculate remainder for main texture
                        float selfPercentage = 100 - xSmooth - ySmooth - cornerSmooth;

                        // Now we got a percentage that will add up to 100%
                        // xSmooth = effect of horizontal texture
                        // ySmooth = effect of vertical texture
                        // cornerSmooth = effect of corner texture
                        // selfPercentage = effect of primary texture
                        // Write to rgba
                        Color col = new Color(xSmooth / 255f, ySmooth / 255f, cornerSmooth / 255f, selfPercentage / 255f);
                        blendImage.SetPixel(imageXPos, imageYPos, col);


                        if (debug)
                        {
                            col = new Color(xSmooth / 255f, xSmooth / 255f, xSmooth / 255f, 1f);
                            testImageR.SetPixel(imageXPos, imageYPos, col);

                            col = new Color(ySmooth / 255f, ySmooth / 255f, ySmooth / 255f, 1f);
                            testImageG.SetPixel(imageXPos, imageYPos, col);

                            col = new Color(cornerSmooth / 255f, cornerSmooth / 255f, cornerSmooth / 255f, 1f);
                            testImageB.SetPixel(imageXPos, imageYPos, col);

                            col = new Color(selfPercentage / 255f, selfPercentage / 255f, selfPercentage / 255f, 1f);
                            testImageA.SetPixel(imageXPos, imageYPos, col);
                        }
                    }
                }
            }
        }

        blendImage.Unlock();

        // Create debug sprite
        TileBlendTexture = new ImageTexture();
        ((ImageTexture)TileBlendTexture).CreateFromImage(blendImage, (uint)Texture.FlagsEnum.Repeat);

        if (debug)
        {
            blendImage.SavePng("user://blendTest.png");

            testImageR.Unlock();
            testImageR.SavePng("user://blendTestR.png");

            testImageG.Unlock();
            testImageG.SavePng("user://blendTestG.png");

            testImageB.Unlock();
            testImageB.SavePng("user://blendTestB.png");

            testImageA.Unlock();
            testImageA.SavePng("user://blendTestA.png");
        }

    }

    /// <summary>
    /// Simple method to create a mega texture
    /// </summary>
    private void CreateMegaTexture()
    {
        // Hardcoded for now
        // a full 16384 x 16384 texture takes 768 mb of vram (roughly 3 mb per 1024x1024 texture)

        // Get tiles
        List<Texture> tileList = GetGroundTextures();
        float count = tileList.Count;

        // Calculate width / height of mega texture and create it
        int height = TEXTURE_SIZE * Mathf.CeilToInt(count / 16f);
        int width = TEXTURE_SIZE * (int)Mathf.Min(count, 16f);
        Image megaImg = new Image();
        megaImg.Create(width, height, false, Image.Format.Rgb8);
        megaImg.Lock();
        float posX = 0;
        float posY = 0;

        // Copy image data to mega texture
        foreach (Texture texture in tileList)
        {
            var img = texture.GetData();
            Vector2 targetPos = new Vector2(posX * TEXTURE_SIZE, posY * TEXTURE_SIZE);
            megaImg.BlendRect(img, new Rect2(Vector2.Zero, img.GetSize()), targetPos);

            // Ensure we only get 16 per row
            posX += 1;
            if (posX > 15)
            {
                posX = 0;
                posY++;
            }
        }

        megaImg.Unlock();

        // Create debug sprite
        MegaTexture = new ImageTexture();
        MegaTexture.CreateFromImage(megaImg, (uint)Texture.FlagsEnum.Mipmaps);
    }

}
