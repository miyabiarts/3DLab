
uniform sampler2D tex;
varying vec2 uv;
varying vec4 diffuseColor;
void main(void)
{	
	gl_FragColor = texture2D(tex,uv);
}
