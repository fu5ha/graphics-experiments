float4x4 Model;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 LightDirection;
float4 DiffuseColor;
float4 AmbientColor;
float AmbientIntensity;
float DiffuseIntensity;
float SpecularIntensity;

float3 camerapos;

struct VertexInput {
	float4 Position: POSITION;
	float4 Normal: NORMAL;
};

struct VertexOutput {
	float4 pos: POSITION0;
	float3 norm: NORMAL;
	float3 worldpos: POSITION1;
};

VertexOutput MyVertexShader(VertexInput vertex) {
	VertexOutput output;
	float4 worldpos = mul(vertex.Position, Model);
	output.pos = mul(mul(worldpos, View), Projection);
	output.worldpos = worldpos.xyz;
	output.norm = normalize(mul(vertex.Normal, WorldInverseTranspose).xyz);
	return output;
}

float4 MyPixelShader(VertexOutput vertex) : COLOR {
	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float3 L = normalize(LightDirection);
	float3 V = normalize(camerapos - vertex.worldpos);
	float3 N = vertex.norm;

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, N));
	col += SpecularIntensity * pow(saturate(dot(reflect(-L, N), V)), 10.0);

	return saturate(col);
}

technique MyTechnique {
	pass Pass1 {
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MyPixelShader();
	}
}