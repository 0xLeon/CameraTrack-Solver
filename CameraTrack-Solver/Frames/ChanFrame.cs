using System;
using System.Collections.Generic;

namespace CameraTrackSolver.Frames
{
	public class ChanFrame : IComparable<ChanFrame>
	{
		private uint frame;
		private double[] position;
		private double[] rotation;

		public ChanFrame(uint frame, ref double[] position, ref double[] rotation)
		{
			this.frame = frame;
			this.position = new double[position.Length];
			this.rotation = new double[rotation.Length];

			Array.Copy(position, this.position, this.position.Length);
			Array.Copy(rotation, this.rotation, this.rotation.Length);
		}

		public ChanFrame(uint frame, SolverFrame solverFrame)
		{
			this.frame = frame;
			this.position = new double[solverFrame.Position.Length];
			this.rotation = new double[solverFrame.Rotation.Length];

			Array.Copy(solverFrame.Position, this.position, this.position.Length);
			Array.Copy(solverFrame.Rotation, this.rotation, this.rotation.Length);
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
			return this.Frame.ToString() + "\t" + this.position[0].ToString() + "\t" + this.position[1].ToString() + "\t" + this.position[2].ToString() + "\t" + this.rotation[0].ToString() + "\t" + this.rotation[1].ToString() + "\t" + this.rotation[2].ToString();
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
