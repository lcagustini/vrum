float intensity(float4 color){
	return sqrt((color.x*color.x)+(color.y*color.y)+(color.z*color.z));
}

void Sobel_float(UnityTexture2D Texture, UnitySamplerState Sampler, float StepX, float StepY, float2 Center, out float4 Color) {
	// get samples around pixel
	float tleft = intensity(SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(-StepX, StepY)));
	float left = intensity(SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(-StepX, 0)));
	float bleft = intensity(SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(-StepX, -StepY)));
	float top = intensity(SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(0, StepY)));
	float bottom = intensity(SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(0, -StepY)));
	float tright = intensity(SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(StepX, StepY)));
	float right = intensity(SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(StepX, 0)));
	float bright = intensity(SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(StepX, -StepY)));

	// Sobel masks (see http://en.wikipedia.org/wiki/Sobel_operator)
	//        1 0 -1     -1 -2 -1
	//    X = 2 0 -2  Y = 0  0  0
	//        1 0 -1      1  2  1

	// You could also use Scharr operator:
	//        3 0 -3        3 10   3
	//    X = 10 0 -10  Y = 0  0   0
	//        3 0 -3        -3 -10 -3

#if 1
	float x = tleft + 2.0*left + bleft - tright - 2.0*right - bright;
	float y = -tleft - 2.0*top - tright + bleft + 2.0 * bottom + bright;
#else
	float x = 3.0*tleft + 10.0*left + 3.0*bleft - 3.0*tright - 10.0*right - 3.0*bright;
	float y = -3.0*tleft - 10.0*top - 3.0*tright + 3.0*bleft + 10.0 * bottom + 3.0*bright;
#endif
	float color = sqrt((x*x) + (y*y));
	Color = float4(color, color, color, 1);
}

void Blur_float(UnityTexture2D Texture, UnitySamplerState Sampler, float StepX, float StepY, float2 Center, out float4 Color) {
	// get samples around pixel
	float4 tleft = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(-StepX, StepY));
	float4 left = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(-StepX, 0));
	float4 bleft = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(-StepX, -StepY));
	float4 top = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(0, StepY));
	float4 center = SAMPLE_TEXTURE2D(Texture, Sampler, Center);
	float4 bottom = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(0, -StepY));
	float4 tright = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(StepX, StepY));
	float4 right = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(StepX, 0));
	float4 bright = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(StepX, -StepY));

	float4 color = 0;
	color += 1.0 * tleft + 2.0 * left + 1.0 * bleft;
	color += 2.0 * top + 4.0 * center + 2.0 * bottom;
	color += 1.0 * tright + 2.0 * right + 1.0 * bright;

	Color = color / 16.0;
}

void Sharpen_float(UnityTexture2D Texture, UnitySamplerState Sampler, float StepX, float StepY, float2 Center, out float4 Color) {
	// get samples around pixel
	float4 left = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(-StepX, 0));
	float4 top = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(0, StepY));
	float4 center = SAMPLE_TEXTURE2D(Texture, Sampler, Center);
	float4 bottom = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(0, -StepY));
	float4 right = SAMPLE_TEXTURE2D(Texture, Sampler, Center + float2(StepX, 0));

	float4 color = 0;
	color += -1.0 * left;
	color += -1.0 * top + 5.0 * center - 1.0 * bottom;
	color += -1.0 * right;

	Color = color;
}
