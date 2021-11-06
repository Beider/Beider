shader_type canvas_item;

uniform vec4 color : hint_color = vec4(1.0);
uniform float width : hint_range(0, 10) = 1.0;
uniform int pattern : hint_range(0, 2) = 0; // diamond, circle, square
uniform bool inside = false;
uniform bool add_margins = true; // only useful when inside is false
uniform float alpha : hint_range(0, 1) = 0.5;

uniform float line_interval_x = 0.016666666;
uniform float line_interval_y = 0.0296296296;
uniform bool enable_build_grid = true;
uniform vec2 grid_offset = vec2(0.0, 0.0);
varying vec2 world_pos;

void vertex() {
	world_pos = (PROJECTION_MATRIX * vec4(VERTEX, 1.0, 1.0)).xy;
	if (add_margins) {
		VERTEX += (UV * 2.0 - 1.0) * width;
	}
}

bool hasContraryNeighbour(vec2 uv, vec2 texture_pixel_size, sampler2D texture) {
	for (float i = -ceil(width); i <= ceil(width); i++) {
		float x = abs(i) > width ? width * sign(i) : i;
		float offset;
		
		if (pattern == 0) {
			offset = width - abs(x);
		} else if (pattern == 1) {
			offset = floor(sqrt(pow(width + 0.5, 2) - x * x));
		} else if (pattern == 2) {
			offset = width;
		}
		
		for (float j = -ceil(offset); j <= ceil(offset); j++) {
			float y = abs(j) > offset ? offset * sign(j) : j;
			vec2 xy = uv + texture_pixel_size * vec2(x, y);
			
			if ((xy != clamp(xy, vec2(0.0), vec2(1.0)) || texture(texture, xy).a == 0.0) == inside) {
				return true;
			}
		}
	}
	
	return false;
}

vec4 getPixelColor(vec2 uv, vec4 pixelColor)
{
	if (enable_build_grid)
	{
		if (mod(world_pos.x + grid_offset.x, line_interval_x) <= 0.001)
		{
			return vec4(0.0, 0.0, 0.0, 0.5);
		}
		else if (mod(world_pos.y + grid_offset.y, line_interval_y) <= 0.002)
		{
			return vec4(0.0, 0.0, 0.0, 0.5);
		}
	}
	
	return vec4(pixelColor.r, pixelColor.g, pixelColor.b, alpha);
}

void fragment() {
	vec2 uv = UV;
	
	if (add_margins) {
		vec2 texture_pixel_size = vec2(1.0) / (vec2(1.0) / TEXTURE_PIXEL_SIZE + vec2(width * 2.0));
		
		uv = (uv - texture_pixel_size * width) * TEXTURE_PIXEL_SIZE / texture_pixel_size;
		
		if (uv != clamp(uv, vec2(0.0), vec2(1.0))) {
			COLOR.a = 0.0;
		} else {
			COLOR = texture(TEXTURE, uv);
		}
	} else {
		COLOR = texture(TEXTURE, uv);
	}
	
	if ((COLOR.a > 0.0) == inside && hasContraryNeighbour(uv, TEXTURE_PIXEL_SIZE, TEXTURE)) {
		COLOR.rgb = inside ? mix(COLOR.rgb, color.rgb, color.a) : color.rgb;
		COLOR.a += (1.0 - COLOR.a) * color.a;
	}
	else
	{
		if (COLOR.a > 0.1)
		{
			COLOR = getPixelColor(uv, COLOR);
		}
	}
}