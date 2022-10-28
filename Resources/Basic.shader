#shader vertex
#version 330 core
layout (location = 0) in vec3 aPos;   // the position variable has attribute position 0
//layout (location = 1) in vec3 aColor; // the color variable has attribute position 1
  
//out vec3 ourColor; // output a color to the fragment shader
out vec2 posit;

void main()
{
    gl_Position = vec4(aPos, 1.0);
    //ourColor = aColor; // set ourColor to the input color we got from the vertex data
}

#shader geometry
#version 330 core
layout(lines) in;
layout(triangle_strip, max_vertices = 7) out;
out vec2 p1;
out vec2 p2;

void main() {

    p1 = vec2(gl_in[0].gl_Position.x, gl_in[0].gl_Position.y);
    p2 = vec2(gl_in[1].gl_Position.x, gl_in[1].gl_Position.y);
    vec4 dir = gl_in[1].gl_Position - gl_in[0].gl_Position;
    vec4 directioNormal = vec4(dir.x, dir.y, 0.0, 0.0);
    directioNormal = directioNormal / length(directioNormal);
    vec4 tan = vec4(dir.y, -dir.x, 0.0, 0.0);
    vec4 norm = tan / length(tan);
    gl_Position = gl_in[0].gl_Position + norm * 0.1;
    EmitVertex();

    gl_Position = gl_in[0].gl_Position - norm * 0.1;
    EmitVertex();

    gl_Position = gl_in[1].gl_Position + norm * 0.1;
    EmitVertex();
    
    gl_Position = gl_in[1].gl_Position - norm * 0.1;
    EmitVertex();

    gl_Position = gl_in[1].gl_Position + directioNormal * 0.15;
    EmitVertex();
    EndPrimitive();
}

#shader fragment
#version 330 core
out vec4 FragColor;

uniform float windowWidth;
uniform float windowHeight;

in vec2 p1;
in vec2 p2;
uniform vec2 mouseposition;
  
void main()
{
    vec2 p3 = vec2((gl_FragCoord.x / windowWidth - 0.5), ((gl_FragCoord.y / windowHeight)-0.5 ));
    if (p3.y < 0.1)
    {
        FragColor = vec4(1.0, 1.0, 1.0, 1.0);
    }
    else
    {
        FragColor = vec4(0.3, 0.3, 0.0, 1.0);
    }
    
}