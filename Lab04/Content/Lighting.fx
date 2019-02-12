float4x4 Model;
float4x4 View;
float4x4 Projection;
float4x4 ModelInverseTranspose;

float3 LightPosition;
float4 DiffuseColor;
float4 AmbientColor;
float AmbientIntensity;
float DiffuseIntensity;
float SpecularIntensity;
float Shininess;

float3 CameraPosition;

float LIGHT_STRENGTH = 200.0;

struct PhongVertexInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL;
};

struct PhongVertexOutput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float3 WorldPosition : POSITION1;
};

PhongVertexOutput PhongVS(in PhongVertexInput input)
{
	PhongVertexOutput output;
	float4 worldpos = mul(input.Position, Model);
	output.Position = mul(mul(worldpos, View), Projection);
	output.WorldPosition = worldpos.xyz;
	output.Normal = normalize(mul(input.Normal, ModelInverseTranspose).xyz);
	return output;
}

float4 PhongPS(PhongVertexOutput input) : COLOR
{
	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 N = normalize(input.Normal);
	float3 R = reflect(-L, N);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, N)) * oodist2 * LIGHT_STRENGTH;
	col += SpecularIntensity * pow(saturate(dot(R, V)), Shininess) * oodist2 * LIGHT_STRENGTH;

	return saturate(col);
}

technique Phong
{
	pass P0
	{
		VertexShader = compile vs_4_0 PhongVS();
		PixelShader = compile ps_4_0 PhongPS();
	}
};

struct GouradVertexInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL;
};

struct GouradVertexOutput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

GouradVertexOutput GouradVS(in GouradVertexInput input)
{
	GouradVertexOutput output;
	float4 worldpos = mul(input.Position, Model);
	output.Position = mul(mul(worldpos, View), Projection);

	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float3 LD = LightPosition - worldpos.xyz;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - worldpos.xyz);
	float3 N = normalize(mul(input.Normal, ModelInverseTranspose).xyz);
	float3 R = reflect(-L, N);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, N)) * oodist2 * LIGHT_STRENGTH;
	col += SpecularIntensity * pow(saturate(dot(R, V)), Shininess) * oodist2 * LIGHT_STRENGTH;

	output.Color = saturate(col);
	return output;
}

float4 GouradPS(GouradVertexOutput input) : COLOR
{
	return input.Color;
}


technique Gourad
{
	pass P0
	{
		VertexShader = compile vs_4_0 GouradVS();
		PixelShader = compile ps_4_0 GouradPS();
	}
};

struct ToonVertexInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL;
};

struct ToonVertexOutput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float3 WorldPosition : POSITION1;
};

ToonVertexOutput ToonVS(in ToonVertexInput input)
{
	PhongVertexOutput output;
	float4 worldpos = mul(input.Position, Model);
	output.Position = mul(mul(worldpos, View), Projection);
	output.WorldPosition = worldpos.xyz;
	output.Normal = normalize(mul(input.Normal, ModelInverseTranspose).xyz);
	return output;
}

float4 ToonPS(ToonVertexOutput input) : COLOR
{
	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 N = normalize(input.Normal);
	float3 R = reflect(-L, N);

	float D = saturate(dot(L, N)) * oodist2 * LIGHT_STRENGTH;
	float S = pow(saturate(dot(R, V)), Shininess) * oodist2 * LIGHT_STRENGTH;

	col += AmbientColor * AmbientIntensity;

	if (D > 0.7) {
		col += DiffuseIntensity * DiffuseColor;
	} else if (D > 0.15) {
		col += DiffuseIntensity * DiffuseColor * 0.22;
	}

	if (S > 0.45) {
		col = float4(1.0, 1.0, 1.0, 1.0);
	}

	return saturate(col);
}

technique Toon
{
	pass P0
	{
		VertexShader = compile vs_4_0 ToonVS();
		PixelShader = compile ps_4_0 ToonPS();
	}
};