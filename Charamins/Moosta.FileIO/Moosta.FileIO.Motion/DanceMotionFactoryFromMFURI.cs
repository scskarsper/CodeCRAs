using Moosta.Common;
using Moosta.Common.DanceMotion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace Moosta.FileIO.Motion
{
	public class DanceMotionFactoryFromMFURI : IDanceMotionFactory
	{
		private string fileName;
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
					".mfuri"
				};
			}
		}
		public DanceMotionFactoryFromMFURI()
		{
			this.fileName = "";
		}
		public DanceMotionFactoryFromMFURI(string filePath)
		{
			this.fileName = filePath;
		}
		public IDanceMotion CreateDanceMotion(string[] bones)
		{
			DanceMotion result = null;
			using (FileStream fileStream = new FileStream(this.fileName, FileMode.Open, FileAccess.Read))
			{
				result = this.readMfuri(new BinaryReader(fileStream), bones);
			}
			return result;
		}
		private DanceMotion readMfuri(BinaryReader br, string[] bones)
		{
			byte[] array = new byte[]
			{
				br.ReadByte(),
				br.ReadByte(),
				br.ReadByte(),
				br.ReadByte()
			};
			br.ReadInt32();
			br.ReadInt32();
			byte[] bytes = br.ReadBytes(130);
			string text = Encoding.Unicode.GetString(bytes);
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\0')
				{
					text = text.Substring(0, i);
					break;
				}
			}
			int frameCount = br.ReadInt32();
			int num = br.ReadInt32();
			int beatCount = br.ReadInt32();
			br.ReadByte();
			List<MFuriKeyFrameList> list = new List<MFuriKeyFrameList>();
			for (int j = 0; j < num; j++)
			{
				list.Add(new MFuriKeyFrameList(br));
			}
			DancePose[] dancePose = this.getDancePose(frameCount, list, bones);
			return new DanceMotion(text, beatCount, dancePose, null);
		}
		private DancePose[] getDancePose(int frameCount, List<MFuriKeyFrameList> keyFrameLists, string[] bones)
		{
			DancePose[] array = new DancePose[frameCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new DancePose(bones);
				foreach (MFuriKeyFrameList current in keyFrameLists)
				{
					BoneState value = new BoneState(current.KeyFrames[i].Rotation);
					array[i][current.BoneName] = value;
				}
			}
			return array;
		}
	}
}
