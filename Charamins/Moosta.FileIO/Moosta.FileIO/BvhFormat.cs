using CTUtilCLR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Moosta.FileIO
{
	public static class BvhFormat
	{
		public static BvhMotion Import(string filename)
		{
			BvhMotion result;
			using (StreamReader streamReader = new StreamReader(filename))
			{
				result = BvhFormat.Import(streamReader);
			}
			return result;
		}
		public static BvhMotion Import(TextReader reader)
		{
			BvhNode root = null;
			if (reader.ReadLine().Trim() == "HIERARCHY")
			{
				string[] array = reader.ReadLine().Trim().Split(new char[]
				{
					' ',
					'\t'
				}, StringSplitOptions.RemoveEmptyEntries);
				if (array[0] == "ROOT")
				{
					root = BvhFormat.ReadNode(array[1], reader);
				}
			}
			if (reader.ReadLine().Trim() == "MOTION")
			{
				int num = int.Parse(reader.ReadLine().Split(new char[]
				{
					':'
				})[1].Trim());
				double spf = double.Parse(reader.ReadLine().Split(new char[]
				{
					':'
				})[1].Trim());
				double[][] array2 = new double[num][];
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = (
						from token in reader.ReadLine().Split(new char[]
						{
							' ',
							'\t'
						}, StringSplitOptions.RemoveEmptyEntries)
						select double.Parse(token)).ToArray<double>();
				}
				return new BvhMotion(root, spf, array2);
			}
			return null;
		}
		private static BvhNode ReadNode(string name, TextReader reader)
		{
			CtVector3D zero = CtVector3D.Zero;
			List<BvhChannels> list = new List<BvhChannels>();
			List<BvhNode> list2 = new List<BvhNode>();
			if (reader.ReadLine().Trim() != "{")
			{
				throw new Exception();
			}
			while (true)
			{
				string[] array = reader.ReadLine().Trim().Split(new char[]
				{
					' ',
					'\t'
				}, StringSplitOptions.RemoveEmptyEntries);
				if (array[0] == "}")
				{
					break;
				}
				if (array[0] == "OFFSET")
				{
					zero = new CtVector3D(float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
				}
				else if (array[0] == "CHANNELS")
				{
					for (int i = 2; i < array.Length; i++)
					{
						list.Add((BvhChannels)Enum.Parse(typeof(BvhChannels), array[i]));
					}
				}
				else if (array[0] == "JOINT" || array[0] == "End")
				{
					list2.Add(BvhFormat.ReadNode(array[1], reader));
				}
			}
			return new BvhNode(name, zero, list.ToArray(), list2.ToArray());
		}
	}
}
