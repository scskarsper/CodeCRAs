using Moosta.Common.CommonFormats;
using System;
using System.IO;
using System.Text;
namespace Moosta.FileIO.Model
{
	public class PMDModelLoader : MMDModelLoader
	{
		private string[] toonTextureNames = new string[15];
		private MMDMorph baseMorph = new MMDMorph();
		public override void ReadModel(BinaryReader br)
		{
			try
			{
				this.ReadHeader(br);
				this.readVertexContainer(br);
				this.readFaceContainer(br);
				this.Materials.Clear();
				this.readMaterialContainer(br);
				this.Bones.Clear();
				this.readBoneContainer(br);
				int num = (int)br.ReadUInt16();
				for (int i = 0; i < num; i++)
				{
					br.ReadInt16();
					br.ReadInt16();
					byte b = br.ReadByte();
					br.ReadInt16();
					br.ReadSingle();
					for (int j = 0; j < (int)b; j++)
					{
						br.ReadInt16();
					}
				}
				this.readMorphContainer(br);
				byte b2 = br.ReadByte();
				for (int k = 0; k < (int)b2; k++)
				{
					br.ReadUInt16();
				}
				byte b3 = br.ReadByte();
				for (int l = 0; l < (int)b3; l++)
				{
					this.readString(br, 50);
				}
				int num2 = br.ReadInt32();
				for (int m = 0; m < num2; m++)
				{
					br.ReadInt16();
					br.ReadByte();
				}
				if (br.PeekChar() > -1)
				{
					byte b4 = br.ReadByte();
					if (b4 == 1)
					{
						string name = this.readString(br, 20);
						string name2 = this.readString(br, 256);
						this.Header.ModelNames.Add(new ModelStringData(MchaLanguageID.English, name));
						this.Header.Descriptions.Add(new ModelStringData(MchaLanguageID.English, name2));
						for (int n = 0; n < this.Bones.Count; n++)
						{
							this.readString(br, 20);
						}
						for (int num3 = 0; num3 < this.Morphs.Count; num3++)
						{
							this.readString(br, 20);
						}
						for (int num4 = 0; num4 < (int)b3; num4++)
						{
							this.readString(br, 50);
						}
					}
				}
				if (br.PeekChar() > -1)
				{
					for (int num5 = 0; num5 < this.toonTextureNames.Length; num5++)
					{
						this.toonTextureNames[num5] = string.Format("toon{0:00}.bmp", num5 + 1);
					}
					for (int num6 = 0; num6 < 10; num6++)
					{
						this.toonTextureNames[num6] = this.readString(br, 100);
					}
					foreach (MMDMaterial current in this.Materials)
					{
						if (current.toonTextureIndex >= 0)
						{
							current.ToonTextureName = this.toonTextureNames[current.toonTextureIndex];
						}
					}
				}
				if (br.PeekChar() > -1)
				{
					this.readRigidBodyContainer(br);
					this.readSpringJointContainer(br);
				}
			}
			catch (Exception)
			{
			}
		}
		private string readString(BinaryReader br, int bytes)
		{
			byte[] array = br.ReadBytes(bytes);
			int num = 0;
			while (array[num] != 0)
			{
				num++;
			}
			return Encoding.GetEncoding("Shift_JIS").GetString(array, 0, num);
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
		public void ReadHeader(BinaryReader br)
		{
			this.Header = new MMDHeader();
			br.ReadBytes(3);
			this.Header.Version = br.ReadSingle();
			string name = this.readString(br, 20);
			string text = this.readString(br, 256);
			this.Header.ModelNames.Add(new ModelStringData(name));
			this.Header.Descriptions.Add(new ModelStringData(text));
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
				this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u))] = (int)br.ReadInt16();
				this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 1u))] = (int)br.ReadInt16();
				this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 2u))] = -1;
				this.VertexMatrixIndices[(int)((UIntPtr)(num2 * 4u + 3u))] = -1;
				byte b = br.ReadByte();
				this.VertexWeights[(int)((UIntPtr)(num2 * 4u))] = (float)b / 100f;
				this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 1u))] = 1f - (float)b / 100f;
				this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 2u))] = 0f;
				this.VertexWeights[(int)((UIntPtr)(num2 * 4u + 3u))] = 0f;
				this.VertexEdgeFlag[(int)((UIntPtr)num2)] = br.ReadByte();
				this.VertexEdgeScale[(int)((UIntPtr)num2)] = 1f;
			}
		}
		private void readFaceContainer(BinaryReader br)
		{
			uint num = br.ReadUInt32() / 3u;
			this.FaceVertexIndices = new uint[num * 3u];
			this.FaceMaterialIndices = new int[num];
			for (uint num2 = 0u; num2 < num; num2 += 1u)
			{
				this.FaceVertexIndices[(int)((UIntPtr)(num2 * 3u))] = (uint)br.ReadUInt16();
				this.FaceVertexIndices[(int)((UIntPtr)(num2 * 3u + 2u))] = (uint)br.ReadUInt16();
				this.FaceVertexIndices[(int)((UIntPtr)(num2 * 3u + 1u))] = (uint)br.ReadUInt16();
			}
		}
		private void readMaterialContainer(BinaryReader br)
		{
			uint num = br.ReadUInt32();
			for (uint num2 = 0u; num2 < num; num2 += 1u)
			{
				MMDMaterial mMDMaterial = new MMDMaterial();
				mMDMaterial.Name = "Material" + num2;
				mMDMaterial.Diffuse.R = br.ReadSingle();
				mMDMaterial.Diffuse.G = br.ReadSingle();
				mMDMaterial.Diffuse.B = br.ReadSingle();
				mMDMaterial.Diffuse.A = br.ReadSingle();
				mMDMaterial.SpecularPower = br.ReadSingle();
				mMDMaterial.Specular.R = br.ReadSingle();
				mMDMaterial.Specular.G = br.ReadSingle();
				mMDMaterial.Specular.B = br.ReadSingle();
				mMDMaterial.Specular.A = 1f;
				mMDMaterial.Ambient.R = br.ReadSingle();
				mMDMaterial.Ambient.G = br.ReadSingle();
				mMDMaterial.Ambient.B = br.ReadSingle();
				mMDMaterial.Ambient.A = mMDMaterial.Diffuse.A;
				mMDMaterial.toonTextureIndex = (int)br.ReadSByte();
				if (mMDMaterial.toonTextureIndex < 0)
				{
					mMDMaterial.ToonTextureName = "";
				}
				else
				{
					mMDMaterial.ToonTextureName = string.Format("toon{0:00}.bmp", mMDMaterial.toonTextureIndex + 1);
				}
				byte b = br.ReadByte();
				if (b == 1)
				{
					mMDMaterial.EdgeWidth = 0.6f;
				}
				else
				{
					mMDMaterial.EdgeWidth = 0f;
				}
				mMDMaterial.FaceCount = br.ReadUInt32();
				mMDMaterial.SphereType = MMDSphereType.None;
				string text = this.readString(br, 20);
				string[] array = text.Split(new char[]
				{
					'*'
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = Path.GetExtension(array[i]).ToLower();
					if (text2.Equals(".sph"))
					{
						mMDMaterial.SphereTextureName = array[i];
						mMDMaterial.SphereType = MMDSphereType.Mul;
					}
					else if (text2.Equals(".spa"))
					{
						mMDMaterial.SphereTextureName = array[i];
						mMDMaterial.SphereType = MMDSphereType.Add;
					}
					else
					{
						mMDMaterial.TextureName = array[i];
					}
				}
				this.Materials.Add(mMDMaterial);
				mMDMaterial.renderFlag = 14u;
				mMDMaterial.renderFlag |= ((mMDMaterial.Diffuse.A < 1f) ? 1u : 0u);
				mMDMaterial.renderFlag |= ((mMDMaterial.EdgeWidth > 0f) ? 16u : 0u);
				mMDMaterial.extendFlag = 255u;
			}
			uint num3 = 0u;
			uint num4 = 0u;
			while ((ulong)num4 < (ulong)((long)this.Materials.Count))
			{
				for (uint num5 = 0u; num5 < this.Materials[(int)num4].FaceCount; num5 += 3u)
				{
					this.FaceMaterialIndices[(int)((UIntPtr)num3)] = (int)((ushort)num4);
					num3 += 1u;
				}
				num4 += 1u;
			}
		}
		private void readBoneContainer(BinaryReader br)
		{
			uint num = (uint)br.ReadUInt16();
			this.Bones.Clear();
			this.RootBones.Clear();
			for (uint num2 = 0u; num2 < num; num2 += 1u)
			{
				MMDBone mMDBone = new MMDBone();
				mMDBone.BoneIndex = (int)num2;
				mMDBone.Name = this.readString(br, 20);
				mMDBone.ParentIndex = (int)br.ReadInt16();
				mMDBone.TailBoneIndex = (int)br.ReadInt16();
				mMDBone.Type = (MMDBoneType)br.ReadByte();
				mMDBone.IkParentBone = (int)br.ReadInt16();
				mMDBone.Position[0] = br.ReadSingle();
				mMDBone.Position[1] = br.ReadSingle();
				mMDBone.Position[2] = br.ReadSingle();
				this.Bones.Add(mMDBone);
				if (mMDBone.ParentIndex < 0)
				{
					this.RootBones.Add(mMDBone);
				}
			}
			for (int i = 0; i < this.Bones.Count; i++)
			{
				if (this.Bones[i].ParentIndex >= 0)
				{
					this.Bones[i].Parent = this.Bones[this.Bones[i].ParentIndex];
				}
			}
		}
		private void readMorphContainer(BinaryReader br)
		{
			uint num = (uint)br.ReadUInt16();
			this.Morphs.Clear();
			for (uint num2 = 0u; num2 < num; num2 += 1u)
			{
				MMDMorph mMDMorph = new MMDMorph();
				mMDMorph.Name = this.readString(br, 20);
				uint num3 = br.ReadUInt32();
				if (br.ReadByte() == 0)
				{
					this.baseMorph = mMDMorph;
				}
				else
				{
					this.Morphs.Add(mMDMorph);
				}
				mMDMorph.Type = MMDMorphType.Vertex;
				mMDMorph.VertexIndices = new uint[num3];
				mMDMorph.VertexData = new float[num3 * 3u];
				for (uint num4 = 0u; num4 < num3; num4 += 1u)
				{
					mMDMorph.VertexIndices[(int)((UIntPtr)num4)] = br.ReadUInt32();
					mMDMorph.VertexData[(int)((UIntPtr)(num4 * 3u))] = br.ReadSingle();
					mMDMorph.VertexData[(int)((UIntPtr)(num4 * 3u + 1u))] = br.ReadSingle();
					mMDMorph.VertexData[(int)((UIntPtr)(num4 * 3u + 2u))] = br.ReadSingle();
				}
			}
			foreach (MMDMorph current in this.Morphs)
			{
				for (int i = 0; i < current.VertexIndices.Length; i++)
				{
					current.VertexIndices[i] = this.baseMorph.VertexIndices[(int)((UIntPtr)current.VertexIndices[i])];
				}
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
				chRigidBody.Name = this.readString(br, 20).Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");
				chRigidBody.relBoneIndex = (int)br.ReadInt16();
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
					if (float.IsNaN(chRigidBody.Position[i]))
					{
						chRigidBody.Position[i] = 0f;
					}
					if (float.IsNaN(chRigidBody.Rotate[i]))
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
		}
		private void readSpringJointContainer(BinaryReader br)
		{
			uint num = br.ReadUInt32();
			this.SpringJoints.Clear();
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				ChSpringJoint chSpringJoint = new ChSpringJoint();
				chSpringJoint.Name = this.readString(br, 20).Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");
				chSpringJoint.RigidBodyA = br.ReadInt32();
				chSpringJoint.RigidBodyB = br.ReadInt32();
				chSpringJoint.Position = this.readVector3(br);
				chSpringJoint.Rotate = this.readVector3(br);
				chSpringJoint.ConstrainPosition1 = this.readVector3(br);
				chSpringJoint.ConstrainPosition2 = this.readVector3(br);
				chSpringJoint.ConstrainRotate1 = this.readVector3(br);
				chSpringJoint.ConstrainRotate2 = this.readVector3(br);
				chSpringJoint.SpringPosition = this.readVector3(br);
				chSpringJoint.SpringRotate = this.readVector3(br);
				this.SpringJoints.Add(chSpringJoint);
				num2++;
			}
		}
	}
}
