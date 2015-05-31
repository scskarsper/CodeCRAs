using Moosta.Common.CommonFormats;
using System;
using System.Collections.Generic;
using System.IO;
namespace Moosta.FileIO.Model
{
	public abstract class MMDModelLoader
	{
		public MMDHeader Header;
		public float[] VertexPositions;
		public float[] VertexNormals;
		public float[] VertexUVs;
		public int[] VertexMatrixIndices;
		public float[] VertexWeights;
		public byte[] VertexEdgeFlag;
		public float[] VertexEdgeScale;
		public uint[] FaceVertexIndices;
		public int[] FaceMaterialIndices;
		public List<MMDMaterial> Materials = new List<MMDMaterial>();
		public List<MMDBone> Bones = new List<MMDBone>();
		public List<MMDBone> RootBones = new List<MMDBone>();
		public List<MMDMorph> Morphs = new List<MMDMorph>();
		public List<ChRigidBody> RigidBodys = new List<ChRigidBody>();
		public List<ChSpringJoint> SpringJoints = new List<ChSpringJoint>();
		public uint VertexCount
		{
			get
			{
				return (uint)(this.VertexPositions.LongLength / 3L);
			}
		}
		public uint FaceCount
		{
			get
			{
				return (uint)(this.FaceVertexIndices.LongLength / 3L);
			}
		}
		public abstract void ReadModel(BinaryReader br);
		public static MMDModelLoader GetLoader(string fileName)
		{
			MMDModelLoader result = null;
			using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader binaryReader = new BinaryReader(fileStream))
				{
					byte[] array = binaryReader.ReadBytes(4);
					if (array[0] == 80 && array[1] == 109 && array[2] == 100)
					{
						result = new PMDModelLoader();
					}
					else if (array[0] == 80 && array[1] == 77 && array[2] == 88 && array[3] == 32)
					{
						result = new PMXModelLoader();
					}
				}
			}
			return result;
		}
	}
}
