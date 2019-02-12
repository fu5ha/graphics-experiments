texture MyTexture;
sampler mySampler = sampler_state {
	Texture = <MyTexture>;
};

float4x4 Model;
float4x4 View;
float4x4 Projection;

float3 camerapos;

float3 lightpos = float3(0.0, 1.0, 0.75);
float3 N = float3(0.0, 0.0, 1.0);

struct VertexPositionTexture {
	float4 position: POSITION;
	float4 uv: TEXCOORD;
};

struct VertexOut {
	float4 position: POSITION0;
	float4 uv: TEXCOORD;
	float3 worldpos: POSITION1;
};

VertexOut MyVertexShader(VertexPositionTexture vertex) {
	//vertex.position = mul(mul(mul(vertex.position, Model), View), Projection);
	VertexOut output;
	float4 worldpos = mul(vertex.position, Model);
	output.position = mul(mul(worldpos, View), Projection);
	output.worldpos = worldpos.xyz;
	output.uv = vertex.uv;
	return output;
}

float4 MyPixelShader(VertexOut vertex) : COLOR {
	float4 samp_col = tex2D(mySampler, vertex.uv);

	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float3 L = lightpos - vertex.worldpos;
	float3 V = camerapos - vertex.worldpos;

	float oodist2 = 1.0 / dot(L, L);

	col += 0.8 * samp_col * saturate(dot(L, N)) * oodist2;
	col += 0.2 * pow(saturate(dot(reflect(-L, N), V)), 5.0) * oodist2;

	return col;
}

technique MyTechnique {
	pass Pass1 {
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MyPixelShader();
	}
}