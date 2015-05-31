using CTUtilCLR;
using Moosta.Common;
using Moosta.Common.DanceMotion;
using System;
using System.Collections.Generic;
using System.IO;
namespace Moosta.FileIO.Motion
{
	public class DanceMotionFactoryFromMVH : IDanceMotionFactory
	{
		private class DanceMotionSerializer : AbstractSerializer<DanceMotion>
		{
			public override Guid TargetGuid
			{
				get
				{
					return new Guid("0ad383c0-a8dd-49e2-b99f-b721767d18df");
				}
			}
			protected override void WriteTarget(BinaryWriter writer, DanceMotion target, IWritingSession session)
			{
				writer.WriteMoostaString(target.Name);
				writer.Write(target.BeatCount);
				writer.Write(target.Tags.Length);
				string[] tags = target.Tags;
				for (int i = 0; i < tags.Length; i++)
				{
					string str = tags[i];
					writer.WriteMoostaString(str);
				}
				string[] boneNames = target.FirstPose.BoneNames;
				writer.Write(boneNames.Length);
				string[] array = boneNames;
				for (int j = 0; j < array.Length; j++)
				{
					string str2 = array[j];
					writer.WriteMoostaString(str2);
				}
				writer.Write(target.Poses.Length);
				DancePose[] poses = target.Poses;
				for (int k = 0; k < poses.Length; k++)
				{
					DancePose dancePose = poses[k];
					string[] array2 = boneNames;
					for (int l = 0; l < array2.Length; l++)
					{
						string boneName = array2[l];
						CtQuaternion rotate = dancePose[boneName].Rotate;
						writer.Write(rotate.X);
						writer.Write(rotate.Y);
						writer.Write(rotate.Z);
						writer.Write(rotate.W);
					}
				}
			}
			protected override DanceMotion ReadTarget(BinaryReader reader, IReadingSession session)
			{
				string name = reader.ReadMoostaString();
				int beatCount = reader.ReadInt32();
				string[] array = new string[reader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = reader.ReadMoostaString();
				}
				string[] array2 = new string[reader.ReadInt32()];
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = reader.ReadMoostaString();
				}
				DancePose[] array3 = new DancePose[reader.ReadInt32()];
				for (int k = 0; k < array3.Length; k++)
				{
					BoneState[] array4 = new BoneState[array2.Length];
					for (int l = 0; l < array2.Length; l++)
					{
						array4[l] = new BoneState(new CtQuaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
					}
					array3[k] = new DancePose(array2, array4);
				}
				return new DanceMotion(name, beatCount, array3, null)
				{
					Tags = array
				};
			}
		}
		private class DanceMotionSerializer_Ver1 : DanceMotionFactoryFromMVH.DanceMotionSerializer
		{
			public override byte FormatVersion
			{
				get
				{
					return 1;
				}
			}
			protected override void WriteTarget(BinaryWriter writer, DanceMotion target, IWritingSession session)
			{
				writer.WriteMoostaString(target.Name);
				writer.Write(target.BeatCount);
				writer.Write(target.Tags.Length);
				string[] tags = target.Tags;
				for (int i = 0; i < tags.Length; i++)
				{
					string str = tags[i];
					writer.WriteMoostaString(str);
				}
				string[] boneNames = target.FirstPose.BoneNames;
				writer.Write(boneNames.Length);
				string[] array = boneNames;
				for (int j = 0; j < array.Length; j++)
				{
					string str2 = array[j];
					writer.WriteMoostaString(str2);
				}
				string[] skinNames = target.FirstPose.SkinNames;
				writer.Write(skinNames.Length);
				string[] array2 = skinNames;
				for (int k = 0; k < array2.Length; k++)
				{
					string str3 = array2[k];
					writer.WriteMoostaString(str3);
				}
				writer.Write(target.Poses.Length);
				DancePose[] poses = target.Poses;
				for (int l = 0; l < poses.Length; l++)
				{
					DancePose dancePose = poses[l];
					string[] array3 = boneNames;
					for (int m = 0; m < array3.Length; m++)
					{
						string boneName = array3[m];
						CtQuaternion rotate = dancePose[boneName].Rotate;
						writer.Write(rotate.X);
						writer.Write(rotate.Y);
						writer.Write(rotate.Z);
						writer.Write(rotate.W);
					}
					string[] array4 = skinNames;
					for (int n = 0; n < array4.Length; n++)
					{
						string skinName = array4[n];
						writer.Write(dancePose.GetSkinState(skinName).Weight);
					}
				}
			}
			protected override DanceMotion ReadTarget(BinaryReader reader, IReadingSession session)
			{
				string name = reader.ReadMoostaString();
				int beatCount = reader.ReadInt32();
				string[] array = new string[reader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = reader.ReadMoostaString();
				}
				string[] array2 = new string[reader.ReadInt32()];
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = reader.ReadMoostaString();
				}
				string[] array3 = new string[reader.ReadInt32()];
				for (int k = 0; k < array3.Length; k++)
				{
					array3[k] = reader.ReadMoostaString();
				}
				DancePose[] array4 = new DancePose[reader.ReadInt32()];
				for (int l = 0; l < array4.Length; l++)
				{
					BoneState[] array5 = new BoneState[array2.Length];
					for (int m = 0; m < array2.Length; m++)
					{
						array5[m] = new BoneState(new CtQuaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
					}
					SkinState[] array6 = new SkinState[array3.Length];
					for (int n = 0; n < array3.Length; n++)
					{
						array6[n] = new SkinState(reader.ReadSingle());
					}
					array4[l] = new DancePose(array2, array5, array3, array6, CtVector3D.Zero);
				}
				return new DanceMotion(name, beatCount, array4, null)
				{
					Tags = array
				};
			}
		}
		private class DanceMotionSerializer_Ver2 : DanceMotionFactoryFromMVH.DanceMotionSerializer
		{
			public override byte FormatVersion
			{
				get
				{
					return 2;
				}
			}
			protected override IEnumerable<object> GetDependencies(DanceMotion target)
			{
				yield return target.SourceDanceModel;
				yield break;
			}
			protected override void WriteTarget(BinaryWriter writer, DanceMotion target, IWritingSession session)
			{
				writer.WriteMoostaString(target.Name);
				writer.Write(target.BeatCount);
				writer.Write((target.SourceDanceModel == null) ? 0 : session.GetKey(target.SourceDanceModel));
				writer.Write(target.Tags.Length);
				string[] tags = target.Tags;
				for (int i = 0; i < tags.Length; i++)
				{
					string str = tags[i];
					writer.WriteMoostaString(str);
				}
				string[] boneNames = target.FirstPose.BoneNames;
				writer.Write(boneNames.Length);
				string[] array = boneNames;
				for (int j = 0; j < array.Length; j++)
				{
					string str2 = array[j];
					writer.WriteMoostaString(str2);
				}
				string[] skinNames = target.FirstPose.SkinNames;
				writer.Write(skinNames.Length);
				string[] array2 = skinNames;
				for (int k = 0; k < array2.Length; k++)
				{
					string str3 = array2[k];
					writer.WriteMoostaString(str3);
				}
				writer.Write(target.Poses.Length);
				DancePose[] poses = target.Poses;
				for (int l = 0; l < poses.Length; l++)
				{
					DancePose dancePose = poses[l];
					writer.Write(dancePose.BodyPosition.X);
					writer.Write(dancePose.BodyPosition.Y);
					writer.Write(dancePose.BodyPosition.Z);
					writer.Write((int)dancePose.FootState);
					string[] array3 = boneNames;
					for (int m = 0; m < array3.Length; m++)
					{
						string boneName = array3[m];
						CtQuaternion rotate = dancePose[boneName].Rotate;
						writer.Write(rotate.X);
						writer.Write(rotate.Y);
						writer.Write(rotate.Z);
						writer.Write(rotate.W);
					}
					string[] array4 = skinNames;
					for (int n = 0; n < array4.Length; n++)
					{
						string skinName = array4[n];
						writer.Write(dancePose.GetSkinState(skinName).Weight);
					}
				}
			}
			protected override DanceMotion ReadTarget(BinaryReader reader, IReadingSession session)
			{
				string name = reader.ReadMoostaString();
				int beatCount = reader.ReadInt32();
				int num = reader.ReadInt32();
				DanceModelInfo sourceModel = (num == 0) ? null : (session.GetObject(num) as DanceModelInfo);
				string[] array = new string[reader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = reader.ReadMoostaString();
				}
				string[] array2 = new string[reader.ReadInt32()];
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = reader.ReadMoostaString();
				}
				string[] array3 = new string[reader.ReadInt32()];
				for (int k = 0; k < array3.Length; k++)
				{
					array3[k] = reader.ReadMoostaString();
				}
				DancePose[] array4 = new DancePose[reader.ReadInt32()];
				for (int l = 0; l < array4.Length; l++)
				{
					CtVector3D bodyPos = new CtVector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
					DancePoseFootState footState = (DancePoseFootState)reader.ReadInt32();
					BoneState[] array5 = new BoneState[array2.Length];
					for (int m = 0; m < array2.Length; m++)
					{
						array5[m] = new BoneState(new CtQuaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
					}
					SkinState[] array6 = new SkinState[array3.Length];
					for (int n = 0; n < array3.Length; n++)
					{
						array6[n] = new SkinState(reader.ReadSingle());
					}
					array4[l] = new DancePose(array2, array5, array3, array6, bodyPos)
					{
						FootState = footState
					};
				}
				return new DanceMotion(name, beatCount, array4, sourceModel)
				{
					Tags = array
				};
			}
		}
		private static bool _registered;
		public string FileName
		{
			get;
			set;
		}
		public string[] TargetExtensions
		{
			get
			{
				return new string[]
				{
					".mvh"
				};
			}
		}
		public DanceMotionFactoryFromMVH()
		{
			DanceMotionFactoryFromMVH.RegisterSerializer();
		}
		public IDanceMotion CreateDanceMotion(string[] bones)
		{
			if (!File.Exists(this.FileName))
			{
				return null;
			}
			return Serialization.Read(this.FileName) as IDanceMotion;
		}
		public static void RegisterSerializer()
		{
			if (!DanceMotionFactoryFromMVH._registered)
			{
				Serialization.Register(new DanceModelInfoSerializer());
				Serialization.Register(new DanceMotionFactoryFromMVH.DanceMotionSerializer());
				Serialization.Register(new DanceMotionFactoryFromMVH.DanceMotionSerializer_Ver1());
				Serialization.Register(new DanceMotionFactoryFromMVH.DanceMotionSerializer_Ver2());
				DanceMotionFactoryFromMVH._registered = true;
			}
		}
	}
}
