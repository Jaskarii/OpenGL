using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenGL
{
	struct VertexBufferElement
	{
		public VertexAttribPointerType type;
		public int count;
		public bool normalized;
	};

	internal class VertexBufferLayout
    {
        public int Stride { get; private set; }

        public List<VertexBufferElement> Elements { get; private set; }

        public VertexBufferLayout()
        {
            Elements = new List<VertexBufferElement>();
            Stride = 0;
        }

		public void AddToBuffer(VertexAttribPointerType Vertextype, int count)
        {

            Elements.Add(new VertexBufferElement() { type = Vertextype, count = count, normalized = false});
			Stride += GetSize(Vertextype) * count;
		}

        public static int GetSize(VertexAttribPointerType type)
        {
            int size = 0;

            switch (type)
            {
                case VertexAttribPointerType.Short:
                    size = sizeof(short);
                    break;
                case VertexAttribPointerType.Int:
                    size = sizeof(int);
                    break;
                case VertexAttribPointerType.Float:
                    size = sizeof(float);
                    break;
                case VertexAttribPointerType.Double:
                    size = sizeof(double);
                    break;
                default:
                    break;
            }

            return size;
        }
	}
}
