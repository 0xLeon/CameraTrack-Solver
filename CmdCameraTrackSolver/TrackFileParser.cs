using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdCameraTrackSolver
{
	public class TrackFileParser
	{
		private string filepath;
		private CameraTrackerType trackFileType;
		private SortedSet<SensorEvent> sensorData;
		private bool parsed = false;

		public TrackFileParser(string filepath)
		{
			this.filepath = filepath;
			this.sensorData = new SortedSet<SensorEvent>(new SensorEvent.Comparer());
		}

		public bool Parse()
		{
			StreamReader sr = new StreamReader(this.filepath, new UTF8Encoding(false));

			this.trackFileType = (CameraTrackerType) uint.Parse(sr.ReadLine().Trim());

			while (!sr.EndOfStream)
			{
				this.sensorData.Add(SensorEvent.FromTrackLine(sr.ReadLine()));
			}

			this.parsed = true;

			return true;
		}

		public string Filepath
		{
			get
			{
				return this.filepath;
			}
		}

		public CameraTrackerType TrackFileType
		{
			get
			{
				return this.trackFileType;
			}
		}

		public SortedSet<SensorEvent> SensorData
		{
			get
			{
				return this.sensorData;
			}
		}

		public bool Parsed
		{
			get
			{
				return this.parsed;
			}
		}

		public enum CameraTrackerType
		{
			GAM_DEFAULT,
			GAM_NATIVE,
			RL_DEFAULT
		}
	}
}
