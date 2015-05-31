using CTUtilCLR;
using Moosta.Common;
using Moosta.Common.DanceMotion;
using System;
using System.Collections.Generic;
namespace Moosta.FileIO
{
	public class DanceMotionFactoryFromBVH : IDanceMotionFactory
	{
		private string fileName;
		private BvhMotion bvhMotion;
		private int beatCount;
		private string motionName;
		public string FileName
		{
			get
			{
				return this.fileName;
			}
			set
			{
				this.fileName = value;
			}
		}
		public string[] TargetExtensions
		{
			get
			{
				return new string[]
				{
					".bvh"
				};
			}
		}
		public DanceMotionFactoryFromBVH(string name, string filePath, int beatCount)
		{
			this.bvhMotion = BvhFormat.Import(filePath);
			this.beatCount = beatCount;
			this.motionName = name;
			this.fileName = filePath;
		}
		public IDanceMotion CreateDanceMotion(string[] bones)
		{
			return DanceMotionFactoryFromBVH.createFromBvhMotion(this.motionName, this.bvhMotion, this.beatCount, bones);
		}
		private static DanceMotion createFromBvhMotion(string name, BvhMotion bvhMotion, int beatCount, string[] bones)
		{
			Array.Sort<string>(bones);
			DancePose[] array = new DancePose[bvhMotion.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = DanceMotionFactoryFromBVH.createPoseFromBvhPose(bvhMotion.RootNode, bvhMotion[i], bones);
			}
			return new DanceMotion(name, beatCount, array, null);
		}
		private static DancePose createPoseFromBvhPose(BvhNode skeleton, double[] bvhPose, string[] bones)
		{
			Dictionary<string, BoneState> items = new Dictionary<string, BoneState>();
			int num = 0;
			foreach (BvhNode current in skeleton.GetAllNodesRecursively())
			{
				Angle x = Angle.Deg000;
				Angle y = Angle.Deg000;
				Angle z = Angle.Deg000;
				BvhChannels[] channels = current.Channels;
				for (int i = 0; i < channels.Length; i++)
				{
					switch (channels[i])
					{
					case BvhChannels.Xrotation:
						x = -Angle.FromDegree(bvhPose[num]);
						break;
					case BvhChannels.Yrotation:
						y = -Angle.FromDegree(bvhPose[num]);
						break;
					case BvhChannels.Zrotation:
						z = Angle.FromDegree(bvhPose[num]);
						break;
					}
					num++;
				}
				BoneState value = new BoneState(x, y, z);
				if (current.Name.Equals("Hips"))
				{
					value.Rotate = new CtQuaternion(new CtVector3D(0f, 1f, 0f), 3.1415926535897931) * value.Rotate;
				}
				items[current.Name] = value;
			}
			return new DancePose(bones, Array.ConvertAll<string, BoneState>(bones, delegate(string name)
			{
				if (!items.ContainsKey(name))
				{
					return BoneState.Zero;
				}
				return items[name];
			}));
		}
	}
}
