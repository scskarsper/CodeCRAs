using System;
namespace Moosta.FileIO.Model
{
	public class MMDBone
	{
		public string Name = "";
		public int ParentIndex = -1;
		public int TailBoneIndex = -1;
		public MMDBoneType Type;
		public int IkParentBone = -1;
		public float[] Position = new float[3];
		public uint ExtendFlags;
		public int BoneIndex;
		public MMDBone Parent;
		public int deformRank;
		public int GrantParentIndex = -1;
		public float GrantRate;
	}
}
