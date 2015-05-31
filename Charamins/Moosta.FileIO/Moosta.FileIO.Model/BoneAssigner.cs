using CTUtilCLR;
using System;
namespace Moosta.FileIO.Model
{
	public class BoneAssigner
	{
		public CtBone Bone
		{
			get;
			private set;
		}
		public CtMatrix4x4 LeftMatrix
		{
			get;
			private set;
		}
		public CtMatrix4x4 RightMatrix
		{
			get;
			private set;
		}
		public BoneAssigner ParentBone
		{
			get;
			private set;
		}
		public BoneAssigner(CtBone bone, CtMatrix4x4 leftMatrix, CtMatrix4x4 rightMatrix)
		{
			this.Bone = bone;
			this.LeftMatrix = leftMatrix;
			this.RightMatrix = rightMatrix;
			if (this.LeftMatrix == null)
			{
				this.LeftMatrix = CtMatrix4x4.UnitMatrix();
			}
			if (this.RightMatrix == null)
			{
				this.RightMatrix = CtMatrix4x4.UnitMatrix();
			}
			this.ParentBone = null;
		}
		public BoneAssigner(CtBone bone) : this(bone, null, null)
		{
		}
		public BoneAssigner(CtBone bone, BoneAssigner parent) : this(bone, null, null)
		{
			this.ParentBone = parent;
		}
	}
}
