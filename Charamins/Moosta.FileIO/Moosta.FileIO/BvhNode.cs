using CTUtilCLR;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Moosta.FileIO
{
	public class BvhNode
	{
		public readonly string Name;
		public readonly CtVector3D Offset;
		public readonly BvhChannels[] Channels;
		public readonly BvhNode[] Nodes;
		public BvhNode(string name, CtVector3D offset, BvhChannels[] channels, BvhNode[] nodes)
		{
			this.Name = name;
			this.Offset = offset;
			this.Channels = channels;
			this.Nodes = nodes;
		}
		public IEnumerable<BvhNode> GetAllNodesRecursively()
		{
			return new BvhNode[]
			{
				this
			}.Concat(this.Nodes.SelectMany((BvhNode n) => n.GetAllNodesRecursively()));
		}
	}
}
