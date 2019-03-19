
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float4x4 LightViewMatrix;
float4x4 LightProjectionMatrix;
float3 CameraPosition;
float3 LightPosition;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal	: TEXCOORD0;
	float2 TexCoord : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
};

VertexShaderOutput VSFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPos = mul(input.Position, World);
	float4 viewPosition = mul(worldPos, LightViewMatrix);
	output.Position = mul(viewPosition, LightProjectionMatrix);
	output.WorldPosition = worldPos.xyz;
	output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);  
	output.TexCoord = input.TexCoord;
	return output;
}

float4 PSFunction(VertexShaderOutput input) : COLOR0
{
	float4 projTexCoord = mul(mul(float4(input.WorldPosition, 1.0), LightViewMatrix), LightProjectionMatrix);
	projTexCoord = projTexCoord / projTexCoord.w;
	projTexCoord.y *= -1.0;
	projTexCoord.xy = projTexCoord.xy * 0.5 + 0.5;
	float depth = 1.0 - projTexCoord.z;
	float4 color = saturate(depth);
	return color;
}
technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VSFunction();
		PixelShader = compile ps_4_0 PSFunction();
	}
}
