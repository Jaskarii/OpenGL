#shader vertex
#version 330 core
layout(location = 0) in vec3 aPos;   // the position variable has attribute position 0

void main()
{
    gl_Position = vec4(aPos, 1.0);
}

#shader geometry
#version 330 core
layout(triangles) in;
layout(line_strip, max_vertices = 16) out;

vec4 asdBezier(vec4 p1, vec4 p2, vec4 p3, float t)
{
    vec4 asd;
    asd = p2 + (1-t)*(1-t)*(p1-p2)+t*t*(p3-p2);
    return asd;
}

void main() 
{

    gl_Position = gl_in[0].gl_Position;
    EmitVertex();
    gl_Position = asdBezier(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_in[2].gl_Position, 0.1);
    EmitVertex();
    gl_Position = asdBezier(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_in[2].gl_Position, 0.2);
    EmitVertex();

    gl_Position = asdBezier(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_in[2].gl_Position, 0.3);
    EmitVertex();
    gl_Position = asdBezier(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_in[2].gl_Position, 0.4);
    EmitVertex();

    gl_Position = asdBezier(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_in[2].gl_Position, 0.5);
    EmitVertex();
    gl_Position = asdBezier(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_in[2].gl_Position, 0.6);
    EmitVertex();

    gl_Position = asdBezier(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_in[2].gl_Position, 0.7);
    EmitVertex();
    gl_Position = asdBezier(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_in[2].gl_Position, 0.8);
    EmitVertex();

    gl_Position = asdBezier(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_in[2].gl_Position, 0.9);
    EmitVertex();
    gl_Position = asdBezier(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_in[2].gl_Position, 1.0);
    EmitVertex();

    EndPrimitive();
}

#shader fragment
#version 330 core
out vec4 FragColor;

void main()
{
    FragColor = vec4(1.0,0.0,0.0,1.0);
}