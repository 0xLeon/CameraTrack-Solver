using System.Collections.Generic;
using System.IO;
using System.Text;

using CameraTrackSolver.Frames;

namespace CameraTrackSolver.Parsers
{
	public class TrackFileParser
	{
		private string filepath;
		private CameraTrackerType trackFileType;
		private SortedList<ulong, SensorEvent> sensorData;
		private bool parsed = false;

		public TrackFileParser(string filepath)
		{
			this.filepath = filepath;
			this.sensorData = new SortedList<ulong, SensorEvent>(new SensorEvent.TimestampComparer());
		}

		public bool Parse()
		{
			StreamReader sr = new StreamReader(this.filepath, new UTF8Encoding(false));
			SensorEvent sensorEvent = null;

			this.trackFileType = (CameraTrackerType) uint.Parse(sr.ReadLine().Trim());

			while (!sr.EndOfStream)
			{
				sensorEvent = SensorEvent.FromTrackLine(sr.ReadLine());
				this.sensorData.Add(sensorEvent.Timestamp, sensorEvent);
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

		public SortedList<ulong, SensorEvent> SensorData
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
			RL_DEFAULT,
			GL_DEFAULT,
			GAM_DEFAULT,
			GAM_NATIVE,
		}
	}
}
