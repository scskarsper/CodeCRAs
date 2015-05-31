using System;
using System.Collections.Generic;
namespace Moosta.FileIO.Model
{
	public class MMDMorph
	{
		public string Name;
		public MMDMorphType Type;
		public uint[] VertexIndices;
		public float[] VertexData;
		public List<MMDMatgerialMorphData> MatgerialMorphData;
	}
}
