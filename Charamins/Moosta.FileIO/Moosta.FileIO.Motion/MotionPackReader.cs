using Moosta.Common;
using Moosta.Common.CommonFormats;
using Moosta.Common.DanceMotion;
using Moosta.Common.UI;
using System;
using System.IO;
using System.IO.Compression;
namespace Moosta.FileIO.Motion
{
	public class MotionPackReader
	{
		private static bool motionPackReadLock;
		public static bool MotionPackReadLock
		{
			get
			{
				return MotionPackReader.motionPackReadLock;
			}
			set
			{
				MotionPackReader.motionPackReadLock = value;
			}
		}
		static MotionPackReader()
		{
			MotionPackReader.motionPackReadLock = false;
			DanceMotionFactoryFromMVH.RegisterSerializer();
		}
		public static MotionPack LoadMotionPackFromMoPkg(Stream stream)
		{
			MotionPack motionPack = new MotionPack();
			using (BinaryReader binaryReader = new BinaryReader(stream))
            {
                Balthasar.IO.PackageLang pl = null;
				if (MotionPackReader.LoadHeader(ref motionPack, binaryReader,ref pl))
				{
					long num = (long)((ulong)binaryReader.ReadUInt32());
					if (motionPack.CompressMode == 1)
					{
						binaryReader.ReadUInt32();
						byte[] array = binaryReader.ReadBytes((int)(num - 4L));
						MemoryStream memoryStream = MotionPackReader.decompress(array);
						array = memoryStream.ToArray();
						using (MemoryStream memoryStream2 = new MemoryStream(array))
						{
							using (BinaryReader binaryReader2 = new BinaryReader(memoryStream2))
							{
                                MotionPackReader.LoadBody(ref motionPack, binaryReader2, ref pl);
							}
						}
					}
				}
			}
			return motionPack;
		}
		protected static MotionPack LoadMotionPackFromDMpkg(MotionPackUserInfo ui, Stream streamHeader, Stream streamContent)
		{
			MotionPack motionPack = new MotionPack(ui);
            bool flag = false;
            Balthasar.IO.PackageLang pl = null;
			using (BinaryReader binaryReader = new BinaryReader(streamHeader))
			{
				flag = MotionPackReader.LoadHeader(ref motionPack, binaryReader,ref pl);
			}
			if (flag)
			{
				using (BinaryReader binaryReader2 = new BinaryReader(streamContent))
				{
					long num = (long)((ulong)binaryReader2.ReadUInt32());
					if (motionPack.CompressMode == 1)
					{
						binaryReader2.ReadUInt32();
						byte[] array = binaryReader2.ReadBytes((int)(num - 4L));
						MemoryStream memoryStream = MotionPackReader.decompress(array);
						array = memoryStream.ToArray();
						using (MemoryStream memoryStream2 = new MemoryStream(array))
						{
							using (BinaryReader binaryReader3 = new BinaryReader(memoryStream2))
							{
								MotionPackReader.LoadBody(ref motionPack, binaryReader3,ref pl);
							}
						}
					}
				}
			}
			return motionPack;
		}
		public static bool LoadHeader(ref MotionPack pack, BinaryReader br, ref Balthasar.IO.PackageLang pl)
		{
			float num = 1.1f;
			byte[] array = br.ReadBytes(5);
			pack.PackNames.Clear();
			pack.PackDescriptions.Clear();
			pack.Copyrights.Clear();
			pack.FormatVersion = br.ReadSingle();
			if (pack.FormatVersion > num)
			{
                //高版本允许加载
				//MoostaErrorManager.ShowErrorDialog(MoostaUIResourceManager.GetText("このバージョンのモーションパックには対応していません。"));
				//return false;
			}
			if (MotionPackReader.motionPackReadLock && array[0] == 77 && (double)pack.FormatVersion == 1.0)
            {
                //锁定版本允许加载允许加载
	//			MoostaErrorManager.ShowErrorDialog(MoostaUIResourceManager.GetText("このバージョンのモーションパックには対応していません。"));
	//			return false;
			}
			pack.PackVersion = br.ReadSingle();
			br.ReadInt32();
			int num2 = br.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				int id = br.ReadInt32();
				string name = ModelReader.ReadString(br);
                if (pl == null) pl = new Balthasar.IO.PackageLang(name);
				pack.PackNames.Add(new ModelStringData(id, pl.GetString(id,name)));
			}
			int num3 = br.ReadInt32();
			for (int j = 0; j < num3; j++)
			{
				int id2 = br.ReadInt32();
				string name2 = ModelReader.ReadString(br);
				pack.PackDescriptions.Add(new ModelStringData(id2, pl.GetString(id2,name2)));
			}
			int num4 = br.ReadInt32();
			for (int k = 0; k < num4; k++)
			{
				pack.Copyrights.Add(ModelReader.ReadString(br));
			}
			pack.IsRemovable = br.ReadBoolean();
			int num5 = br.ReadInt32();
			int num6 = br.ReadInt32();
			if (num5 * num6 > 0)
			{
				br.ReadBytes(num5 * num6 * 4);
			}
			pack.CompressMode = br.ReadByte();
			int num7 = br.ReadInt32();
			if (num7 > 0)
			{
				br.ReadBytes(num7);
			}
			return true;
		}
        public static void LoadBody(ref MotionPack pack, BinaryReader br, ref Balthasar.IO.PackageLang pl)
		{
			pack.Motions.Clear();
			while (br.PeekChar() > -1)
			{
				MotionPackContainerID motionPackContainerID = (MotionPackContainerID)br.ReadInt32();
				int num = br.ReadInt32();
				switch (motionPackContainerID)
				{
				case MotionPackContainerID.MVH:
					pack.Motions.Add(MotionPackReader.readMVHContainer(br, num,ref pl));
					break;
				case MotionPackContainerID.RepresentNames:
					MotionPackReader.readMotionRepresentnameContainer(br, ref pack,ref pl);
					break;
				case MotionPackContainerID.AnalyzeInformation:
					MotionPackReader.readAnalyzeInformationContainer(br, ref pack);
					break;
				default:
					br.BaseStream.Seek((long)num, SeekOrigin.Current);
					break;
				}
			}
			int num2 = 0;
			foreach (IDanceMotionLeaf current in pack.Motions)
			{
				current.IndexInMotionPack = num2;
				current.ParentMotionPack = pack;
				num2++;
			}
		}
        private static IDanceMotionLeaf readMVHContainer(BinaryReader br, int length, ref Balthasar.IO.PackageLang pl)
		{
			byte[] buffer = br.ReadBytes(length);
			MemoryStream stream = new MemoryStream(buffer);
			object obj = Serialization.Read(stream);
            DanceMotion ret = obj as DanceMotion;
            return ret;
		}
        private static void readMotionRepresentnameContainer(BinaryReader br, ref MotionPack pack, ref Balthasar.IO.PackageLang pl)
		{
			string value = ModelReader.ReadString(br);
			IDanceMotionLeaf danceMotionLeaf = null;
			foreach (IDanceMotionLeaf current in pack.Motions)
			{
				if (current.Name.Equals(value))
				{
					danceMotionLeaf = current;
					break;
				}
			}
			int num = br.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int langId = br.ReadInt32();
				string name = ModelReader.ReadString(br);
				if (danceMotionLeaf != null)
				{
                    danceMotionLeaf.SetRepresentName((MchaLanguageID)langId, pl.GetString(langId, name));
				}
			}
		}
		private static void readAnalyzeInformationContainer(BinaryReader br, ref MotionPack pack)
		{
			string value = ModelReader.ReadString(br);
			IDanceMotionLeaf danceMotionLeaf = null;
			foreach (IDanceMotionLeaf current in pack.Motions)
			{
				if (current.Name.Equals(value))
				{
					danceMotionLeaf = current;
					break;
				}
			}
			DanceMotionInformation danceMotionInformation = new DanceMotionInformation(br, danceMotionLeaf);
			if (danceMotionLeaf != null)
			{
				danceMotionLeaf.SetDanceMotionInformation(danceMotionInformation);
			}
		}
		private static MemoryStream decompress(byte[] bytes)
		{
			byte[] array = new byte[1024];
			MemoryStream memoryStream = new MemoryStream(bytes);
			GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
			MemoryStream memoryStream2 = new MemoryStream();
			using (memoryStream)
			{
				using (memoryStream2)
				{
					using (gZipStream)
					{
						int count;
						while ((count = gZipStream.Read(array, 0, array.Length)) > 0)
						{
							memoryStream2.Write(array, 0, count);
						}
					}
				}
			}
			return memoryStream2;
		}
	}
}
