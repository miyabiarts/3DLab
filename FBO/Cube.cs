using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace FBO
{
	class Cube
	{
		// キューブを構成する頂点要素（シェーダの頂点要素と合わせる）
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
		int vertexCount;

		public Cube(int program)
		{
			GL.GenBuffers(1, out vbo);

			Vector3[] pos = new Vector3[]{
				new Vector3(-1.0f, 1.0f,-1.0f),
				new Vector3( 1.0f, 1.0f,-1.0f),
				new Vector3( 1.0f, 1.0f, 1.0f), 
				new Vector3(-1.0f, 1.0f, 1.0f),
				new Vector3(-1.0f,-1.0f,-1.0f),
				new Vector3( 1.0f,-1.0f,-1.0f),
				new Vector3( 1.0f,-1.0f, 1.0f),
				new Vector3(-1.0f,-1.0f, 1.0f),
			};

			Vector2[] texcoord = new Vector2[]{
				new Vector2(0.0f,0.0f),
				new Vector2(1.0f,0.0f),
				new Vector2(1.0f,1.0f),
				new Vector2(0.0f,1.0f),
			};


			uint[] indices = new uint[]{
             0,1,2,3,
						 1,0,4,5,
						 2,1,5,6,
						 3,2,6,7,
						 0,3,7,4,
						 7,6,5,4
			};


			List<Vertex> v = new List<Vertex>();
			for (int i = 0; i < indices.Count(); ++i)
			{
				v.Add(new Vertex(pos[indices[i]], texcoord[i%4]));
			}
			

			// VBO作成
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
			GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, new IntPtr(v.Count() * Vertex.Stride), v.ToArray(), BufferUsageHint.StaticDraw);
			vertexCount = v.Count();

			// VAO作成
			GL.GenVertexArrays(1, out vao);
			GL.BindVertexArray(vao);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

			int positionLocation = GL.GetAttribLocation(program, "position");
			int texcoordLocation = GL.GetAttribLocation(program, "texcoord");

			GL.EnableVertexAttribArray(positionLocation);
			GL.EnableVertexAttribArray(texcoordLocation);

			GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 0);
			GL.VertexAttribPointer(texcoordLocation, 2, VertexAttribPointerType.Float, true, Vertex.Stride, Vector3.SizeInBytes);
		}

		public void Dispose()
		{
			GL.DeleteBuffers(1, ref vbo);
			GL.DeleteVertexArrays(1, ref vao);
		}

		// レンダリング
		public void Render()
		{
			GL.BindVertexArray(vao);
			GL.DrawArrays(BeginMode.Quads, 0, vertexCount);
		}

	}

}
