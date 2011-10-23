///////////////////////////////////////////////////////////////////
//
// Simple texture mapping
//
// Paraboloid mapping derived from Jason Zink's gamedev paper
///////////////////////////////////////////////////////////////////

float4x4 World;
float4x4 WorldInvTrans;
float4x4 WorldViewProj;

float Direction;

texture  DiffuseTex;

float TexScale;

float NearPlane = .1f;
float FarPlane = 1000.0f;

sampler TexS = sampler_state
{
	Texture = <DiffuseTex>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = LINEAR;
	MaxAnisotropy = 4;
	AddressU  = WRAP;
    AddressV  = WRAP;
};
 
struct OutputVS
{
    float4 posH    : POSITION0;
    float3 normalW : TEXCOORD0;
    float2 tex0    : TEXCOORD1;
    float3 pos	   : TEXCOORD2;
};

struct OutputVS_DP
{
    float4 posH    : POSITION0;
    float3 normalW : TEXCOORD0;
    float2 tex0    : TEXCOORD1;
    float z		   : TEXCOORD2;
};

////////////////////////////////////////////////////////////////////////////////////////////
//
// Phong Shading
//
////////////////////////////////////////////////////////////////////////////////////////////

OutputVS TextureVS(float3 posL : POSITION0, float3 normalL : NORMAL0, float2 tex0: TEXCOORD0)
{
    // Zero out our output.
	OutputVS outVS = (OutputVS)0;
	
	// Transform to homogeneous clip space.
	outVS.posH = mul(float4(posL, 1.0f), WorldViewProj);
	
	outVS.pos = mul(float4(posL, 1.0f), World).xyz;
	
	// Pass on texture coordinates to be interpolated in rasterization.
	outVS.tex0 = tex0;

	// Done--return the output.
    return outVS;
}

float4 TexturePS(float3 normalW : TEXCOORD0, float2 tex0 : TEXCOORD1, float3 pos : TEXCOORD2) : COLOR
{
	return tex2D(TexS, tex0 * TexScale);	
}

////////////////////////////////////////////////////////////////////////////////////////////
//
// Texture + Build the Dual-Paraboloid map
//
////////////////////////////////////////////////////////////////////////////////////////////
OutputVS_DP BuildDP_VS(float3 posL : POSITION0, float3 normalL : NORMAL0, float2 tex0: TEXCOORD0)
{
    // Zero out our output.
	OutputVS_DP outVS = (OutputVS_DP)0;
	
	// Pass on texture coordinates to be interpolated in rasterization.
	outVS.tex0 = tex0;
	
	//Render with the Dual-Paraboloid distortion
	
	// Transform to homogeneous clip space.
	outVS.posH = mul(float4(posL, 1.0f), WorldViewProj);
	
	outVS.posH.z = outVS.posH.z * Direction;
	
	float L = length( outVS.posH.xyz );						// determine the distance between (0,0,0) and the vertex
	outVS.posH = outVS.posH / L;							// divide the vertex position by the distance 
	
	outVS.z = outVS.posH.z;									// remember which hemisphere the vertex is in
	outVS.posH.z = outVS.posH.z + 1;						// add the reflected vector to find the normal vector

	outVS.posH.x = outVS.posH.x / outVS.posH.z;				// divide x coord by the new z-value
	outVS.posH.y = outVS.posH.y / outVS.posH.z;				// divide y coord by the new z-value

	outVS.posH.z = (L - NearPlane) / (FarPlane - NearPlane);// scale the depth to [0, 1]
	outVS.posH.w = 1;										// set w to 1 so there is no w divide

	// Done--return the output.
    return outVS;
}

float4 BuildDP_PS(float3 normalW : TEXCOORD0, float2 tex0 : TEXCOORD1, float z : TEXCOORD2) : COLOR
{
	clip(z);
	
	return tex2D(TexS, tex0 * TexScale);
}

technique Texture
{
    pass P0
    {
        // Specify the vertex and pixel shader associated with this pass.
        vertexShader = compile vs_2_0 TextureVS();
        pixelShader  = compile ps_2_0 TexturePS();

        //FillMode = Wireframe;
    }
}

technique BuildDP
{
	pass P0
	{
		vertexShader = compile vs_2_0 BuildDP_VS();
        pixelShader  = compile ps_2_0 BuildDP_PS();
        
        //FillMode = Wireframe;
	}
}