using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Texture
{
	class Plane
	{
		struct Vertex
		{
			public Vector3 position;
			public Vector2 texcoord;

			public Vertex(Vector3 position, Vector2 texcoord)
			{
				this.position = position;
				this.texcoord = texcoord;
			}
			public static readonly int Stride = Marshal.SizeOf(default(Vertex));
		}

		int vbo;
		int vao;
		int ebo;
		int vertexCount;

		public Plane(int program)
		{
			GL.GenBuffers(1, out vbo);
			Vertex[] v = new Vertex[]{
				new Vertex(new Vector3(-1.0f, 1.0f, 0.0f), new Vector2( 0.0f, 0.0f)),
				new Vertex(new Vector3( 1.0f, 1.0f, 0.0f), new Vector2( 1.0f, 0.0f)),
				new Vertex(new Vector3( 1.0f,-1.0f, 0.0f), new Vector2( 1.0f, 1.0f)),
				new Vertex(new Vector3(-1.0f,-1.0f, 0.0f), new Vector2( 0.0f, 1.0f)),
			};



			uint[] indices = new uint[]{
             0,1,2,
						 0,2,3,
			};

			// VBO作成
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
			GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, new IntPtr(v.Length * Vertex.Stride), v, BufferUsageHint.StaticDraw);

			// EBO作成
			GL.GenBuffers(1, out ebo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
			GL.BufferData<uint>(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(uint) * indices.Length), indices, BufferUsageHint.StaticDraw);
			vertexCount = indices.Length;

			// VAO作成
			GL.GenVertexArrays(1, out vao);
			GL.BindVertexArray(vao);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

			int positionLocation = GL.GetAttribLocation(program, "position");
			int normalLocation = GL.GetAttribLocation(program, "texcoord");

			GL.EnableVertexAttribArray(positionLocation);
			GL.EnableVertexAttribArray(normalLocation);

			GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 0);
			GL.VertexAttribPointer(normalLocation, 2, VertexAttribPointerType.Float, true, Vertex.Stride, Vector3.SizeInBytes);
		}

		public void Dispose()
		{
			GL.DeleteBuffers(1, ref vbo);
			GL.DeleteBuffers(1, ref ebo);
			GL.DeleteVertexArrays(1, ref vao);
		}

		// レンダリング
		public void Render()
		{
			GL.BindVertexArray(vao);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
			GL.DrawElements(BeginMode.Triangles, vertexCount, DrawElementsType.UnsignedInt, 0);
		}
	}
}
