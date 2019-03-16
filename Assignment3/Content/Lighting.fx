float4x4 Model;
float4x4 View;
float4x4 Projection;
float4x4 ModelInverseTranspose;

float3 LightPosition;
float LightStrength;
float3 LightColor;
float4 DiffuseColor;
float4 AmbientColor;
float AmbientIntensity;
float DiffuseIntensity;
float SpecularIntensity;
float Shininess;
float EtaRatio;
int UseMipmap;

float3 CameraPosition;

float3 UvwScale;

texture NormalMap;
texture SkyboxTexture;

sampler TSampler = sampler_state {
	texture = <NormalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler TSamplerNoMip = sampler_state {
	texture = <NormalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = None;
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

// Technique order:
// 1. Raw normalmap on the object
// 2. World space normals visualization
// 3. Tangent space bump mapped Blinn
// 4. Tangent space bump mapped reflection
// 5. Tangent space bump mapped refraction
// 6. Tangent space bump mapped Blinn, NOT normalized tangent frame, normalized map sample
// 7. Tangent space bump mapped Blinn, NOT normalized tangent frame, NOT normalized map sample
// 8. Tangent space bump mapped Blinn, normalized tangent frame, NOT normalized map sample
// 9. Tangent space bump mapped Blinn, normalized tangent frame, NOT normalized map sample for diffuse, normlized map sample for specular

struct StandardVertexInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float4 Tangent : TANGENT0;
	float2 TexCoord : TEXCOORD0;
};

struct StandardVertexOutput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT0;
	float3 WorldPosition : POSITION1;
	float2 TexCoord : TEXCOORD0;
};

StandardVertexOutput StandardVS(in StandardVertexInput input)
{
	StandardVertexOutput output;
	float4 worldpos = mul(input.Position, Model);
	output.Position = mul(mul(worldpos, View), Projection);
	output.WorldPosition = worldpos.xyz;
	output.Normal = normalize(mul(float4(input.Normal.xyz, 0), Model).xyz);
	output.Tangent = normalize(mul(float4(input.Tangent.xyz, 0), Model).xyz);
	output.TexCoord = input.TexCoord;
	return output;
}

float4 RawNormalPS(in StandardVertexOutput input) : COLOR
{
	float3 n_tex;
	if (UseMipmap) {
		n_tex = tex2D(TSampler, input.TexCoord * UvwScale.xy).rgb;
	}
	else {
		n_tex = tex2D(TSamplerNoMip, input.TexCoord * UvwScale.xy).rgb;
	}
	n_tex = lerp(float3(0.5, 0.5, 1), n_tex, UvwScale.z);
	return float4(n_tex, 1.0);
}

technique RawNormal
{
	pass P0
	{
		VertexShader = compile vs_4_0 StandardVS();
		PixelShader = compile ps_4_0 RawNormalPS();
	}
};

