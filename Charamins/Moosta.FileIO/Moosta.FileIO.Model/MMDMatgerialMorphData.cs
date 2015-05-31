using Moosta.Common.CommonFormats;
using System;
namespace Moosta.FileIO.Model
{
	public class MMDMatgerialMorphData
	{
		public int MaterialIndex;
		public byte OffsetType;
		public FloatColor diffuse;
		public FloatColor specular;
		public float specularPower;
		public FloatColor ambient;
		public FloatColor edgeColor;
		public float edgeSize;
		public FloatColor texColor;
		public FloatColor sphereColor;
		public FloatColor toonColor;
		public void InitToMul()
		{
			this.diffuse = new FloatColor(1f, 1f, 1f, 1f);
			this.specular = new FloatColor(1f, 1f, 1f, 1f);
			this.ambient = new FloatColor(1f, 1f, 1f, 1f);
			this.edgeColor = new FloatColor(1f, 1f, 1f, 1f);
			this.specularPower = 1f;
			this.edgeSize = 1f;
		}
		public void InitToAdd()
		{
			this.diffuse = new FloatColor(0f, 0f, 0f, 0f);
			this.specular = new FloatColor(0f, 0f, 0f, 0f);
			this.ambient = new FloatColor(0f, 0f, 0f, 0f);
			this.edgeColor = new FloatColor(0f, 0f, 0f, 0f);
			this.specularPower = 0f;
			this.edgeSize = 0f;
		}
	}
}
