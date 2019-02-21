float4x4 Model;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

float3 LightPosition;
float3 AmbientColor;
float AmbientIntensity;
float DiffuseIntensity;
float SpecularIntensity;
float Shininess;

texture NormalMap;

sampler TSampler = sampler_state {
	texture = <NormalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

float LIGHT_STRENGTH = 200.0;

struct PhongVertexInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float4 Tangent: TANGENT0;
	float2 TexCoord : TEXCOORD0;
};

struct PhongVertexOutput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT0;
	float3 WorldPosition : POSITION1;
	float2 TexCoord : TEXCOORD0;
};

PhongVertexOutput PhongVS(in PhongVertexInput input)
{
	PhongVertexOutput output;
	float4 worldpos = mul(input.Position, Model);
	output.Position = mul(mul(worldpos, View), Projection);
	output.WorldPosition = worldpos.xyz;
	output.Normal = normalize(mul(float4(input.Normal.xyz, 0), Model).xyz);
	output.Tangent = normalize(mul(float4(input.Tangent.xyz, 0), Model).xyz);
	output.TexCoord = input.TexCoord;
	return output;
}

float4 PhongPS(PhongVertexOutput input) : COLOR
{
	float3 col = float3(0.0, 0.0, 0.0);

	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = cross(N, T);

	float3x3 TBN = float3x3(T, B, N);

	float3 n_tex = tex2D(TSampler, input.TexCoord).rgb;

	n_tex = (2.0 * n_tex) - 1.0;

	N = normalize(mul(n_tex, TBN));

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * saturate(dot(L, N)) * oodist2 * LIGHT_STRENGTH;
	col += SpecularIntensity * pow(saturate(dot(H, N)), Shininess) * oodist2 * LIGHT_STRENGTH;

	return saturate(float4(col, 1.0));
}

technique Phong
{
	pass P0
	{
		VertexShader = compile vs_4_0 PhongVS();
		PixelShader = compile ps_4_0 PhongPS();
	}
};