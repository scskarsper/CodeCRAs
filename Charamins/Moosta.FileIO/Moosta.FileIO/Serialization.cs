using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace Moosta.FileIO
{
	public static class Serialization
	{
		private class WriteAction
		{
			private object _target;
			private ISerializer _serializer;
			public void Execute(BinaryWriter writer, IWritingSession session)
			{
				using (writer.BeginMoostaBlock())
				{
					try
					{
						writer.Write(this._serializer.TargetGuid.ToByteArray());
						writer.Write(this._serializer.FormatVersion);
						writer.Write(session.GetKey(this._target));
						this._serializer.Write(writer, this._target, session);
					}
					catch (Exception)
					{
					}
				}
			}
			public static Serialization.WriteAction[] GetDependences(object target)
			{
				HashSet<Serialization.WriteAction> hash = new HashSet<Serialization.WriteAction>();
				return (
					from w in Serialization.WriteAction.GetDependencesPrivate(target)
					where hash.Add(w)
					select w).ToArray<Serialization.WriteAction>();
			}
			private static IEnumerable<Serialization.WriteAction> GetDependencesPrivate(object target)
			{
				ISerializer[] array;
				if (target != null && Serialization._type2serializers.TryGetValue(target.GetType(), out array))
				{
					ISerializer serializer = array[array.Length - 1];
					return serializer.GetDependencies(target).SelectMany((object obj) => Serialization.WriteAction.GetDependencesPrivate(obj)).Concat(new Serialization.WriteAction[]
					{
						new Serialization.WriteAction
						{
							_target = target,
							_serializer = serializer
						}
					});
				}
				return Enumerable.Empty<Serialization.WriteAction>();
			}
		}
		private class WritingSession : IWritingSession
		{
			private readonly Dictionary<object, int> _dic = new Dictionary<object, int>();
			private int _lastKey;
			public int GetKey(object obj)
			{
				int num;
				if (!this._dic.TryGetValue(obj, out num))
				{
					num = ++this._lastKey;
					this._dic.Add(obj, num);
				}
				return num;
			}
		}
		private class ReadingSession : IReadingSession
		{
			private readonly Dictionary<int, object> _dic = new Dictionary<int, object>();
			public object GetObject(int key)
			{
				object result;
				if (!this._dic.TryGetValue(key, out result))
				{
					return null;
				}
				return result;
			}
			public void AddObject(int key, object obj)
			{
				this._dic.Add(key, obj);
			}
		}
		private class Disposable : IDisposable
		{
			private readonly Action _dispose;
			public Disposable(Action dispose)
			{
				this._dispose = dispose;
			}
			public void Dispose()
			{
				this._dispose();
			}
		}
		private static readonly Guid FORMAT_KEY = new Guid("01b8b564-738d-4394-97ff-9702a04091a1");
		private static readonly Dictionary<Type, ISerializer[]> _type2serializers = new Dictionary<Type, ISerializer[]>();
		private static readonly Dictionary<Guid, ISerializer[]> _guid2serializers = new Dictionary<Guid, ISerializer[]>();
		public static void Write(string filename, object target)
		{
			using (FileStream fileStream = File.OpenWrite(filename))
			{
				Serialization.Write(fileStream, target);
			}
		}
		public static void Write(Stream stream, object target)
		{
			Serialization.WritingSession session = new Serialization.WritingSession();
			Serialization.WriteAction[] dependences = Serialization.WriteAction.GetDependences(target);
			using (BinaryWriter binaryWriter = new BinaryWriter(stream))
			{
				binaryWriter.Write(Serialization.FORMAT_KEY.ToByteArray());
				binaryWriter.Write(dependences.Length);
				Serialization.WriteAction[] array = dependences;
				for (int i = 0; i < array.Length; i++)
				{
					Serialization.WriteAction writeAction = array[i];
					writeAction.Execute(binaryWriter, session);
				}
			}
		}
		public static object Read(string filename)
		{
			object result;
			using (FileStream fileStream = File.OpenRead(filename))
			{
				result = Serialization.Read(fileStream);
			}
			return result;
		}
		public static object Read(Stream stream)
		{
			Serialization.ReadingSession session = new Serialization.ReadingSession();
			object result;
			using (BinaryReader binaryReader = new BinaryReader(stream))
			{
				Guid a = new Guid(binaryReader.ReadBytes(16));
				if (a != Serialization.FORMAT_KEY)
				{
					throw new Exception("Unknown format");
				}
				int num = binaryReader.ReadInt32();
				object obj = null;
				for (int i = 0; i < num; i++)
				{
					obj = binaryReader.ReadMoostaObject(session);
				}
				result = obj;
			}
			return result;
		}
		public static void WriteMoostaString(this BinaryWriter writer, string str)
		{
			writer.WriteMoostaString(str, Encoding.UTF8);
		}
		public static string ReadMoostaString(this BinaryReader reader)
		{
			return reader.ReadMoostaString(Encoding.UTF8);
		}
		public static void WriteMoostaString(this BinaryWriter writer, string str, Encoding encoding)
		{
			if (string.IsNullOrEmpty(str))
			{
				writer.Write(0);
				return;
			}
			byte[] bytes = encoding.GetBytes(str);
			writer.Write(bytes.Length);
			writer.Write(bytes);
		}
		public static string ReadMoostaString(this BinaryReader reader, Encoding encoding)
		{
			int count = reader.ReadInt32();
			byte[] bytes = reader.ReadBytes(count);
			return encoding.GetString(bytes);
		}
		public static void WriteMoostaObject(this BinaryWriter writer, object obj, IWritingSession session)
		{
			using (writer.BeginMoostaBlock())
			{
				ISerializer[] array;
				if (obj != null && Serialization._type2serializers.TryGetValue(obj.GetType(), out array))
				{
					try
					{
						ISerializer serializer = array[array.Length - 1];
						writer.Write(serializer.TargetGuid.ToByteArray());
						writer.Write(serializer.FormatVersion);
						writer.Write(session.GetKey(obj));
						serializer.Write(writer, obj, session);
					}
					catch (Exception)
					{
					}
				}
			}
		}
		public static object ReadMoostaObject(this BinaryReader reader, IReadingSession session)
		{
			object result;
			using (reader.BeginMoostaBlock())
			{
				try
				{
					Guid key = new Guid(reader.ReadBytes(16));
					byte version = reader.ReadByte();
					int key2 = reader.ReadInt32();
					ISerializer[] array;
					if (Serialization._guid2serializers.TryGetValue(key, out array))
					{
						ISerializer serializer = array.FirstOrDefault((ISerializer s) => s.FormatVersion == version);
						if (serializer == null)
						{
							serializer = array[array.Length - 1];
						}
						object obj = serializer.Read(reader, session);
						((Serialization.ReadingSession)session).AddObject(key2, obj);
						result = obj;
					}
					else
					{
						result = null;
					}
				}
				catch (Exception)
				{
					result = null;
				}
			}
			return result;
		}
		public static IDisposable BeginMoostaBlock(this BinaryWriter writer)
		{
			long pos = writer.BaseStream.Position;
			writer.Write(0);
			return new Serialization.Disposable(delegate
			{
				int num = (int)(writer.BaseStream.Position - pos);
				writer.Seek(-num, SeekOrigin.Current);
				writer.Write(num);
				writer.Seek(num - 4, SeekOrigin.Current);
			});
		}
		public static IDisposable BeginMoostaBlock(this BinaryReader reader)
		{
			long pos = reader.BaseStream.Position;
			int size = reader.ReadInt32();
			return new Serialization.Disposable(delegate
			{
				reader.BaseStream.Seek(pos + (long)size - reader.BaseStream.Position, SeekOrigin.Current);
			});
		}
		public static void Register(ISerializer ser)
		{
			ISerializer[] first;
			if (Serialization._type2serializers.TryGetValue(ser.TargetType, out first))
			{
				Serialization._type2serializers[ser.TargetType] = (
					from s in first.Concat(new ISerializer[]
					{
						ser
					})
					orderby s.FormatVersion
					select s).ToArray<ISerializer>();
			}
			else
			{
				Serialization._type2serializers.Add(ser.TargetType, new ISerializer[]
				{
					ser
				});
			}
			if (Serialization._guid2serializers.TryGetValue(ser.TargetGuid, out first))
			{
				Serialization._guid2serializers[ser.TargetGuid] = (
					from s in first.Concat(new ISerializer[]
					{
						ser
					})
					orderby s.FormatVersion
					select s).ToArray<ISerializer>();
				return;
			}
			Serialization._guid2serializers.Add(ser.TargetGuid, new ISerializer[]
			{
				ser
			});
		}
	}
}
