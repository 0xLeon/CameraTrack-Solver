using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using CameraTrackSolver.Frames;
using CameraTrackSolver.Parsers;
using CameraTrackSolver.Files;
using CameraTrackSolver.Exceptions;

namespace CameraTrackSolver.Solvers
{
	abstract class AbstractTrackSolver : ICameraTrackSolver
	{
		public const double ns2s = 1e-9;
		public const double sqNs2s = 1e-18;
		public const double rad2grad = 57.29577951308232;
		public const double piHalf = 0.5 * Math.PI;

		protected SortedList<ulong, SensorEvent> sensorData;
		protected Dictionary<ulong, SolverFrame> solverData;
		protected HashSet<ChanFrame> chanFrames;
		protected double fps;
		protected double nsPerFrame;
		protected bool solved = false;

		protected ulong firstTimestamp;

		public bool Solved
		{
			get
			{
				return this.solved;
			}
		}

		public AbstractTrackSolver(TrackFileParser tfParser, double fps)
		{
			if (!tfParser.Parsed)
			{
				tfParser.Parse();
			}

			if (Array.IndexOf(this.GetSupportedTrackFileTypes(), tfParser.TrackFileType) == -1)
			{
				throw new CameraTrackException("Incompatible track file type");
			}

			this.sensorData = tfParser.SensorData;
			this.solverData = new Dictionary<ulong, SolverFrame>(tfParser.SensorData.Count / 2);
			this.chanFrames = new HashSet<ChanFrame>();
			this.fps = fps;
			this.nsPerFrame = 1e9 / this.fps;
			this.firstTimestamp = this.sensorData.Values[0].Timestamp;
		}

		public virtual void WriteChanFile(string filepath)
		{
			if (!this.solved)
			{
				new CameraTrackException("Didn't solve camera movement from sensor data.");
			}

			ChanFile chanFile = new ChanFile(filepath);
			chanFile.AddFrames(this.chanFrames);
			chanFile.WriteFile();
		}

		public virtual void WriteSensorDataFiles(string filepath)
		{
			string filenameTemplate = filepath.Substring(0, filepath.Length - 5) + ".{0}.csv";
			UTF8Encoding utf8 = new UTF8Encoding(false, false);
			Dictionary<SensorEvent.SensorType, StreamWriter> files = new Dictionary<SensorEvent.SensorType, StreamWriter>();
			StreamWriter currentFile;

			foreach (KeyValuePair<ulong, SensorEvent> sensorEvent in this.sensorData)
			{
				if (!files.TryGetValue(sensorEvent.Value.Type, out currentFile))
				{
					string path = string.Format(filenameTemplate, sensorEvent.Value.Type.ToString());

					if (File.Exists(path))
					{
						File.Delete(path);

						if (File.Exists(path))
						{
							throw new IOException("Sensor data file not writable");
						}
					}

					currentFile = new StreamWriter(path, false, utf8);

					files.Add(sensorEvent.Value.Type, currentFile);
				}

				currentFile.Write(string.Format("{0},{1},{2}\n", sensorEvent.Value.X, sensorEvent.Value.Y, sensorEvent.Value.Z));
			}

			foreach (KeyValuePair<SensorEvent.SensorType, StreamWriter> file in files)
			{
				file.Value.Flush();
				file.Value.Close();
			}
		}

		public virtual void Solve()
		{
			this.ConsumeSensorData();
			this.SolveFromSolverData();

			this.solved = true;
		}

		protected virtual void ConsumeSensorData()
		{
			foreach (KeyValuePair<ulong, SensorEvent> sensorEvent in this.sensorData)
			{
				this.HandleSensorEvent(sensorEvent.Value);
			}
		}

		protected virtual void SolveFromSolverData()
		{
			uint frameCounter = 0;
			ulong currentTimestamp = 0;
			ulong workingTimestamp = 0;
			ulong limitTimestamp = 0;
			ulong timestampRange = Convert.ToUInt64(this.nsPerFrame * 0.5);
			ulong nsPerFrame = Convert.ToUInt64(Math.Floor(this.nsPerFrame));
			SolverFrame currentSolverFrame = null;
			SolverFrame maxSolverFrame = this.solverData.Last().Value;

			while (currentTimestamp < maxSolverFrame.Timestamp)
			{
				if (!this.solverData.TryGetValue(currentTimestamp, out currentSolverFrame))
				{
					workingTimestamp = currentTimestamp + 1;
					limitTimestamp = currentTimestamp + timestampRange;

					while (workingTimestamp < limitTimestamp)
					{
						if (this.solverData.TryGetValue(workingTimestamp, out currentSolverFrame))
						{
							goto found;
						}

						workingTimestamp++;
					}
					found:;

					if (null == currentSolverFrame)
					{
						throw new CameraTrackException("Unable to solve frame " + (frameCounter + 1).ToString());
					}
				}

				this.chanFrames.Add(new ChanFrame(frameCounter + 1, currentSolverFrame));


				frameCounter++;
				currentTimestamp += nsPerFrame;
				currentSolverFrame = null;
			}
		}

		public abstract TrackFileParser.CameraTrackerType[] GetSupportedTrackFileTypes();

		protected abstract void HandleSensorEvent(SensorEvent sensorEvent);

		protected abstract void PushSolverFrame(SensorEvent sensorEvent);
	}
}
