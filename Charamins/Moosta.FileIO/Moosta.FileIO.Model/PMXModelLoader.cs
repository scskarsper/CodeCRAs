using Moosta.Common.CommonFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace Moosta.FileIO.Model
{
	public class PMXModelLoader : MMDModelLoader
	{
		private List<string> textureTable = new List<string>();
		private string readString(BinaryReader br)
		{
			string result = "";
			int count = br.ReadInt32();
			byte[] array = br.ReadBytes(count);
			if (this.Header.EncodeType == 0)
			{
				result = Encoding.Unicode.GetString(array, 0, array.Length);
			}
			else if (this.Header.EncodeType == 1)
			{
				result = Encoding.UTF8.GetString(array, 0, array.Length);
			}
			return result;
		}
		private float[] readVector3(BinaryReader br)
		{
			return new float[]
			{
				br.ReadSingle(),
				br.ReadSingle(),
				br.ReadSingle()
			};
		}
		private float[] readFloat(BinaryReader br, int count)
		{
			float[] array = new float[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = br.ReadSingle();
			}
			return array;
		}
		private FloatColor readColor(BinaryReader br, bool readAlpha)
		{
			FloatColor result = default(FloatColor);
			result.R = br.ReadSingle();
			result.G = br.ReadSingle();
			result.B = br.ReadSingle();
			if (readAlpha)
			{
				result.A = br.ReadSingle();
			}
			else
			{
				result.A = 1f;
			}
			return result;
		}
		private int readIndex(BinaryReader br, byte size)
		{
			switch (size)
			{
			case 1:
				return (int)br.ReadByte();
			case 2:
				return (int)br.ReadUInt16();
			case 4:
				return br.ReadInt32();
			}
			return -1;
		}
		private int readSIndex(BinaryReader br, byte size)
		{
			switch (size)
			{
			case 1:
				return (int)br.ReadSByte();
			case 2:
				return (int)br.ReadInt16();
			case 4:
				return br.ReadInt32();
			}
			return -1;
		}
		private int readVertexIndex(BinaryReader br)
		{
			return this.readIndex(br, this.Header.VertexIndexSize);
		}
		private int readTextureIndex(BinaryReader br)
		{
			return this.readSIndex(br, this.Header.TextureIndexSize);
		}
		private int readMaterialIndex(BinaryReader br)
		{
			return this.readSIndex(br, this.Header.MaterialIndexSize);
		}
		private int readBoneIndex(BinaryReader br)
		{
			return this.readSIndex(br, this.Header.BoneIndexSize);
		}
		private int readMorphIndex(BinaryReader br)
		{
			return this.readSIndex(br, this.Header.MorphIndexSize);
		}
		private int readRidgitBodyIndex(BinaryReader br)
		{
			return this.readSIndex(br, this.Header.RidgitBodyIndexSize);
		}
		public override void ReadModel(BinaryReader br)
		{
			try
			{
				this.ReadHeader(br);
				this.readVertexContainer(br);
				this.readFaceContainer(br);
				this.readTextureTableContainer(br);
				this.readMaterialContainer(br);
				this.readBoneContainer(br);
				this.readMorphContainer(br);
				int num = br.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					this.readString(br);
					this.readString(br);
					br.ReadByte();
					int num2 = br.ReadInt32();
					for (int j = 0; j < num2; j++)
					{
						if (br.ReadByte() == 0)
						{
							this.readBoneIndex(br);
						}
						else
						{
							this.readMorphIndex(br);
						}
					}
				}
				this.readRigidBodyContainer(br);
				this.readSpringJointContainer(br);
			}
			catch (Exception)
			{
			}
		}
		public void ReadHeader(BinaryReader br)
		{
			this.Header = new MMDHeader();
			br.ReadBytes(4);
			this.Header.Version = br.ReadSingle();
			br.ReadByte();
			this.Header.EncodeType = br.ReadByte();
			this.Header.AdditionalTextureCount = br.ReadByte();
			this.Header.VertexIndexSize = br.ReadByte();
			this.Header.TextureIndexSize = br.ReadByte();
			this.Header.MaterialIndexSize = br.ReadByte();
			this.Header.BoneIndexSize = br.ReadByte();
			this.Header.MorphIndexSize = br.ReadByte();
			this.Header.RidgitBodyIndexSize = br.ReadByte();
			string name = this.readString(br);
			string name2 = this.readString(br);
			string text = this.readString(br);
			string name3 = this.readString(br);
			this.Header.ModelNames.Add(new ModelStringData(MchaLanguageID.Japanese, name));
			this.Header.ModelNames.Add(new ModelStringData(MchaLanguageID.English, name2));
			this.Header.Descriptions.Add(new ModelStringData(MchaLanguageID.Japanese, text));
			this.Header.Descriptions.Add(new ModelStringData(MchaLanguageID.English, name3));
			this.Header.Copyrights.Add(text);
		}
		private void readVertexContainer(BinaryReader br)
		{
			uint num = br.ReadUInt32();
			this.VertexPositions = new float[num * 3u];
			this.VertexNormals = new float[num * 3u];
			this.VertexUVs = new float[num * 2u];
			this.VertexMatrixIndices = new int[num * 4u];
			this.VertexWeights = new float[num * 4u];
			this.VertexEdgeFlag = new byte[num];
			this.VertexEdgeScale = new float[num];
			for (uint num2 = 0u; num2 < num; num2 += 1u)
			{
				this.VertexPositions[(int)((UIntPtr)(num2 * 3u))] = br.ReadSingle();
				this.VertexPositions[(int)((UIntPtr)(num2 * 3u + 1u))] = br.ReadSingle();
				this.VertexPositions[(int)((UIntPtr)(num2 * 3u + 2u))] = br.ReadSingle();
				this.VertexNormals[(int)((UIntPtr)(num2 * 3u))] = br.ReadSingle();
				this.VertexNormals[(int)((UIntPtr)(num2 * 3u + 1u))] = br.ReadSingle();
				this.VertexNormals[(int)((UIntPtr)(num2 * 3u + 2u))] = br.ReadSingle();
				this.VertexUVs[(int)((UIntPtr)(num2 * 2u))] = br.ReadSingle();
				this.VertexUVs[(int)((UIntPtr)(num2 * 2u + 1u))] = br.ReadSingle();
				for (int i = 0; i < (int)this.Header.AdditionalTextureCount; i++)
				{
					br.ReadSingle();
					br.ReadSingle();
					br.ReadSingle();
					br.ReadSingle();
				}
				switch (br.ReadByte())
				{
				case 0:
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u))] = this.readBoneIndex(br);
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 1u))] = -1;
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 2u))] = -1;
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 3u))] = -1;
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u))] = 1f;
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 1u))] = 0f;
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 2u))] = 0f;
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 3u))] = 0f;
					break;
				case 1:
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u))] = this.readBoneIndex(br);
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 1u))] = this.readBoneIndex(br);
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 2u))] = -1;
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 3u))] = -1;
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u))] = br.ReadSingle();
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 1u))] = 1f - this.VertexWeights[(int)((UIntPtr)(num2 * 4u))];
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 2u))] = 0f;
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 3u))] = 0f;
					break;
				case 2:
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u))] = this.readBoneIndex(br);
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 1u))] = this.readBoneIndex(br);
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 2u))] = this.readBoneIndex(br);
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 3u))] = this.readBoneIndex(br);
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u))] = br.ReadSingle();
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 1u))] = br.ReadSingle();
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 2u))] = br.ReadSingle();
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 3u))] = br.ReadSingle();
					break;
				case 3:
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u))] = this.readBoneIndex(br);
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 1u))] = this.readBoneIndex(br);
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 2u))] = -1;
					this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 3u))] = -1;
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u))] = br.ReadSingle();
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 1u))] = 1f - this.VertexWeights[(int)((UIntPtr)(num2 * 4u))];
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 2u))] = 0f;
					this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 3u))] = 0f;
					this.readVector3(br);
					this.readVector3(br);
					this.readVector3(br);
					break;
				}
				this.VertexEdgeFlag[(int)((UIntPtr)num2)] = 0;
				this.VertexEdgeScale[(int)((UIntPtr)num2)] = br.ReadSingle();
			}
		}
		private void readFaceContainer(BinaryReader br)
		{
			uint num = br.ReadUInt32() / 3u;
			this.FaceVertexIndices = new uint[num * 3u];
			this.FaceMaterialIndices = new int[num];
			for (uint num2 = 0u; num2 < num; num2 += 1u)
			{
				this.FaceVertexIndices[(int)((UIntPtr)(num2 * 3u + 1u))] = (uint)this.readVertexIndex(br);
				this.FaceVertexIndices[(int)((UIntPtr)(num2 * 3u))] = (uint)this.readVertexIndex(br);
				this.FaceVertexIndices[(int)((UIntPtr)(num2 * 3u + 2u))] = (uint)this.readVertexIndex(br);
			}
		}
		private void readTextureTableContainer(BinaryReader br)
		{
			this.textureTable.Clear();
			uint num = br.ReadUInt32();
			for (uint num2 = 0u; num2 < num; num2 += 1u)
			{
				this.textureTable.Add(this.readString(br));
			}
		}
		private void readMaterialContainer(BinaryReader br)
		{
			uint num = br.ReadUInt32();
			for (uint num2 = 0u; num2 < num; num2 += 1u)
			{
				MMDMaterial mMDMaterial = new MMDMaterial();
				mMDMaterial.Name = this.readString(br);
				this.readString(br);
				mMDMaterial.Diffuse.R = br.ReadSingle();
				mMDMaterial.Diffuse.G = br.ReadSingle();
				mMDMaterial.Diffuse.B = br.ReadSingle();
				mMDMaterial.Diffuse.A = br.ReadSingle();
				mMDMaterial.Specular.R = br.ReadSingle();
				mMDMaterial.Specular.G = br.ReadSingle();
				mMDMaterial.Specular.B = br.ReadSingle();
				mMDMaterial.Specular.A = 1f;
				mMDMaterial.SpecularPower = br.ReadSingle();
				mMDMaterial.Ambient.R = br.ReadSingle();
				mMDMaterial.Ambient.G = br.ReadSingle();
				mMDMaterial.Ambient.B = br.ReadSingle();
				mMDMaterial.Ambient.A = mMDMaterial.Diffuse.A;
				mMDMaterial.renderFlag = (uint)br.ReadByte();
				mMDMaterial.EdgeColor.R = br.ReadSingle();
				mMDMaterial.EdgeColor.G = br.ReadSingle();
				mMDMaterial.EdgeColor.B = br.ReadSingle();
				mMDMaterial.EdgeColor.A = br.ReadSingle();
				mMDMaterial.EdgeWidth = br.ReadSingle();
				int num3 = this.readTextureIndex(br);
				int num4 = this.readTextureIndex(br);
				if (num3 >= 0)
				{
					mMDMaterial.TextureName = this.textureTable[num3];
				}
				mMDMaterial.SphereType = (MMDSphereType)br.ReadByte();
				if (num4 >= 0)
				{
					mMDMaterial.SphereTextureName = this.textureTable[num4];
				}
				else
				{
					mMDMaterial.SphereType = MMDSphereType.None;
				}
				byte b = br.ReadByte();
				if (b == 0)
				{
					int num5 = this.readTextureIndex(br);
					if (num5 >= 0)
					{
						mMDMaterial.ToonTextureName = this.textureTable[num5];
					}
				}
				else if (b == 1)
				{
					sbyte b2 = br.ReadSByte();
					if (b2 >= 0)
					{
						mMDMaterial.ToonTextureName = string.Format("toon{0:00}.bmp", (int)(b2 + 1));
					}
				}
				this.readString(br);
				mMDMaterial.FaceCount = br.ReadUInt32();
				this.Materials.Add(mMDMaterial);
			}
			uint num6 = 0u;
			uint num7 = 0u;
			while ((ulong)num7 < (ulong)((long)this.Materials.Count))
			{
				for (uint num8 = 0u; num8 < this.Materials[(int)num7].FaceCount; num8 += 3u)
				{
					this.FaceMaterialIndices[(int)((UIntPtr)num6)] = (int)((ushort)num7);
					num6 += 1u;
				}
				num7 += 1u;
			}
		}
		private void readBoneContainer(BinaryReader br)
		{
			uint num = br.ReadUInt32();
			this.Bones.Clear();
			this.RootBones.Clear();
			for (uint num2 = 0u; num2 < num; num2 += 1u)
			{
				MMDBone mMDBone = new MMDBone();
				mMDBone.Name = this.readString(br);
				this.readString(br);
				mMDBone.Position = this.readVector3(br);
				mMDBone.ParentIndex = this.readBoneIndex(br);
				if (mMDBone.ParentIndex < 0)
				{
					this.RootBones.Add(mMDBone);
				}
				mMDBone.deformRank = br.ReadInt32();
				uint num3 = (uint)br.ReadUInt16();
				bool flag = (num3 & 1u) == 1u;
				bool flag2 = (num3 & 32u) == 32u;
				bool flag3 = (num3 & 256u) == 256u;
				bool flag4 = (num3 & 512u) == 512u;
				bool flag5 = (num3 & 1024u) == 1024u;
				bool flag6 = (num3 & 2048u) == 2048u;
				bool flag7 = (num3 & 8192u) == 8192u;
				if (flag)
				{
					this.readBoneIndex(br);
				}
				else
				{
					this.readFloat(br, 3);
				}
				if (flag3 || flag4)
				{
					mMDBone.GrantParentIndex = this.readBoneIndex(br);
					mMDBone.GrantRate = br.ReadSingle();
				}
				if (flag5)
				{
					this.readFloat(br, 3);
				}
				if (flag6)
				{
					this.readFloat(br, 3);
					this.readFloat(br, 3);
				}
				if (flag7)
				{
					br.ReadInt32();
				}
				if (flag2)
				{
					this.readBoneIndex(br);
					br.ReadInt32();
					br.ReadSingle();
					int num4 = br.ReadInt32();
					for (int i = 0; i < num4; i++)
					{
						this.readBoneIndex(br);
						byte b = br.ReadByte();
						if (b == 1)
						{
							this.readFloat(br, 3);
							this.readFloat(br, 3);
						}
					}
				}
				mMDBone.BoneIndex = (int)num2;
				this.Bones.Add(mMDBone);
			}
			for (int j = 0; j < this.Bones.Count; j++)
			{
				if (this.Bones[j].ParentIndex >= 0)
				{
					this.Bones[j].Parent = this.Bones[this.Bones[j].ParentIndex];
				}
			}
		}
		private void readMorphContainer(BinaryReader br)
		{
			uint num = br.ReadUInt32();
			this.Morphs.Clear();
			for (uint num2 = 0u; num2 < num; num2 += 1u)
			{
				MMDMorph mMDMorph = new MMDMorph();
				mMDMorph.Name = this.readString(br);
				this.readString(br);
				br.ReadByte();
				mMDMorph.Type = (MMDMorphType)br.ReadByte();
				int num3 = br.ReadInt32();
				if (mMDMorph.Type == MMDMorphType.Vertex)
				{
					mMDMorph.VertexIndices = new uint[num3];
					mMDMorph.VertexData = new float[num3 * 3];
					for (int i = 0; i < num3; i++)
					{
						mMDMorph.VertexIndices[i] = (uint)this.readVertexIndex(br);
						mMDMorph.VertexData[i * 3] = br.ReadSingle();
						mMDMorph.VertexData[i * 3 + 1] = br.ReadSingle();
						mMDMorph.VertexData[i * 3 + 2] = br.ReadSingle();
					}
				}
				else if (mMDMorph.Type == MMDMorphType.UV || mMDMorph.Type == MMDMorphType.AddUV1 || mMDMorph.Type == MMDMorphType.AddUV2 || mMDMorph.Type == MMDMorphType.AddUV3 || mMDMorph.Type == MMDMorphType.AddUV4)
				{
					for (int j = 0; j < num3; j++)
					{
						this.readVertexIndex(br);
						br.ReadSingle();
						br.ReadSingle();
						br.ReadSingle();
						br.ReadSingle();
					}
				}
				else if (mMDMorph.Type == MMDMorphType.Bone)
				{
					for (int k = 0; k < num3; k++)
					{
						this.readBoneIndex(br);
						br.ReadSingle();
						br.ReadSingle();
						br.ReadSingle();
						br.ReadSingle();
						br.ReadSingle();
						br.ReadSingle();
						br.ReadSingle();
					}
				}
				else if (mMDMorph.Type == MMDMorphType.Material)
				{
					mMDMorph.MatgerialMorphData = new List<MMDMatgerialMorphData>();
					for (int l = 0; l < num3; l++)
					{
						MMDMatgerialMorphData mMDMatgerialMorphData = new MMDMatgerialMorphData();
						mMDMatgerialMorphData.MaterialIndex = this.readMaterialIndex(br);
						mMDMatgerialMorphData.OffsetType = br.ReadByte();
						mMDMatgerialMorphData.diffuse = this.readColor(br, true);
						mMDMatgerialMorphData.specular = this.readColor(br, false);
						mMDMatgerialMorphData.specularPower = br.ReadSingle();
						mMDMatgerialMorphData.ambient = this.readColor(br, false);
						mMDMatgerialMorphData.edgeColor = this.readColor(br, true);
						mMDMatgerialMorphData.edgeSize = br.ReadSingle();
						mMDMatgerialMorphData.texColor = this.readColor(br, true);
						mMDMatgerialMorphData.sphereColor = this.readColor(br, true);
						mMDMatgerialMorphData.toonColor = this.readColor(br, true);
						mMDMorph.MatgerialMorphData.Add(mMDMatgerialMorphData);
					}
				}
				else if (mMDMorph.Type == MMDMorphType.Group)
				{
					for (int m = 0; m < num3; m++)
					{
						this.readMorphIndex(br);
						br.ReadSingle();
					}
				}
				this.Morphs.Add(mMDMorph);
			}
		}
		private void readRigidBodyContainer(BinaryReader br)
		{
			uint num = br.ReadUInt32();
			this.RigidBodys.Clear();
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				ChRigidBody chRigidBody = new ChRigidBody();
				chRigidBody.Name = this.readString(br).Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");
				this.readString(br).Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");
				chRigidBody.relBoneIndex = this.readBoneIndex(br);
				byte b = br.ReadByte();
				chRigidBody.Filter = 1u << (int)b;
				chRigidBody.Mask = (uint)br.ReadUInt16();
				chRigidBody.ShapeType = (ChRigidBodyShapeType)br.ReadByte();
				chRigidBody.ShapeParam[0] = br.ReadSingle();
				chRigidBody.ShapeParam[1] = br.ReadSingle();
				chRigidBody.ShapeParam[2] = br.ReadSingle();
				chRigidBody.Position[0] = br.ReadSingle();
				chRigidBody.Position[1] = br.ReadSingle();
				chRigidBody.Position[2] = br.ReadSingle();
				chRigidBody.Rotate[0] = br.ReadSingle();
				chRigidBody.Rotate[1] = br.ReadSingle();
				chRigidBody.Rotate[2] = br.ReadSingle();
				for (int i = 0; i < 3; i++)
				{
					if (float.IsNaN(chRigidBody.Position[i]) || float.IsInfinity(chRigidBody.Position[i]))
					{
						chRigidBody.Position[i] = 0f;
					}
					if (float.IsNaN(chRigidBody.Rotate[i]) || float.IsInfinity(chRigidBody.Rotate[i]))
					{
						chRigidBody.Rotate[i] = 0f;
					}
				}
				chRigidBody.Weight = br.ReadSingle();
				chRigidBody.PositionDim = br.ReadSingle();
				chRigidBody.RotateDim = br.ReadSingle();
				chRigidBody.Recoil = br.ReadSingle();
				chRigidBody.Friction = br.ReadSingle();
				chRigidBody.Type = br.ReadByte();
				this.RigidBodys.Add(chRigidBody);
				num2++;
			}
			foreach (ChRigidBody current in this.RigidBodys)
			{
				if (current.relBoneIndex >= 0)
				{
					MMDBone mMDBone = this.Bones[current.relBoneIndex];
					current.Position[0] -= mMDBone.Position[0];
					current.Position[1] -= mMDBone.Position[1];
					current.Position[2] -= mMDBone.Position[2];
				}
			}
		}
		private void readSpringJointContainer(BinaryReader br)
		{
			uint num = br.ReadUInt32();
			this.SpringJoints.Clear();
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				ChSpringJoint chSpringJoint = new ChSpringJoint();
				chSpringJoint.Name = this.readString(br).Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");
				this.readString(br).Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");
				if (br.ReadByte() == 0)
				{
					chSpringJoint.RigidBodyA = this.readRidgitBodyIndex(br);
					chSpringJoint.RigidBodyB = this.readRidgitBodyIndex(br);
					chSpringJoint.Position = this.readVector3(br);
					chSpringJoint.Rotate = this.readVector3(br);
					chSpringJoint.ConstrainPosition1 = this.readVector3(br);
					chSpringJoint.ConstrainPosition2 = this.readVector3(br);
					chSpringJoint.ConstrainRotate1 = this.readVector3(br);
					chSpringJoint.ConstrainRotate2 = this.readVector3(br);
					chSpringJoint.SpringPosition = this.readVector3(br);
					chSpringJoint.SpringRotate = this.readVector3(br);
				}
				this.SpringJoints.Add(chSpringJoint);
				num2++;
			}
		}
	}
}
