using System;
using System.Collections.Generic;
namespace PartVision.Standard
{
	public class PVPartBatch
	{
		public PVPartBatch(List<byte[]> batch = null)
		{
			Images = batch ?? new List<byte[]>();
		}

		public List<byte[]> Images { get; }
	}
}
