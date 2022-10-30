#shader vertex
#version 330 core
layout(location = 0) in vec3 aPos;   // the position variable has attribute position 0
layout(location = 1) in vec2 aColor;   // the position variable has attribute position 0

uniform mat4 rotate;
out vec2 ourColor;
void main()
{
    ourColor = aColor;
    gl_Position = rotate * vec4(aPos, 1.0);
}

#shader fragment
#version 330 core

in vec2 ourColor;
out vec4 FragColor;

uniform sampler2D texture0;

void main()
{
    FragColor = texture(texture0, ourColor);
}