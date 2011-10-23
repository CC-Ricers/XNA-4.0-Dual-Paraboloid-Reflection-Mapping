///////////////////////////////////////////////////////////////////////
//
// Cube environment mapping
//
// Paraboloid mapping derived from Jason Zink's gamedev paper
///////////////////////////////////////////////////////////////////////

float4x4 WorldViewProj;

float Direction;

texture EnvMap;

float NearPlane = .1f;
float FarPlane = 1000.0f;

sampler EnvTex = sampler_state
{
	Texture = <EnvMap>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU  = WRAP;
    AddressV  = WRAP;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 EnvTexC	: TEXCOORD0;
};

struct BuildDP_VSOut
{
	float4 Position : POSITION0;
    float3 EnvTexC	: TEXCOORD0;
    float z : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProj);
    
    output.EnvTexC = input.Position.xyz;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return texCUBE(EnvTex, input.EnvTexC);
}

BuildDP_VSOut BuildDP_VS(VertexShaderInput input)
{
	BuildDP_VSOut output;
	
	output.EnvTexC = input.Position.xyz;
	
	// Transform to homogeneous clip space.
	output.Position = mul(input.Position, WorldViewProj);
	
	output.Position.z = output.Position.z * Direction;
	
	float L = length( output.Position.xyz );						// determine the distance between (0,0,0) and the vertex
	output.Position = output.Position / L;							// divide the vertex position by the distance 
	
	output.z = output.Position.z;									// remember which hemisphere the vertex is in
	output.Position.z = output.Position.z + 1;						// add the reflected vector to find the normal vector

	output.Position.x = output.Position.x / output.Position.z;		// divide x coord by the new z-value
	output.Position.y = output.Position.y / output.Position.z;		// divide y coord by the new z-value

	output.Position.z = (L - NearPlane) / (FarPlane - NearPlane);	//	scale the depth to [0, 1]
	output.Position.w = 1;											// set w to 1 so there is no w divide
	
	return output;
}

float4 BuildDP_PS(BuildDP_VSOut input) : COLOR0
{
	clip(input.z);
	
    return texCUBE(EnvTex, input.EnvTexC);
}

technique EnvrionmentMap
{
    pass Pass1
    {		
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

technique BuildDP
{
    pass Pass1
    {		
        VertexShader = compile vs_2_0 BuildDP_VS();
        PixelShader = compile ps_2_0 BuildDP_PS();
		CullMode = None;
    }
}
