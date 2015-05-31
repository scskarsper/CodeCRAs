using System;
using System.Collections;
using System.Collections.Generic;
namespace Moosta.FileIO
{
	public class BvhMotion : IEnumerable<double[]>, IEnumerable
	{
		public readonly BvhNode RootNode;
		public readonly double SecondsPerFrame;
		private readonly double[][] _poses;
		public double[] this[int frameIndex]
		{
			get
			{
				return this._poses[frameIndex];
			}
		}
		public int Count
		{
			get
			{
				return this._poses.Length;
			}
		}
		public BvhMotion(BvhNode root, double spf, double[][] poses)
		{
			this.RootNode = root;
			this.SecondsPerFrame = spf;
			this._poses = poses;
		}
		public IEnumerator<double[]> GetEnumerator()
		{
			return ((IEnumerable<double[]>)this._poses).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._poses.GetEnumerator();
		}
	}
}
