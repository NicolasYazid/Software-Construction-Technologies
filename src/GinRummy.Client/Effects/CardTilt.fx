// Translated from the "2D perspective" shader by Hei (MIT License), in the form "Balatro Card Tilt" publishes it on Godot Shaders.
// WPF pixel shaders have no vertex stage, so the projected point that Godot interpolates between the corners is computed for every pixel.
// The result is the same because that point depends linearly on the texture coordinate.
// Compile with: fxc /T ps_2_0 /E main /Fo CardTilt.ps CardTilt.fx
// Shader model 2.0 is kept because it is the only one WPF can still run when it falls back to software rendering.

sampler2D input : register(s0);

// xyz holds the first row of the inverse rotation and w the distance from the camera to the screen plane.
float4 rowX : register(c0);

// xyz holds the second row of the inverse rotation and w the distance from the camera to the card.
float4 rowY : register(c1);

// xyz holds the third row of the inverse rotation.
float4 rowZ : register(c2);

// xy maps the host to the screen plane, and zw maps the card back to the host.
// The host is larger than the card so that a tilted card has room to grow without being cut.
float4 hostScale : register(c3);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float3 screenPoint = float3((uv - 0.5) * hostScale.xy, rowX.w);
    float3 projected = float3(dot(rowX.xyz, screenPoint), dot(rowY.xyz, screenPoint), dot(rowZ.xyz, screenPoint));
    float2 centerShift = rowY.w * float2(rowX.z, rowY.z);
    projected.xy *= rowY.w * rowZ.z;
    float2 hostPoint = ((projected.xy / projected.z) - centerShift) * hostScale.zw;

    // A pixel behind the camera or outside the host is left transparent, as the original discards it.
    float isFacing = step(0.0001, projected.z);
    float isInside = step(max(abs(hostPoint.x), abs(hostPoint.y)), 0.5);

    // WPF works with premultiplied alpha, so the mask scales the whole color and not only its alpha.
    return tex2D(input, hostPoint + 0.5) * isFacing * isInside;
}
