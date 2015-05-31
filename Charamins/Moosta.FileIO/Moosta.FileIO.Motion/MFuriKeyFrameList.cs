using CTUtilCLR;
using System;
using System.IO;
using System.Text;
namespace Moosta.FileIO.Motion
{
	internal class MFuriKeyFrameList
	{
		public string BoneName;
		public int KeyFrameCount;
		public MFuriKeyFrame[] KeyFrames;
		public MFuriKeyFrameList(BinaryReader br)
		{
			br.ReadInt32();
			byte[] bytes = br.ReadBytes(130);
			this.BoneName = Encoding.Unicode.GetString(bytes);
			for (int i = 0; i < this.BoneName.Length; i++)
			{
				if (this.BoneName[i] == '\0')
				{
					this.BoneName = this.BoneName.Substring(0, i);
					break;
				}
			}
			this.KeyFrameCount = br.ReadInt32();
			this.KeyFrames = new MFuriKeyFrame[this.KeyFrameCount];
			for (int j = 0; j < this.KeyFrameCount; j++)
			{
				br.ReadInt32();
				int frameNumber = br.ReadInt32();
				br.ReadByte();
				float x = br.ReadSingle();
				float y = br.ReadSingle();
				float z = br.ReadSingle();
				float x2 = br.ReadSingle();
				float y2 = br.ReadSingle();
				float z2 = br.ReadSingle();
				float w = br.ReadSingle();
				br.ReadInt32();
				br.ReadByte();
				this.KeyFrames[j].FrameNumber = frameNumber;
				this.KeyFrames[j].Position = new CtVector3D(x, y, z);
				this.KeyFrames[j].Rotation = new CtQuaternion(x2, y2, z2, w);
			}
		}
	}
}
