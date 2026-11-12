// Translated from the "2D perspective" shader by Hei (MIT License), in the form "Balatro Card Tilt" publishes it on Godot Shaders.

sampler2D input : register(s0);

float4 rowX : register(c0);

float4 rowY : register(c1);

float4 rowZ : register(c2);

float4 hostScale : register(c3);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float3 screenPoint = float3((uv - 0.5) * hostScale.xy, rowX.w);
    float3 projected = float3(dot(rowX.xyz, screenPoint), dot(rowY.xyz, screenPoint), dot(rowZ.xyz, screenPoint));
    float2 centerShift = rowY.w * float2(rowX.z, rowY.z);
    projected.xy *= rowY.w * rowZ.z;
    float2 hostPoint = ((projected.xy / projected.z) - centerShift) * hostScale.zw;

    float isFacing = step(0.0001, projected.z);
    float isInside = step(max(abs(hostPoint.x), abs(hostPoint.y)), 0.5);

    return tex2D(input, hostPoint + 0.5) * isFacing * isInside;
}
