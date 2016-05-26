using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdCameraTrackSolver
{
	class SolverFrame : IComparable<SolverFrame>
	{
		private ulong timestamp;
		private Vector<double> position;
		private Vector<double> rotation;

		public SolverFrame(ulong timestamp, double[] position, double[] rotation)
		{
			this.timestamp = timestamp;
			this.position = Vector<double>.Build.DenseOfArray(position);
			this.rotation = Vector<double>.Build.DenseOfArray(rotation);
		}

		public int CompareTo(SolverFrame other)
		{
			if (null == other)
			{
				return 1;
			}

			return this.Timestamp.CompareTo(other.Timestamp);
		}

		public ulong Timestamp
		{
			get
			{
				return this.timestamp;
			}
		}

		public Vector<double> Position
		{
			get
			{
				return this.position;
			}
		}

		public Vector<double> Rotation
		{
			get
			{
				return this.rotation;
			}
		}

		public class Comparer : IComparer<SolverFrame>
		{
			public int Compare(SolverFrame frameA, SolverFrame frameB)
			{
				return frameA.Timestamp.CompareTo(frameB.Timestamp);
			}
		}
	}
}
