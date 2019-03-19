float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float4x4 LightViewMatrix;
float4x4 LightProjectionMatrix;
float3 CameraPosition;
float3 LightPosition;

texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
	Texture = <ShadowMap>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = none;
	AddressU = border;
	AddressV = border;
};

struct StandardVertexInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct StandardVertexOutput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float3 WorldPosition : POSITION1;
	float2 TexCoord : TEXCOORD0;
};

StandardVertexOutput StandardVS(in StandardVertexInput input)
{
	StandardVertexOutput output;
	float4 worldpos = mul(input.Position, World);
	output.Position = mul(mul(worldpos, View), Projection);
	output.WorldPosition = worldpos.xyz;
	output.Normal = normalize(mul(float4(input.Normal.xyz, 0), WorldInverseTranspose).xyz);
	output.TexCoord = input.TexCoord;
	return output;
}

float4 BlinnMappedStandardPS(in StandardVertexOutput input) : COLOR
{
	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float4 projTexCoord = mul(mul(float4(input.WorldPosition, 1.0), LightViewMatrix), LightProjectionMatrix);
	projTexCoord = projTexCoord / projTexCoord.w;
	projTexCoord.y *= -1.0;
	projTexCoord.xy = projTexCoord.xy * 0.5 + 0.5;
	float depth = 1.0 - projTexCoord.z;
	if (projTexCoord.x >= 0 && projTexCoord.x <= 1 &&
		projTexCoord.y >= 0 && projTexCoord.y <= 1 &&
		saturate(projTexCoord).x == projTexCoord.x &&
		saturate(projTexCoord).y == projTexCoord.y)
	{

		float shadowmapDepth = tex2D(ShadowMapSampler, projTexCoord.xy).r;

		if (depth + (1.0 / 100.0) > shadowmapDepth) {
			float3 LD = LightPosition - input.WorldPosition;
			float oodist = 1.0 / dot(LD, LD);

			float3 N = normalize(input.Normal);
			float3 L = normalize(LD);
			float3 V = normalize(CameraPosition - input.WorldPosition);
			float3 H = normalize(L + V);

			col += saturate(dot(L, N)) * oodist * 50;
			col += pow(saturate(dot(H, N)), 4 * 100.0) * oodist * 50;
		}
		else {
			col.r = 1.0;
		}
	}

	return saturate(col);
}

technique BlinnMappedStandard
{
	pass P0
	{
		VertexShader = compile vs_4_0 StandardVS();
		PixelShader = compile ps_4_0 BlinnMappedStandardPS();
	}
};
