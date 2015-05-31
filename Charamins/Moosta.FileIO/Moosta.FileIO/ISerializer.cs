using System;
using System.Collections.Generic;
using System.IO;
namespace Moosta.FileIO
{
	public interface ISerializer
	{
		Type TargetType
		{
			get;
		}
		Guid TargetGuid
		{
			get;
		}
		byte FormatVersion
		{
			get;
		}
		IEnumerable<object> GetDependencies(object target);
		void Write(BinaryWriter writer, object target, IWritingSession session);
		object Read(BinaryReader reader, IReadingSession session);
	}
}
