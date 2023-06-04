float2 OutlineWidth;

texture ModelTexture;
sampler2D textureSampler = sampler_state {
    Texture = (ModelTexture);
    MinFilter = Point;
    MagFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = input.Position;
    output.TextureCoordinate = input.TextureCoordinate;
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
    
    float alpha = textureColor.a;

    // Sides:
    alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate + float2(OutlineWidth.x, 0.0)).a);
    alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate - float2(OutlineWidth.x, 0.0)).a);
    alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate + float2(0.0, OutlineWidth.y)).a);
    alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate - float2(0.0, OutlineWidth.y)).a);

    // Corners:
    alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate + float2(OutlineWidth.x, OutlineWidth.y)).a);
    alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate - float2(OutlineWidth.x, -OutlineWidth.y)).a);
    alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate + float2(OutlineWidth.x, -OutlineWidth.y)).a);
    alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate - float2(OutlineWidth.x, OutlineWidth.y)).a);

    if (alpha > textureColor.a)
    {
        return float4(0.1, 0.1, 0.1, 1.0);
    }
    
    if (textureColor.a < 1.0) discard;
       
    return textureColor;
}

technique Textured
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}