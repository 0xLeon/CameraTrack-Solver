using System;
using System.Collections.Generic;

namespace CameraTrackSolver.Frames
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
		private double[] data;

		public static SensorEvent FromTrackLine(string line)
		{
			string[] dataFields = line.Trim().Replace(',', '.').Split('\t');

			ulong dfTimestamp = ulong.Parse(dataFields[0]);
			int dfType = int.Parse(dataFields[1]);
			double[] dfData = new double[] {
				double.Parse(dataFields[2]),
				double.Parse(dataFields[3]),
				double.Parse(dataFields[4])
			};

			return new SensorEvent(dfTimestamp, (SensorType) dfType, ref dfData);
		}

		public SensorEvent(ulong timestamp, SensorType type, ref double[] data)
		{
			this.timestamp = timestamp;
			this.type = type;
			this.data = new double[data.Length];

			Array.Copy(data, this.data, this.data.Length);
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

		public double[] Data
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

		public class TimestampComparer : IComparer<ulong>
		{
			public int Compare(ulong a, ulong b)
			{
				int r = a.CompareTo(b);

				if (0 == r)
				{
					return 1;
				}

				return r;
			}
		}
	}
}
