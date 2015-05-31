using Moosta.Common.CommonFormats;
using System;
namespace Moosta.FileIO.Model
{
	public class MMDMaterial
	{
		public string Name = "";
		public FloatColor Ambient = default(FloatColor);
		public FloatColor Diffuse = default(FloatColor);
		public FloatColor Specular = default(FloatColor);
		public FloatColor EdgeColor = new FloatColor(1f, 0f, 0f, 0f);
		public float SpecularPower;
		public string TextureName = "";
		public string SphereTextureName = "";
		public MMDSphereType SphereType;
		public string ToonTextureName = "";
		public float EdgeWidth = 0.8f;
		public uint renderFlag;
		public uint extendFlag;
		public int toonTextureIndex;
		public uint FaceCount;
	}
}
