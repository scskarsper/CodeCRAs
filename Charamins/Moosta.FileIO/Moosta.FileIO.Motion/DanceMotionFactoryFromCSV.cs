using CTUtilCLR;
using Moosta.Common;
using Moosta.Common.DanceMotion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Moosta.FileIO.Motion
{
	public class DanceMotionFactoryFromCSV : IDanceMotionFactory
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
					".csv"
				};
			}
		}
		public DanceMotionFactoryFromCSV()
		{
			this.fileName = "";
		}
		public DanceMotionFactoryFromCSV(string filename)
		{
			this.fileName = filename;
		}
		public IDanceMotion CreateDanceMotion(string[] bones)
		{
			IDanceMotion result;
			using (StreamReader streamReader = new StreamReader(this.fileName))
			{
				result = DanceMotionFactoryFromCSV.ReadMotion(streamReader, Path.GetFileNameWithoutExtension(this.fileName));
			}
			return result;
		}
		private static DanceMotion ReadMotion(TextReader reader, string name)
		{
			string[] array = DanceMotionFactoryFromCSV.SplitByComma(reader.ReadLine());
			int beatCount = int.Parse(array[0]);
			string[] tags = array.Skip(1).ToArray<string>();
			string[] bones = reader.ReadLine().Split(new char[]
			{
				','
			}, StringSplitOptions.RemoveEmptyEntries);
			List<DancePose> list = new List<DancePose>();
			for (string text = reader.ReadLine(); text != null; text = reader.ReadLine())
			{
				try
				{
					BoneState[] items = (
						from str in text.Split(new char[]
						{
							','
						}, StringSplitOptions.RemoveEmptyEntries)
						select str.Split(new char[]
						{
							' '
						}, StringSplitOptions.RemoveEmptyEntries) into tokens
						select Array.ConvertAll<string, float>(tokens, (string s) => float.Parse(s)) into value
						select new CtQuaternion(value[0], value[1], value[2], value[3]) into quaternion
						select new BoneState(quaternion)).ToArray<BoneState>();
					list.Add(new DancePose(bones, items));
				}
				catch
				{
				}
			}
			return new DanceMotion(name, beatCount, list.ToArray(), null)
			{
				Tags = tags
			};
		}
		private static string[] SplitByComma(string line)
		{
			return line.Split(new char[]
			{
				','
			}, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
