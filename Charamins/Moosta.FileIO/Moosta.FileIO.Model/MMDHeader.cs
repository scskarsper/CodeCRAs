using Moosta.Common.CommonFormats;
using System;
using System.Collections.Generic;
namespace Moosta.FileIO.Model
{
	public class MMDHeader
	{
		public float Version;
		public List<ModelStringData> ModelNames = new List<ModelStringData>();
		public List<ModelStringData> Descriptions = new List<ModelStringData>();
		public List<string> Copyrights = new List<string>();
		public byte EncodeType;
		public byte AdditionalTextureCount;
		public byte VertexIndexSize = 2;
		public byte TextureIndexSize = 2;
		public byte MaterialIndexSize = 2;
		public byte BoneIndexSize = 2;
		public byte MorphIndexSize = 2;
		public byte RidgitBodyIndexSize = 2;
	}
}
