
/////////////////////////////////////////////////
//
//  Dual-Paraboloid Mapped Reflections
//
//////////////////////////////////////////////////

float4x4 World;
float4x4 WorldInvTrans;
float4x4 WorldViewProj;

// The dual paraboloid map camera's view matrix
float4x4 ParaboloidBasis;

float3 EyePos;

float4 MaterialColor = float4(1.0f, .86f, .1f, 1.0f);

texture Front;
texture Back;

float TexScale;

sampler FrontTex = sampler_state
{
	Texture = <Front>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	
	AddressU = Clamp;				// Clamp sampling in U
    AddressV = Clamp;				// Clamp sampling in V
    //ClampColor = float4(0,0,0,0);	// outside of Clamp should be black
};

sampler BackTex = sampler_state
{
	Texture = <Back>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	
	AddressU = Clamp;				// Clamp sampling in U
    AddressV = Clamp;				// Clamp sampling in V
    //ClampColor = float4(0,0,0,0);	// outside of Clamp should be black
};

struct DPReflectVSInput
{
    float4 Position : POSITION0;
    float4 Normal	: NORMAL0;
};

struct DPReflectVSOutput
{
    float4 PosH		: POSITION0;
    float3 PosW		: TEXCOORD0;
    float3 NormalW	: TEXCOORD1;
};


DPReflectVSOutput DPReflectVS(DPReflectVSInput input)
{
    DPReflectVSOutput output;

    output.PosH = mul(input.Position, WorldViewProj);
    
    output.PosW = mul(input.Position, World);
    
    output.NormalW = mul(input.Normal, WorldInvTrans);

    return output;
}

float4 DPReflectPS(DPReflectVSOutput input) : COLOR0
{
    input.NormalW = normalize(input.NormalW);
    
    float3 pos = mul(float4(input.PosW, 1), ParaboloidBasis);
    float3 fromEyeW = normalize(EyePos - pos);
    
    //find the relflected ray by reflecting the fromEyeW vector across the normal
    float3 normal = mul(float4(input.NormalW, 0), ParaboloidBasis);
    
    float3 R = normalize(reflect(fromEyeW, normal));
    
    //DPM
    R = mul( float4(R, 0), ParaboloidBasis );	// transform reflection vector to the maps basis
    
    // calculate the front paraboloid map texture coordinates
    float2 front;
	float bias = 1;
	front.x = R.x / (R.z + bias);
	front.y = R.y / (R.z + bias);
	front.x = .5f * front.x + .5f; //bias and scale to correctly sample a d3d texture
	front.y = -.5f * front.y + .5f;
	
	// calculate the back paraboloid map texture coordinates
	float2 back;
	back.x = R.x / (bias - R.z);
	back.y = R.y / (bias - R.z);
	back.x = .5f * back.x + .5f; //bias and scale to correctly sample a d3d texture
	back.y = -.5f * back.y + .5f;
	
	float4 forward = tex2D( FrontTex, front );	// sample the front paraboloid map
	float4 backward = tex2D( BackTex, back );	// sample the back paraboloid map
	
    float4 finalColor = max(forward, backward);
    
    finalColor.rgb *= MaterialColor;
    return finalColor;
}

technique DPMReflect
{
    pass Pass1
    {		
        VertexShader = compile vs_2_0 DPReflectVS();
        PixelShader = compile ps_2_0 DPReflectPS();
    }
}
