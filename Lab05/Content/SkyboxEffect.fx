float4x4 Model;
float4x4 View;
float4x4 Projection;
Texture SkyboxTexture;

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
	float4 Position : POSITION0;
};

struct VertexOutput {
	float4 Position : POSITION0;
	float3 TexCoord : TEXCOORD0;
};

VertexOutput VS(VertexInput input) {
	VertexOutput output;
	float4x4 view = View;
	view[0][3] = 0;
	view[1][3] = 0;
	view[2][3] = 0;
	output.TexCoord = input.Position.xyz;
	output.Position = mul(mul(mul(input.Position, Model), view), Projection);
	return output;
}

float4 PS(VertexOutput input) : COLOR{
	return texCUBE(SkyboxSampler, normalize(input.TexCoord));
}

technique Skybox {
	pass Pass1 {
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
	}
};