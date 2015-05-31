using System;
using System.Collections.Generic;
using System.IO;
namespace Moosta.FileIO
{
	public abstract class AbstractSerializer<T> : ISerializer
	{
		public Type TargetType
		{
			get
			{
				return typeof(T);
			}
		}
		public abstract Guid TargetGuid
		{
			get;
		}
		public virtual byte FormatVersion
		{
			get
			{
				return 0;
			}
		}
		public IEnumerable<object> GetDependencies(object target)
		{
			return this.GetDependencies((T)((object)target));
		}
		public void Write(BinaryWriter writer, object target, IWritingSession session)
		{
			this.WriteTarget(writer, (T)((object)target), session);
		}
		public object Read(BinaryReader reader, IReadingSession session)
		{
			return this.ReadTarget(reader, session);
		}
		protected virtual IEnumerable<object> GetDependencies(T target)
		{
			yield break;
		}
		protected abstract void WriteTarget(BinaryWriter writer, T target, IWritingSession session);
		protected abstract T ReadTarget(BinaryReader reader, IReadingSession session);
	}
}
