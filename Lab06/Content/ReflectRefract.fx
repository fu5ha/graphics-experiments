float4x4 Model;
float4x4 View;
float4x4 Projection;
Texture SkyboxTexture;
Texture HelicopterTexture;

float3 CameraPosition;

float iorRatio;

sampler HelicopterSampler = sampler_state
{
	texture = <HelicopterTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

samplerCUBE SkyboxSampler = sampler_state
{
	texture = <SkyboxTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexInput
{
	float4 Position : POSITION;
	float4 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexOutput {
	float4 Position : POSITION;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float3 WorldPosition: TEXCOORD1;
};

VertexOutput ReflectVS(VertexInput input) {
	VertexOutput output;
	float4 worldpos = mul(input.Position, Model);
	output.Position = mul(mul(worldpos, View), Projection);
	output.WorldPosition = worldpos.xyz;
	output.Normal = mul(input.Normal.xyz, Model).xyz;
	output.TexCoord = input.TexCoord;
	return output;
}

float4 ReflectPS(VertexOutput input) : COLOR {
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 N = normalize(input.Normal);
	float3 R = reflect(-V, N);
	float3 col = texCUBE(SkyboxSampler, R).rgb;
	float3 texCol = tex2D(HelicopterSampler, input.TexCoord).rgb;
	return float4(lerp(col, texCol, 0.25), 1.0);
}

technique Reflect {
	pass Pass1 {
		VertexShader = compile vs_4_0 ReflectVS();
		PixelShader = compile ps_4_0 ReflectPS();
	}
};

VertexOutput RefractVS(VertexInput input) {
	VertexOutput output;
	float4 worldpos = mul(input.Position, Model);
	output.Position = mul(mul(worldpos, View), Projection);
	output.WorldPosition = worldpos.xyz;
	output.Normal = mul(input.Normal.xyz, Model).xyz;
	output.TexCoord = input.TexCoord;
	return output;
}

float4 RefractPS(VertexOutput input) : COLOR{
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 N = normalize(input.Normal);
	float3 T = refract(-V, N, iorRatio);
	float3 col = texCUBE(SkyboxSampler, T).rgb;
	float3 texCol = tex2D(HelicopterSampler, input.TexCoord).rgb;
	return float4(lerp(col, texCol, 0.1), 1.0);
}

technique Refract {
	pass Pass1 {
		VertexShader = compile vs_4_0 RefractVS();
		PixelShader = compile ps_4_0 RefractPS();
	}
};

VertexOutput FresnelVS(VertexInput input) {
	VertexOutput output;
	float4 worldpos = mul(input.Position, Model);
	output.Position = mul(mul(worldpos, View), Projection);
	output.WorldPosition = worldpos.xyz;
	output.Normal = mul(input.Normal.xyz, Model).xyz;
	output.TexCoord = input.TexCoord;
	return output;
}


float3 FresnelSchlick(float3 F0, float3 V, float3 N)
{
	return F0 + (1.0f - F0) * pow(1.0f - saturate(dot(V, N)), 5);
}

float4 FresnelPS(VertexOutput input) : COLOR{
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 I = -V;
	float3 N = normalize(input.Normal);
	float3 fresnel = FresnelSchlick(float3(0.08, 0.08, 0.08), V, N);
	float3 R = reflect(I, N);
	float3 T = refract(I, N, iorRatio);
	float3 RCol = texCUBE(SkyboxSampler, R).rgb;
	float3 TCol = texCUBE(SkyboxSampler, T).rgb;
	float3 fresCol = lerp(TCol, RCol, fresnel);
	float3 decalCol = tex2D(HelicopterSampler, input.TexCoord).rgb;
	float3 col = lerp(fresCol, decalCol, 0.1);
	return float4(col, 1.0);
}


technique Fresnel {
	pass Pass1 {
		VertexShader = compile vs_4_0 FresnelVS();
		PixelShader = compile ps_4_0 FresnelPS();
	}
};