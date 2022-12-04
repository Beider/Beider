shader_type canvas_item;

// This shader requires you to set it's size to
// width = cellSize * mapData.width-2
// height = cellSize * mapData.width-2
// mapTilesCountX needs to be mapData.x - 2
// mapTilesCountY needs to be mapData.y - 2
// This is because we need a border on all sides for edge calculations
// Keep this in mind when positioning the image in your scene

// Size of our grid, please scale texture accordingly from outside
uniform float tileSizeInPixels = 64.0f;

// The cell size divided by 2
uniform float halfTileSizeInPixels = 32.0f;

// Size of textures in the atlas in pixels, assumed it is rectangular
// Should be a power of 2
uniform float textureAtlasTextureSizeInPixels = 1024;

// Amount of textures per row (max) in the texture atlas
// Should be 16384 / taTextureSize
uniform float textureAtlasTexturesWidth = 16;

// The width / height of the mapData in tiles. Set from outside
// Set it to 2 less in both width / height than the mapData texture
uniform float mapTilesCountX = 298;
uniform float mapTilesCountY = 298;

// The texture atlas
uniform sampler2D textureAtlas;

// The map data
// Currently only red channel is used to store the ID of the texture to use
// from the textureAtlas texture
uniform sampler2D mapData;

// Tile blend texture containing strength for blend
// R = horizontal, G = Vertical, B = Corner, A = Self
uniform sampler2D blendTexture;

// How many tiles x and y are there in the blend texture
// Has to be a size that lines up with taTexturesWidth.
// For instance if taTexturesWidth = 16, this could be 64,32,16,8,4,2,1
// Just make sure that the larger one of this and taTexturesWidth loops perfectly
// with chunk size of map. Else you will end up with artifacts
uniform float blendTextureTiles = 16;

// Should we blend
uniform bool blend = true;

//
// Get the tile the current pixel is in.
// This can be used to fetch the tileID from the mapData texture
//
// returns - The tile position in the current mapData
vec2 getTilePos(vec2 uv) {
	// Find the tile we are on (we got a border of 1 so +1)
	return vec2(floor(uv.x * mapTilesCountX)+1.0f, floor(uv.y * mapTilesCountY)+1.0f);
}

//
// Get our relative pixel position in the current tile
// Should be a number from 0 to tileSizeInPixels-1
// So for a tile size of 64 pixels this a number from 0 to 63
//
// returns - the pixel position within the current tile
vec2 getPixelPosInTile(vec2 uv) {
	float exactPosX = floor(((uv.x * mapTilesCountX)+1.0f) * tileSizeInPixels);
	float exactPosY = floor(((uv.y * mapTilesCountY)+1.0f) * tileSizeInPixels);
	float relativePosX = mod(exactPosX, tileSizeInPixels);
	float relativePosY = mod(exactPosY, tileSizeInPixels);
	return vec2(relativePosX, relativePosY);
}

//
// Get the id of the tile given the current tile positon
// [pos] - result of getTilePos
//
// returns - the ID of the tile from the mapData texture's red channel
float getTileId(vec2 pos) {
	float tileRed = texelFetch(mapData, ivec2(pos), 0).r;
	return tileRed * 255.;
}

//
// Get the pixel
// [pixelPosInTile] - result of getPixelPosInTile
// [tileId] - result of getTileId
// [tile] = result of getTilePos
//
// returns - The coordinate of the pixel to use from the textureAtlas
vec2 getAtlasPixelPos(vec2 pixelPosInTile, float tileId, vec2 tile) {
	// Find the row/column in the texture atlas to use
	// and calculate the pixel position
	float row = floor(tileId / textureAtlasTexturesWidth);
	float col = tileId - (row * textureAtlasTexturesWidth);
	vec2 textureStartPos = vec2(textureAtlasTextureSizeInPixels * col, 
								textureAtlasTextureSizeInPixels * row);

	// Find out how many tiles are within each texture atlas texture
	float tileRepeat = textureAtlasTextureSizeInPixels / tileSizeInPixels;
	
	// Mod our current position in the map with how many cells we got in
	// each atlas texutre. Then multiply this by tile size.
	// This gives us the correct part of the mega texture to use.
	float xAdd = mod(tile.x, tileRepeat) * tileSizeInPixels;
	float yAdd = mod(tile.y, tileRepeat) * tileSizeInPixels;
	textureStartPos += vec2(xAdd, yAdd);
	
	// Add the current position in the tile to where the current atlas
	// tile starts. This will give us the atlas pixel we want.
	return textureStartPos + pixelPosInTile;
}

