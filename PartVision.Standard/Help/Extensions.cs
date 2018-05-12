using System;
using System.IO;
using Plugin.Media.Abstractions;
using System.Threading.Tasks;
using System.Threading;

namespace PartVision.Standard
{
	public static partial class Extensions
	{
		public static string AsVisionTagId(this GestureCommand command)
		{
			switch (command)
			{
				case GestureCommand.Recognize:
					return "afb72e8a-a65f-45c1-ab73-3ce354cb0f4c";
				//return "recognize-gesture";
				case GestureCommand.Train:
					return "1b68998b-d2dd-471a-b749-3026657b2f05";
				//return "train-gesture";
				default:
					throw new Exception("Missing GestureCommand tag definition");
			}
		}
	}
}
