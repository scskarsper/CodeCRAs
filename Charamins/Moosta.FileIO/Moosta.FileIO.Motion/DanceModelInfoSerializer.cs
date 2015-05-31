using CTUtilCLR;
using Moosta.Common.DanceMotion;
using System;
using System.Collections.Generic;
using System.IO;
namespace Moosta.FileIO.Motion
{
	internal class DanceModelInfoSerializer : AbstractSerializer<DanceModelInfo>
	{
		public override Guid TargetGuid
		{
			get
			{
				return new Guid("90b9a686-002e-436b-9171-dc10977cee02");
			}
		}
		protected override void WriteTarget(BinaryWriter writer, DanceModelInfo target, IWritingSession session)
		{
			writer.Write(target.ModelName);
			writer.Write(target.BonePositions.Count);
			foreach (KeyValuePair<string, CtVector3D> current in target.BonePositions)
			{
				writer.Write(current.Key);
				writer.Write(current.Value.X);
				writer.Write(current.Value.Y);
				writer.Write(current.Value.Z);
			}
		}
		protected override DanceModelInfo ReadTarget(BinaryReader reader, IReadingSession session)
		{
			DanceModelInfo danceModelInfo = new DanceModelInfo(reader.ReadString());
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string key = reader.ReadString();
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				float z = reader.ReadSingle();
				danceModelInfo.BonePositions.Add(key, new CtVector3D(x, y, z));
			}
			return danceModelInfo;
		}
	}
}