//
// Get the color of the tile relative to the original tile
// [tile] - result of getTilePos
// [tileId] - result of getTileId
// [pixelPosInTile] - result of getPixelPosInTile
//
// returns - the color for the current pixel based on the tileId
vec4 getColorForCurrentPixel(vec2 tile, float tileId, vec2 pixelPosInTile) {
	vec2 pixelPosAtlas = getAtlasPixelPos(pixelPosInTile, tileId, tile);
	return (texture( textureAtlas, pixelPosAtlas / vec2(textureSize(textureAtlas,0)), -10));
}

void fragment() {
	// Get our tile position & pixel position in the tile
	vec2 tile = getTilePos(UV);
	vec2 pixelPosInTile = getPixelPosInTile(UV);
	
	// Grab the ID of the atlas texture to use and the color for this pixel
	float tileIdSelf = getTileId(tile);
	vec4 colorSelf = getColorForCurrentPixel(tile, tileIdSelf, pixelPosInTile);
	
	// Check if we should blend
	if (!blend) {
		// No blending, simply apply color
		COLOR = colorSelf;
	} else {
		// Do blending
		
		// Find closest edges
		// If these numbers are negative they are left / up
		// Could be 0 at center of tile but does not matter as we will just sample ourselves
		// and corner / sides should have no effect at center
		float horizontal = clamp((halfTileSizeInPixels * -1.0f) + pixelPosInTile.x, -1.0f, 1.0f);
		float vertical = clamp((halfTileSizeInPixels * -1.0f) + pixelPosInTile.y, -1.0f, 1.0f);
		// Corner pos is 0,0 - 0,64 - 64,0 - 64,64
		vec2 cornerPos = vec2(max(horizontal * tileSizeInPixels, 0.0f), 
							  max(vertical * tileSizeInPixels, 0.0f));
		
		
		// Grab the tile ID of the surrounding tiles we care about
		float tileIdHorizontal = getTileId(tile+vec2(horizontal,0));
		float tileIdVertical = getTileId(tile+vec2(0,vertical));
		float tileIdCorner = getTileId(tile+vec2(horizontal,vertical));
		
		// Grab for the current pixel with the neighbour tile IDs
		vec4 colorHorizontal = getColorForCurrentPixel(tile, tileIdHorizontal, pixelPosInTile);
		vec4 colorVertical = getColorForCurrentPixel(tile, tileIdVertical, pixelPosInTile);
		vec4 colorCorner = getColorForCurrentPixel(tile, tileIdCorner, pixelPosInTile);
		
		// Fetch the blend strength from our blend texture
		// First figure out which tile in the blend texture to use, the blend texture
		// simply repeats forever
		float modX = (mod(tile.x, blendTextureTiles) * tileSizeInPixels) + pixelPosInTile.x;
		float modY = (mod(tile.y, blendTextureTiles) * tileSizeInPixels) + pixelPosInTile.y;
		
		// Extract blend strength as a number from 0 to 255 and convert it to 0 to 1 value
		vec4 blendStrength = texelFetch(blendTexture, ivec2(vec2(modX, modY)), 0);
		float strHorizontal = (blendStrength.r * 255.) / 100.0f;
		float strVertical = (blendStrength.g * 255.) / 100.0f;
		float strCorner = (blendStrength.b * 255.) / 100.0f;
		float strSelf = (blendStrength.a * 255.) / 100.0f;
		
		// Blend based on percentages
		COLOR = (colorSelf * strSelf) + (colorHorizontal * strHorizontal) + 
				(colorVertical * strVertical) + (colorCorner * strCorner);
	}
}











