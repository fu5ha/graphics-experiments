texture MyTexture;
sampler mySampler = sampler_state {
	Texture = <MyTexture>;
};

float4x4 Model;
float4x4 View;
float4x4 Projection;

struct VertexPositionTexture {
	float4 position: POSITION;
	float4 uv: TEXCOORD;
};

VertexPositionTexture MyVertexShader(VertexPositionTexture vertex) {
	return mul(Projection, mul(View, mul(vertex, Model)));
}

float4 MyPixelShader(VertexPositionTexture vertex) : COLOR {
	return tex2D(mySampler, vertex.uv);
}

technique MyTechnique {
	pass Pass1 {
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MyPixelShader();
	}
}