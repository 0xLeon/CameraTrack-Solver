using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MathNet.Numerics.LinearAlgebra;

namespace CmdCameraTrackSolver
{
	public class ChanFrame : IComparable<ChanFrame>
	{
		private uint frame;
		private Vector<double> position;
		private Vector<double> rotation;

		public ChanFrame(uint frame, double[] position, double[] rotation)
		{
			this.frame = frame;
			this.position = Vector<double>.Build.DenseOfArray(position);
			this.rotation = Vector<double>.Build.DenseOfArray(rotation);
		}

		public int CompareTo(ChanFrame other)
		{
			if (null == other)
			{
				return 1;
			}

			return this.frame.CompareTo(other.Frame);
		}

		public override string ToString()
		{
			return this.Frame.ToString() + "\t" + this.position[0].ToString() + "\t" + this.position[1].ToString() + "\t" + this.position[2].ToString() + "\t" + this.rotation[0].ToString() + "\t" + this.rotation[1].ToString() + "\t" + this.rotation[2].ToString() + "\t0.0";
		}

		public uint Frame
		{
			get
			{
				return this.frame;
			}
		}

		public class Comparer : IComparer<ChanFrame>
		{
			public int Compare(ChanFrame frameA, ChanFrame frameB)
			{
				return frameA.Frame.CompareTo(frameB.Frame);
			}
		}
	}
}
