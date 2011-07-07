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


namespace FBO
{
	public partial class MainForm : Form
	{
		int program;
		Cube cube;
		int tex;

	  const int TexSize = 1024;
		int colorTex;
		int depthTex;
		int fbo;

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			glControl1.MakeCurrent();

			program = CreateShader(Encoding.UTF8.GetString(Properties.Resources.textured_vert), Encoding.UTF8.GetString(Properties.Resources.textured_frag));
			cube = new Cube(program);

			// Texture
			GL.GenTextures(1, out tex);
			GL.BindTexture(TextureTarget.Texture2D, tex);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			using (Bitmap bitmap = new Bitmap("../../texture.png"))
			{
				BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
				bitmap.UnlockBits(data);
			}

			// Color Tex
			GL.GenTextures(1, out colorTex);
			GL.BindTexture(TextureTarget.Texture2D, colorTex);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TexSize, TexSize, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			// Depth Tex
			GL.GenTextures(1, out depthTex);
			GL.BindTexture(TextureTarget.Texture2D, depthTex);
			GL.TexImage2D(TextureTarget.Texture2D, 0, (PixelInternalFormat)All.DepthComponent32, TexSize, TexSize, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);


			// FBO
			GL.Ext.GenFramebuffers(1, out fbo);
			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fbo);
			GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, colorTex, 0);
			GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, depthTex, 0);

			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			cube.Dispose();

			GL.DeleteTexture(tex);
			GL.DeleteTexture(colorTex);
			GL.DeleteTexture(tex);

			GL.DeleteFramebuffers(1, ref fbo);

			GL.DeleteProgram(program);
		}

		public void Render()
		{
			glControl1.MakeCurrent();

			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			GL.FrontFace(FrontFaceDirection.Cw);
			GL.CullFace(CullFaceMode.Back);

			// 1 pass
			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fbo);

			GL.Viewport(0, 0, TexSize, TexSize);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			Vector3 eyePos = new Vector3(3.0f, 3.0f, 3.0f);
			Vector3 lookAt = new Vector3(0.0f, 0.0f, 0.0f);
			Vector3 eyeUp = new Vector3(0.0f, 1.0f, 0.0f);
			Matrix4 viewMatrix = Matrix4.LookAt(eyePos, lookAt, eyeUp);
			Matrix4 projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)System.Math.PI / 4.0f, (float)TexSize / (float)TexSize, 0.1f, 15.0f);
			Matrix4 viewProjectionMatrix = viewMatrix * projectionMatrix;

			Matrix4 worldMatrix = Matrix4.Identity;

			GL.UseProgram(program);

			GL.UniformMatrix4(GL.GetUniformLocation(program, "viewProjection"), false, ref viewProjectionMatrix);
			GL.UniformMatrix4(GL.GetUniformLocation(program, "world"), false, ref worldMatrix);
			
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, tex);
			GL.Uniform1(GL.GetUniformLocation(program, "tex"), 0);
			
			cube.Render();

			// 2 pass
			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);

			GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)System.Math.PI / 4.0f, (float)glControl1.Width / (float)glControl1.Height, 0.1f, 15.0f);
			viewProjectionMatrix = viewMatrix * projectionMatrix;

			GL.UniformMatrix4(GL.GetUniformLocation(program, "viewProjection"), false, ref viewProjectionMatrix);
			GL.UniformMatrix4(GL.GetUniformLocation(program, "world"), false, ref worldMatrix);

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, colorTex);
			GL.Uniform1(GL.GetUniformLocation(program, "tex"), 0);

			cube.Render();


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
