float4x4 World;
float4x4 View;
float4x4 Projection;

texture2D Texture;

sampler ParticleSampler :register(s0) = sampler_state {
	Texture = <Texture>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
};

struct VertexShaderInput {
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float4 ParticlePosition : POSITION1;
	float4 ParticleParameter : POSITION2;
};

struct VertexShaderOutput {
	float4 Position : POSITION0;
	float2 TexCoord :  TEXCOORD0;
	float4 Color : COLOR0;
};

VertexShaderOutput ParticleVS(VertexShaderInput input)
{
	VertexShaderOutput output;
	float scale = sqrt(input.ParticleParameter.x);
	float4 scale_vec = float4(scale, scale, 1.0, 1.0);
	float4 viewPosition = mul(mul(float4(0.0, 0.0, 0.0, 1.0), World), View) + float4(input.Position.xy, 0.0, 0.0) * scale_vec;
	output.Position = mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = 1 - input.ParticleParameter.x / input.ParticleParameter.y;
	return output;
}

float4 ParticlePS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(ParticleSampler, input.TexCoord);
	color *= input.Color;
	return color;
}

technique Skybox {
	pass Pass1 {
		VertexShader = compile vs_4_0 ParticleVS();
		PixelShader = compile ps_4_0 ParticlePS();
	}
};
