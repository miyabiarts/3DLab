attribute vec3 position;
attribute vec2 texcoord;
uniform mat4 world;
uniform mat4 viewProjection;
varying vec2 uv;
varying vec4 diffuseColor;
void main(void)
{
	gl_Position = viewProjection * world * vec4(position, 1.0);
	uv = texcoord;
	diffuseColor = vec4(1,1,1,1);
}
