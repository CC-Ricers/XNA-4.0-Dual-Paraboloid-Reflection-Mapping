//////////////////////////////////////////////////////////////
//
// Phong shading
//
// Paraboloid mapping derived from Jason Zink's gamedev paper
//////////////////////////////////////////////////////////////

float4x4 World;
float4x4 WorldInvTrans;
float4x4 WorldViewProj;

float Direction;

float4	LightDiffuse0;
float4	LightAmbient0;
float3	LightDir0;

float4	LightDiffuse1;
float4	LightAmbient1;
float3	LightDir1;

float4	LightDiffuse2;
float4	LightAmbient2;
float3	LightDir2;

texture  DiffuseTex;
float TexScale;

float FarPlane = 1000.0f;
float NearPlane = .1f;

sampler TexS = sampler_state
{
	Texture = <DiffuseTex>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = LINEAR;
	MaxAnisotropy = 8;
	AddressU  = WRAP;
    AddressV  = WRAP;
};
 
struct OutputVS
{
    float4 posH    : POSITION0;
    float3 normalW : TEXCOORD0;
    float2 tex0    : TEXCOORD1;
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
OutputVS PhongVS(float3 posL : POSITION0, float3 normalL : NORMAL0, float2 tex0: TEXCOORD0)
{
    // Zero out our output.
	OutputVS outVS;// = (OutputVS)0;
	
	// Transform to homogeneous clip space.
	outVS.posH = mul(float4(posL, 1.0f), WorldViewProj);
	
	// Transform normal to world space.
	outVS.normalW = mul(float4(normalL, 0.0f), WorldInvTrans).xyz;	
	
	// Pass on texture coordinates to be interpolated in rasterization.
	outVS.tex0 = tex0;

	// Done--return the output.
    return outVS;
}

float4 PhongPS(float3 normalW : TEXCOORD0, float2 tex0 : TEXCOORD1) : COLOR
{
	// Interpolated normals can become unnormal--so normalize.
	normalW = normalize(normalW);
	
	// Light vector is opposite the direction of the light.
	float3 lightVecW0 = -LightDir0;
	float3 lightVecW1 = -LightDir1;
	float3 lightVecW2 = -LightDir2;
	
	// Determine the diffuse light intensity that strikes the vertex.
	float s0 = saturate(dot(lightVecW0, normalW));
	float s1 = saturate(dot(lightVecW1, normalW));
	float s2 = saturate(dot(lightVecW2, normalW));
	
	// Compute the ambient, diffuse and specular terms separatly. 
	float3 diffuse0 = s0 * LightDiffuse0.rgb;
	float3 diffuse1 = s1 * LightDiffuse1.rgb;
	float3 diffuse2 = s2 * LightDiffuse2.rgb;
	
	// Get the texture color.
	float4 texColor = tex2D(TexS, tex0 * TexScale);
	
	// Combine the color from lighting with the texture color.
	float3 color = (diffuse0 + diffuse1 + diffuse2 + LightAmbient0) * texColor.rgb;
		
	// Sum all the terms together and copy over the diffuse alpha.
    return float4(color, texColor.a);
}

////////////////////////////////////////////////////////////////////////////////////////////
//
// Phong Shading + Build the Dual-Paraboloid map
//
////////////////////////////////////////////////////////////////////////////////////////////
OutputVS_DP BuildDP_VS(float3 posL : POSITION0, float3 normalL : NORMAL0, float2 tex0: TEXCOORD0)
{
    // Zero out our output.
	OutputVS_DP outVS = (OutputVS_DP)0;
	
	// Transform normal to world space.
	outVS.normalW = mul(float4(normalL, 0.0f), WorldInvTrans).xyz;	
	
	// Pass on texture coordinates to be interpolated in rasterization.
	outVS.tex0 = tex0;
	
	//Render with the Dual-Paraboloid distortion
	
	// Transform to homogeneous clip space.
	outVS.posH = mul(float4(posL, 1.0f), WorldViewProj);
	
	outVS.posH.z = outVS.posH.z * Direction;
	
	float L = length( outVS.posH.xyz );							// determine the distance between (0,0,0) and the vertex
	outVS.posH = outVS.posH / L;								// divide the vertex position by the distance 
	
	outVS.z = outVS.posH.z;										// remember which hemisphere the vertex is in
	outVS.posH.z = outVS.posH.z + 1;							// add the reflected vector to find the normal vector

	outVS.posH.x = outVS.posH.x / outVS.posH.z;					// divide x coord by the new z-value
	outVS.posH.y = outVS.posH.y / outVS.posH.z;					// divide y coord by the new z-value

	outVS.posH.z = (L - NearPlane) / (FarPlane - NearPlane);	// scale the depth to [0, 1]
	outVS.posH.w = 1;											// set w to 1 so there is no w divide

	// Done--return the output.
    return outVS;
}

float4 BuildDP_PS(float3 normalW : TEXCOORD0, float2 tex0 : TEXCOORD1, float z : TEXCOORD2) : COLOR
{
	clip(z);
	
	// Interpolated normals can become unnormal--so normalize.
	normalW = normalize(normalW);
	
	// Light vector is opposite the direction of the light.
	float3 lightVecW0 = -LightDir0;
	float3 lightVecW1 = -LightDir1;
	float3 lightVecW2 = -LightDir2;
	
	// Determine the diffuse light intensity that strikes the vertex.
	float s0 = saturate(dot(lightVecW0, normalW));
	float s1 = saturate(dot(lightVecW1, normalW));
	float s2 = saturate(dot(lightVecW2, normalW));
	
	// Compute the ambient, diffuse and specular terms separatly. 
	float3 diffuse0 = s0 * LightDiffuse0.rgb;
	float3 diffuse1 = s1 * LightDiffuse1.rgb;
	float3 diffuse2 = s2 * LightDiffuse2.rgb;
	
	// Get the texture color.
	float4 texColor = tex2D(TexS, tex0 * TexScale);
	
	// Combine the color from lighting with the texture color.
	float3 color = (diffuse0 + diffuse1 + diffuse2 + LightAmbient0) * texColor.rgb;
		
	// Sum all the terms together and copy over the diffuse alpha.
    return float4(color, texColor.a);
}

technique Phong
{
    pass P0
    {
        // Specify the vertex and pixel shader associated with this pass.
        vertexShader = compile vs_2_0 PhongVS();
        pixelShader  = compile ps_2_0 PhongPS();
    }
}

technique BuildDP
{
	pass P0
	{
		vertexShader = compile vs_2_0 BuildDP_VS();
        pixelShader  = compile ps_2_0 BuildDP_PS();
        
        //FillMode = Wireframe;
	
		CullMode = NONE;
	}
}