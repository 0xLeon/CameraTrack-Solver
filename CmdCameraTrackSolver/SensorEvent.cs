using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MathNet.Numerics.LinearAlgebra;

namespace CmdCameraTrackSolver
{
	public class SensorEvent : IComparable<SensorEvent>
	{
		public enum SensorType {
			ACCEL		= 1,
			MAGNETIC	= 2,
			GYRO		= 4,
			LIN_ACCEL	= 10,
			ROT_VEC		= 11,
		};

		private ulong timestamp;
		private SensorType type;
		private Vector<double> data;

		public static SensorEvent FromTrackLine(string line)
		{
			string[] dataFields = line.Trim().Split('\t');

			ulong dfTimestamp = ulong.Parse(dataFields[0]);
			int dfType = int.Parse(dataFields[1]);
			double[] dfData = new double[] { double.Parse(dataFields[2]), double.Parse(dataFields[3]), double.Parse(dataFields[4]) };

			return new SensorEvent(dfTimestamp, (SensorType) dfType, dfData);
		}

		public SensorEvent(ulong timestamp, SensorType type, double[] data)
		{
			this.timestamp = timestamp;
			this.type = type;
			this.data = Vector<double>.Build.DenseOfArray(data);
		}

		public int CompareTo(SensorEvent other)
		{
			if (null == other)
			{
				return 1;
			}

			return this.Timestamp.CompareTo(other.Timestamp);
		}

		public override string ToString()
		{
			return "SensorEvent / " + this.Timestamp.ToString() + " / " + this.Type.ToString() + " / " + this.data.ToString();
		}

		public ulong Timestamp
		{
			get
			{
				return this.timestamp;
			}
		}

		public SensorType Type
		{
			get
			{
				return this.type;
			}
		}

		public double X
		{
			get
			{
				return this.data[0];
			}
		}

		public double Y
		{
			get
			{
				return this.data[1];
			}
		}

		public double Z
		{
			get
			{
				return this.data[2];
			}
		}

		public Vector<double> Data
		{
			get
			{
				return this.data;
			}
		}

		public class Comparer : IComparer<SensorEvent>
		{
			public int Compare(SensorEvent eventA, SensorEvent eventB)
			{
				return eventA.Timestamp.CompareTo(eventB.Timestamp);
			}
		}
	}
}
