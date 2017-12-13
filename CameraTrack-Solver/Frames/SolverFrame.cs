using System;
using System.Collections.Generic;

namespace CameraTrackSolver.Frames
{
	public class SolverFrame : IComparable<SolverFrame>
	{
		private ulong timestamp;
		private double[] position;
		private double[] rotation;

		public SolverFrame(ulong timestamp, ref double[] position, ref double[] rotation)
		{
			this.timestamp = timestamp;
			this.position = new double[position.Length];
			this.rotation = new double[rotation.Length];

			Array.Copy(position, this.position, this.position.Length);
			Array.Copy(rotation, this.rotation, this.rotation.Length);
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

		public double[] Position
		{
			get
			{
				return this.position;
			}
		}

		public double[] Rotation
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
