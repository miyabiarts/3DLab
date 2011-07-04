using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Texture
{
	public partial class MainForm : Form
	{
		int program;
		Plane plane;
		int tex;

		public MainForm()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			glControl1.MakeCurrent();

			program = CreateShader(Encoding.UTF8.GetString(Properties.Resources.textured_vert), Encoding.UTF8.GetString(Properties.Resources.textured_frag));
			plane = new Plane(program);

			// Texture
			GL.GenTextures(1, out tex);
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, tex);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1024, 1024, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			using (Bitmap bitmap = new Bitmap("../../texture.png"))
			{
				BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
				bitmap.UnlockBits(data);
			}
		}

		public void Render()
		{
			glControl1.MakeCurrent();

			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			GL.FrontFace(FrontFaceDirection.Cw);
			GL.CullFace(CullFaceMode.Back);

			GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


			Vector3 eyePos = new Vector3(0.0f, 0.0f, 5.0f);
			Vector3 lookAt = new Vector3(0.0f, 0.0f, 0.0f);
			Vector3 eyeUp = new Vector3(0.0f, 1.0f, 0.0f);
			Matrix4 viewMatrix = Matrix4.LookAt(eyePos, lookAt, eyeUp);
			Matrix4 projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)System.Math.PI / 4.0f, (float)glControl1.Width / (float)glControl1.Height, 0.1f, 15.0f);
			Matrix4 viewProjectionMatrix = viewMatrix * projectionMatrix;

			Matrix4 worldMatrix = Matrix4.Identity;

			GL.UseProgram(program);

			GL.UniformMatrix4(GL.GetUniformLocation(program, "viewProjection"), false, ref viewProjectionMatrix);
			GL.UniformMatrix4(GL.GetUniformLocation(program, "world"), false, ref worldMatrix);

			plane.Render();

			glControl1.SwapBuffers();
		}

		// シェーダを作成
		int CreateShader(string vertexShaderCode, string fragmentShaderCode)
		{
			int vshader = GL.CreateShader(ShaderType.VertexShader);
			int fshader = GL.CreateShader(ShaderType.FragmentShader);

			string info;
			int status_code;

			// Vertex shader
			GL.ShaderSource(vshader, vertexShaderCode);
			GL.CompileShader(vshader);
			GL.GetShaderInfoLog(vshader, out info);
			GL.GetShader(vshader, ShaderParameter.CompileStatus, out status_code);
			if (status_code != 1)
			{
				throw new ApplicationException(info);
			}

			// Fragment shader
			GL.ShaderSource(fshader, fragmentShaderCode);
			GL.CompileShader(fshader);
			GL.GetShaderInfoLog(fshader, out info);
			GL.GetShader(fshader, ShaderParameter.CompileStatus, out status_code);
			if (status_code != 1)
			{
				throw new ApplicationException(info);
			}

			int program = GL.CreateProgram();
			GL.AttachShader(program, vshader);
			GL.AttachShader(program, fshader);

			GL.LinkProgram(program);

			return program;
		}
	}
}
