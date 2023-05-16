float4x4 World;
float4x4 View;
float4x4 Projection;
float Alpha;
float OutlineWidth;
bool UseOutline;

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
    float4 Color: COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    
    output.Color = input.Color;
    output.TextureCoordinate = input.TextureCoordinate;
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
    
    if (textureColor.a < 1.0)
    {
        if (UseOutline)
        {
            float alpha = textureColor.a;
            
            // Sides:
            alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate + float2(OutlineWidth, 0.0)).a);
            alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate - float2(OutlineWidth, 0.0)).a);
            alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate + float2(0.0, OutlineWidth)).a);
            alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate - float2(0.0, OutlineWidth)).a);
            
            // Corners:
            alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate + float2(OutlineWidth, OutlineWidth)).a);
            alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate - float2(OutlineWidth, -OutlineWidth)).a);
            alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate + float2(OutlineWidth, -OutlineWidth)).a);
            alpha = max(alpha, tex2D(textureSampler, input.TextureCoordinate - float2(OutlineWidth, OutlineWidth)).a);
            
            if (alpha > textureColor.a)
            {
                return float4(0.0, 0.0, 0.0, 1.0);
            }
        }
    
        discard;
    }
    
    float4 color = textureColor * input.Color;
    color.a = Alpha;
       
    return color;
}

technique Textured
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}