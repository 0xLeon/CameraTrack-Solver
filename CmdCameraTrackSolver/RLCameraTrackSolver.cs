using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdCameraTrackSolver
{
	class RLCameraTrackSolver : ICameraTrackSolver
	{
		private IEnumerable<SensorEvent> sensorData;
		private SortedList<ulong, SolverFrame> solverData;
		private SortedSet<ChanFrame> chanFrames;
		private double fps;
		private double nsPerFrame;

		private ulong firstTimestamp;

		private bool solved = false;

		public RLCameraTrackSolver(TrackFileParser tfParser, double fps)
		{
			if (!tfParser.Parsed)
			{
				tfParser.Parse();
			}

			if (TrackFileParser.CameraTrackerType.RL_DEFAULT != tfParser.TrackFileType)
			{
				throw new CameraTrackException("Incompatible track file type");
			}

			this.sensorData = tfParser.SensorData;
			this.solverData = new SortedList<ulong, SolverFrame>(tfParser.SensorData.Count / 2, null);
			this.chanFrames = new SortedSet<ChanFrame>(new ChanFrame.Comparer());
			this.fps = fps;
			this.firstTimestamp = this.sensorData.First().Timestamp;
		}

		public void WriteTrackFile(string filepath)
		{
			if (!this.solved)
			{
				new CameraTrackException("Didn't solve camera movement from sensor data.");
			}

			ChanFile chanFile = new ChanFile(filepath);
			chanFile.AddFrames(this.chanFrames);
			chanFile.WriteFile();
		}


		public void Solve()
		{
			this.ConsumeSensorData();
			this.SolveFromSolverData();

			this.solved = true;
		}

		private void ConsumeSensorData()
		{
			foreach (SensorEvent sensorEvent in this.sensorData)
			{
				switch (sensorEvent.Type)
				{
					case SensorEvent.SensorType.LIN_ACCEL:
						this.HandleLinAccelEvent(sensorEvent);
						break;
					case SensorEvent.SensorType.ROT_VEC:
						this.HandleRotVecEvent(sensorEvent);
						break;
				}
			}
		}

		private void SolveFromSolverData()
		{
			
		}

		private void HandleLinAccelEvent(SensorEvent sensorEvent)
		{

		}

		private void HandleRotVecEvent(SensorEvent sensorEvent)
		{

		}

		public bool Solved
		{
			get
			{
				return this.solved;
			}
		}
	}
}