float4 WorldNormalPS(in StandardVertexOutput input) : COLOR
{
	float3 n_tex;
	if (UseMipmap) {
		n_tex = tex2D(TSampler, input.TexCoord * UvwScale.xy).rgb;
	}
	else {
		n_tex = tex2D(TSamplerNoMip, input.TexCoord * UvwScale.xy).rgb;
	}
	n_tex = lerp(float3(0.5, 0.5, 1), n_tex, UvwScale.z);

	n_tex = (n_tex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 world_norm = normalize(mul(normalize(n_tex), TBN));

	return float4((world_norm / 2.0) + 0.5, 1.0);
}

technique WorldNormal
{
	pass P0
	{
		VertexShader = compile vs_4_0 StandardVS();
		PixelShader = compile ps_4_0 WorldNormalPS();
	}
};

float4 BlinnMappedStandardPS(in StandardVertexOutput input) : COLOR
{
	float3 n_tex;
	if (UseMipmap) {
		n_tex = tex2D(TSampler, input.TexCoord * UvwScale.xy).rgb;
	}
	else {
		n_tex = tex2D(TSamplerNoMip, input.TexCoord * UvwScale.xy).rgb;
	}
	n_tex = lerp(float3(0.5, 0.5, 1), n_tex, UvwScale.z);

	n_tex = (n_tex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 world_norm = normalize(mul(normalize(n_tex), TBN));

	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, world_norm)) * oodist2 * LightStrength * float4(LightColor, 1.0);
	col += SpecularIntensity * pow(saturate(dot(H, world_norm)), 4 * Shininess) * oodist2 * LightStrength * float4(LightColor, 1.0);

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

float4 ReflectPS(in StandardVertexOutput input) : COLOR
{
	float3 n_tex;
	if (UseMipmap) {
		n_tex = tex2D(TSampler, input.TexCoord * UvwScale.xy).rgb;
	}
	else {
		n_tex = tex2D(TSamplerNoMip, input.TexCoord * UvwScale.xy).rgb;
	}
	n_tex = lerp(float3(0.5, 0.5, 1), n_tex, UvwScale.z);

	n_tex = (n_tex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 world_norm = normalize(mul(n_tex, TBN));

	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 R = reflect(-V, world_norm);
	float3 col = texCUBE(SkyboxSampler, R).rgb;

	return float4(col, 1.0);
}

technique Reflect
{
	pass P0
	{
		VertexShader = compile vs_4_0 StandardVS();
		PixelShader = compile ps_4_0 ReflectPS();
	}
};

float4 RefractPS(in StandardVertexOutput input) : COLOR
{
	float3 n_tex;
	if (UseMipmap) {
		n_tex = tex2D(TSampler, input.TexCoord * UvwScale.xy).rgb;
	}
	else {
		n_tex = tex2D(TSamplerNoMip, input.TexCoord * UvwScale.xy).rgb;
	}
	n_tex = lerp(float3(0.5, 0.5, 1), n_tex, UvwScale.z);

	n_tex = (n_tex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 world_norm = normalize(mul(n_tex, TBN));

	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 R = refract(-V, world_norm, EtaRatio);
	float3 col = texCUBE(SkyboxSampler, R).rgb;

	return float4(col, 1.0);
}

technique Refract
{
	pass P0
	{
		VertexShader = compile vs_4_0 StandardVS();
		PixelShader = compile ps_4_0 RefractPS();
	}
};

float4 BlinnMappedNoNormalizeTangentFramePS(in StandardVertexOutput input) : COLOR
{
	float3 n_tex;
	if (UseMipmap) {
		n_tex = tex2D(TSampler, input.TexCoord * UvwScale.xy).rgb;
	}
	else {
		n_tex = tex2D(TSamplerNoMip, input.TexCoord * UvwScale.xy).rgb;
	}
	n_tex = lerp(float3(0.5, 0.5, 1), n_tex, UvwScale.z);

	n_tex = (n_tex - 0.5) * 2.0;

	float3 N = input.Normal;
	float3 T = input.Tangent;
	float3 B = cross(N, T);
	float3x3 TBN = float3x3(T, B, N);

	float3 world_norm = mul(normalize(n_tex), TBN);

	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, world_norm)) * oodist2 * LightStrength * float4(LightColor, 1.0);
	col += SpecularIntensity * pow(saturate(dot(H, world_norm)), 4 * Shininess) * oodist2 * LightStrength * float4(LightColor, 1.0);

	return saturate(col);
}

technique BlinnMappedNoNormalizeTangentFrame
{
	pass P0
	{
		VertexShader = compile vs_4_0 StandardVS();
		PixelShader = compile ps_4_0 BlinnMappedNoNormalizeTangentFramePS();
	}
};

float4 BlinnMappedNoNormalizeTangentFrameNoNormalizeSamplePS(in StandardVertexOutput input) : COLOR
{
	float3 n_tex;
	if (UseMipmap) {
		n_tex = tex2D(TSampler, input.TexCoord * UvwScale.xy).rgb;
	}
	else {
		n_tex = tex2D(TSamplerNoMip, input.TexCoord * UvwScale.xy).rgb;
	}
	n_tex = lerp(float3(0.5, 0.5, 1), n_tex, UvwScale.z);

	n_tex = (n_tex - 0.5) * 2.0;

	float3 N = input.Normal;
	float3 T = input.Tangent;
	float3 B = cross(N, T);
	float3x3 TBN = float3x3(T, B, N);

	float3 world_norm = mul(n_tex, TBN);

	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, world_norm)) * oodist2 * LightStrength * float4(LightColor, 1.0);
	col += SpecularIntensity * pow(saturate(dot(H, world_norm)), 4 * Shininess) * oodist2 * LightStrength * float4(LightColor, 1.0);

	return saturate(col);
}

