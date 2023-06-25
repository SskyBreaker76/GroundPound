void RGBtoCMYK_float(float3 RGB, out float4 CMYK)
{
	float K = 1.0 - max(RGB.r, max(RGB.g, RGB.b));
	float C = (1.0 - RGB.r - K) / (1.0 - K);
	float M = (1.0 - RGB.g - K) / (1.0 - K);
	float Y = (1.0 - RGB.b - K) / (1.0 - K);
	CMYK = float4(saturate(C), saturate(M), saturate(Y), saturate(K));
}