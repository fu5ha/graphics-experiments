float4x4 Model;
float4x4 View;
float4x4 Projection;
Texture SkyboxTexture;
Texture HelicopterTexture;

float3 CameraPosition;

float EtaRatio;
float3 DispersionEtaRatio;
float TextureMixFactor;

float Reflectivity;

float FresBias;
float FresScale;
float FresPower;

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
	output.Normal = mul(float4(input.Normal.xyz, 0.0), Model).xyz;
	output.TexCoord = input.TexCoord;
	return output;
}

float4 ReflectPS(VertexOutput input) : COLOR {
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 N = normalize(input.Normal);
	float3 R = reflect(-V, N);
	float3 col = texCUBE(SkyboxSampler, R).rgb;
	float3 texCol = tex2D(HelicopterSampler, input.TexCoord).rgb;
	return float4(Reflectivity * lerp(col, texCol, TextureMixFactor), 1.0);
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
	output.Normal = mul(float4(input.Normal.xyz, 0.0), Model).xyz;
	output.TexCoord = input.TexCoord;
	return output;
}

float4 RefractPS(VertexOutput input) : COLOR{
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 N = normalize(input.Normal);
	float3 T = refract(-V, N, EtaRatio);
	float3 col = texCUBE(SkyboxSampler, T).rgb;
	float3 texCol = tex2D(HelicopterSampler, input.TexCoord).rgb;
	return float4(lerp(col, texCol, TextureMixFactor), 1.0);
}

technique Refract {
	pass Pass1 {
		VertexShader = compile vs_4_0 RefractVS();
		PixelShader = compile ps_4_0 RefractPS();
	}
};

float4 DispRefractPS(VertexOutput input) : COLOR{
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 N = normalize(input.Normal);
	float3 TR = refract(-V, N, DispersionEtaRatio.r);
	float3 TG = refract(-V, N, DispersionEtaRatio.g);
	float3 TB = refract(-V, N, DispersionEtaRatio.b);
	float3 col;
	col.r = texCUBE(SkyboxSampler, TR).r;
	col.g = texCUBE(SkyboxSampler, TG).g;
	col.b = texCUBE(SkyboxSampler, TB).b;
	float3 texCol = tex2D(HelicopterSampler, input.TexCoord).rgb;
	return float4(lerp(col, texCol, TextureMixFactor), 1.0);
}

technique DispRefract {
	pass Pass1 {
		VertexShader = compile vs_4_0 RefractVS();
		PixelShader = compile ps_4_0 DispRefractPS();
	}
};

VertexOutput FresnelVS(VertexInput input) {
	VertexOutput output;
	float4 worldpos = mul(input.Position, Model);
	output.Position = mul(mul(worldpos, View), Projection);
	output.WorldPosition = worldpos.xyz;
	output.Normal = mul(float4(input.Normal.xyz, 0.0), Model).xyz;
	output.TexCoord = input.TexCoord;
	return output;
}


float3 Fresnel(float3 V, float3 N)
{
	return FresBias + FresScale * pow(1.0f - saturate(dot(V, N)), FresPower);
}

float4 FresnelPS(VertexOutput input) : COLOR{
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 I = -V;
	float3 N = normalize(input.Normal);
	float3 fresnel = Fresnel(V, N);
	float3 R = reflect(I, N);
	float3 TR = refract(-V, N, DispersionEtaRatio.r);
	float3 TG = refract(-V, N, DispersionEtaRatio.g);
	float3 TB = refract(-V, N, DispersionEtaRatio.b);
	float3 TCol;
	TCol.r = texCUBE(SkyboxSampler, TR).r;
	TCol.g = texCUBE(SkyboxSampler, TG).g;
	TCol.b = texCUBE(SkyboxSampler, TB).b;
	float3 RCol = texCUBE(SkyboxSampler, R).rgb;
	float3 fresCol = lerp(TCol, RCol, fresnel);
	float3 decalCol = tex2D(HelicopterSampler, input.TexCoord).rgb;
	float3 col = lerp(fresCol, decalCol, TextureMixFactor);
	return float4(col, 1.0);
}


technique Fresnel {
	pass Pass1 {
		VertexShader = compile vs_4_0 FresnelVS();
		PixelShader = compile ps_4_0 FresnelPS();
	}
};