technique BlinnMappedNoNormalizeTangentFrameNoNormalizeSamplePS
{
	pass P0
	{
		VertexShader = compile vs_4_0 StandardVS();
		PixelShader = compile ps_4_0 BlinnMappedNoNormalizeTangentFrameNoNormalizeSamplePS();
	}
};

float4 BlinnMappedNormalizeTangentNoNormalizeSamplePS(in StandardVertexOutput input) : COLOR
{
	float3 n_tex;
	if (UseMipmap) {
		n_tex = tex2D(TSampler, input.TexCoord * UvwScale.xy).rgb;
	}
	else {
		n_tex = tex2D(TSamplerNoMip, input.TexCoord * UvwScale.xy).rgb;
	}
	n_tex = lerp(float3(0.5, 0.5, 1), n_tex, UvwScale.z);

	n_tex = (n_tex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 world_norm = normalize(mul(n_tex, TBN));

	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, world_norm)) * oodist2 * LightStrength * float4(LightColor, 1.0);
	col += SpecularIntensity * pow(saturate(dot(H, world_norm)), 4 * Shininess) * oodist2 * LightStrength * float4(LightColor, 1.0);

	return saturate(col);
}

technique BlinnMappedNormalizeTangentNoNormalizeSample
{
	pass P0
	{
		VertexShader = compile vs_4_0 StandardVS();
		PixelShader = compile ps_4_0 BlinnMappedNormalizeTangentNoNormalizeSamplePS();
	}
};

float4 BlinnMappedNormalizeTangentNormalizeSamplePS(in StandardVertexOutput input) : COLOR
{
	float3 n_tex;
	if (UseMipmap) {
		n_tex = tex2D(TSampler, input.TexCoord * UvwScale.xy).rgb;
	}
	else {
		n_tex = tex2D(TSamplerNoMip, input.TexCoord * UvwScale.xy).rgb;
	}
	n_tex = lerp(float3(0.5, 0.5, 1), n_tex, UvwScale.z);

	n_tex = (n_tex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 world_norm = normalize(mul(normalize(n_tex), TBN));

	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, world_norm)) * oodist2 * LightStrength * float4(LightColor, 1.0);
	col += SpecularIntensity * pow(saturate(dot(H, world_norm)), 4 * Shininess) * oodist2 * LightStrength * float4(LightColor, 1.0);

	return saturate(col);
}

technique BlinnMappedNormalizeTangentNoNormalizeSample
{
	pass P0
	{
		VertexShader = compile vs_4_0 StandardVS();
		PixelShader = compile ps_4_0 BlinnMappedNormalizeTangentNormalizeSamplePS();
	}
};

struct FullscreenVertexOutput
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD;
};

FullscreenVertexOutput FullscreenVS(uint id: SV_VertexID)
{
	FullscreenVertexOutput output;

	output.TexCoord = float2((id << 1) & 2, id & 2);
	output.Position = float4(output.TexCoord * float2(2, -2) + float2(-1, 1), 0, 1);

	return output;
}

float4 FullscreenPS(in FullscreenVertexOutput input) : COLOR
{
	float3 n_tex = tex2D(TSamplerNoMip, input.TexCoord).rgb;
	return float4(n_tex, 1.0);
}

technique Fullscreen
{
	pass P0
	{
		VertexShader = compile vs_4_0 FullscreenVS();
		PixelShader = compile ps_4_0 FullscreenPS();
	}
};

