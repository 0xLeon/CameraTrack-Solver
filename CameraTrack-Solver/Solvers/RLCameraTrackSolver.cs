using System;

using CameraTrackSolver.Frames;
using CameraTrackSolver.Parsers;

namespace CameraTrackSolver.Solvers
{
	class RLCameraTrackSolver : AbstractTrackSolver
	{
		private const double tenthDegreeInRad = 0.0017453292519943296;
		
		private ulong lastLinAccelTimestamp = 0;
		
		private double[] lastPosition = new double[3];
		private double[] lastRotation = new double[3];

		private double positionScaleFactor;
		private double accumulatedPositionFactor;

		public double PositionScaleFactor
		{
			get
			{
				return this.positionScaleFactor;
			}
			set
			{
				this.positionScaleFactor = value;
				this.accumulatedPositionFactor = 0.5 * AbstractTrackSolver.sqNs2s * value;
			}
		}

		public RLCameraTrackSolver(TrackFileParser tfParser, double fps, double psFactor) : base(tfParser, fps)
		{
			this.PositionScaleFactor = psFactor;
		}

		public RLCameraTrackSolver(TrackFileParser tfParser, double fps) : this(tfParser, fps, 100.0) { }
		
		public override TrackFileParser.CameraTrackerType[] GetSupportedTrackFileTypes()
		{
			return new TrackFileParser.CameraTrackerType[] {
				TrackFileParser.CameraTrackerType.RL_DEFAULT,
			};
		}

		protected override void HandleSensorEvent(SensorEvent sensorEvent)
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

		private void HandleLinAccelEvent(SensorEvent sensorEvent)
		{
			if (0 == this.lastLinAccelTimestamp)
			{
				this.lastLinAccelTimestamp = sensorEvent.Timestamp;
				return;
			}
			// double deltaT = (sensorEvent.Timestamp - this.lastLinAccelTimestamp) * AbstractTrackSolver.ns2s;

			/*
			this.lastPosition[0] += -sensorEvent.Y * deltaT * deltaT * 0.5 * 100;
			this.lastPosition[1] +=  sensorEvent.Z * deltaT * deltaT * 0.5 * 100;
			this.lastPosition[2] += -sensorEvent.X * deltaT * deltaT * 0.5 * 100;
			*/

			ulong nsDeltaT = sensorEvent.Timestamp - this.lastLinAccelTimestamp;
			double sqDeltaT = nsDeltaT * nsDeltaT * this.accumulatedPositionFactor;

			this.lastPosition[0] += -sensorEvent.Y * sqDeltaT;
			this.lastPosition[1] +=  sensorEvent.Z * sqDeltaT;
			this.lastPosition[2] += -sensorEvent.X * sqDeltaT;

			this.lastLinAccelTimestamp = sensorEvent.Timestamp;

			this.PushSolverFrame(sensorEvent);
		}

		private void HandleRotVecEvent(SensorEvent sensorEvent)
		{
			double x = -sensorEvent.Y;
			double y =  sensorEvent.Z;
			double z = -sensorEvent.X;

			double sqX = x * x;
			double sqY = y * y;
			double sqZ = z * z;
			double sqW = Math.Abs(1.0 - sqX - sqY - sqZ);

			double w = Math.Sqrt(sqW);

			
			double alpha, beta, gamma;

			alpha = Math.Atan2(sqW + sqX - sqY - sqZ, 2.0 * (x * y + w * z)) * AbstractTrackSolver.rad2grad;
			beta  = Math.Asin(-2.0 * (x * z - w * y)) * AbstractTrackSolver.rad2grad;
			gamma = Math.Atan2(2.0 * (y * z + w * x), sqW - sqX - sqY + sqZ) * AbstractTrackSolver.rad2grad;

			alpha = (alpha + 270.0) % 360.0;

			if (double.IsNaN(beta))
			{
				beta = 90.0;
			}

			this.lastRotation[0] =  gamma;
			this.lastRotation[1] =  beta;
			this.lastRotation[2] =  alpha;


			/*
			double t = x * y + z * w;

			if (Math.Abs(Math.Abs(t) - 0.5) < double.Epsilon)
			{
				alpha = Math.Atan2(x, w);
				beta  = AbstractTrackSolver.piHalf;
				gamma = 0.0;

				if (t < 0)
				{
					alpha = -alpha;
					beta  = -beta;
				}
			}
			else
			{
				alpha = Math.Atan2(2.0 * (y * w - x * z), 1.0 - 2.0 * (sqY + sqZ));
				beta  = Math.Asin(2.0 * t);
				gamma = Math.Atan2(2.0 * (x * w - y * z), 1.0 - 2.0 * (sqX + sqZ));
			}

			alpha *= AbstractTrackSolver.rad2grad;
			beta  *= AbstractTrackSolver.rad2grad;
			gamma *= AbstractTrackSolver.rad2grad;

			alpha = (alpha + 270.0) % 360.0;

			this.lastRotation[0] =  alpha;
			this.lastRotation[1] = -beta;
			this.lastRotation[2] =  gamma;
			*/


			/*
			Quaternions rot = new Quaternions(w, x, y, z);

			rot.GetEuler(ref this.lastRotation[0], ref this.lastRotation[1], ref this.lastRotation[2]);

			SolverFrame solverFrame = new SolverFrame(sensorEvent.Timestamp - this.firstTimestamp, this.lastPosition, this.lastRotation);

			this.solverData.Add(solverFrame.Timestamp, solverFrame);
			*/


			/*
			double[] rotMat = null;
			double[] remappedRotMat = null;

			this.GetRotationMatrixFromSensorEvent(sensorEvent, out rotMat);
			this.RemapRotationMatrixToCameraOrientation(ref rotMat, out remappedRotMat);
			this.GetEulerFromRotationMatrix(ref remappedRotMat, ref this.lastRotation);
			

			this.PushSolverFrame(sensorEvent);
			*/
		}

		protected override void PushSolverFrame(SensorEvent sensorEvent)
		{
			ulong timestamp = sensorEvent.Timestamp - this.firstTimestamp;
			SolverFrame solverFrame;
			SolverFrame existingSolverFrame;
			double[] existingData;

			if (this.solverData.TryGetValue(timestamp, out existingSolverFrame))
			{
				switch (sensorEvent.Type)
				{
					case SensorEvent.SensorType.LIN_ACCEL:
						existingData = existingSolverFrame.Rotation;
						solverFrame = new SolverFrame(timestamp, ref this.lastPosition, ref existingData);
						break;
					case SensorEvent.SensorType.ROT_VEC:
						existingData = existingSolverFrame.Position;
						solverFrame = new SolverFrame(timestamp, ref existingData, ref this.lastRotation);
						break;
					default:
						solverFrame = new SolverFrame(timestamp, ref this.lastPosition, ref this.lastRotation);
						break;
				}

				this.solverData.Remove(timestamp);
			}
			else
			{
				solverFrame = new SolverFrame(timestamp, ref this.lastPosition, ref this.lastRotation);
			}

			this.solverData.Add(solverFrame.Timestamp, solverFrame);
		}

		private void GetRotationMatrixFromSensorEvent(SensorEvent sensorEvent, out double[] matrix)
		{
			double x = sensorEvent.X;
			double y = sensorEvent.Y;
			double z = sensorEvent.Z;
			double w;

			double sqX = x * x;
			double sqY = y * y;
			double sqZ = z * z;
			double dSqX = 2.0 * sqX;
			double dSqY = 2.0 * sqY;
			double dSqZ = 2.0 * sqZ;

			w = 1.0 - sqX - sqY - sqZ;
			w = (w > 0) ? Math.Sqrt(w) : 0.0;

			double dXY = 2.0 * x * y;
			double dZW = 2.0 * z * w;
			double dXZ = 2.0 * x * z;
			double dYW = 2.0 * y * w;
			double dYZ = 2.0 * y * z;
			double dXW = 2.0 * x * w;

			matrix = new double[16];

			matrix[0] = 1.0 - dSqY - dSqZ;
			matrix[1] = dXY - dZW;
			matrix[2] = dXZ + dYW;
			matrix[3] = 0.0;

			matrix[4] = dXY + dZW;
			matrix[5] = 1 - dSqX - dSqZ;
			matrix[6] = dYZ - dXW;
			matrix[7] = 0.0;

			matrix[8] = dXZ - dYW;
			matrix[9] = dYZ + dXW;
			matrix[10] = 1.0 - dSqX - dSqY;
			matrix[11] = 0.0f;

			matrix[12] = matrix[13] = matrix[14] = 0.0;
			matrix[15] = 1.0;
		}

		private void RemapRotationMatrixToCameraOrientation(ref double[] matrix, out double[] remappedMatrix)
		{
			if ((null == matrix) || (16 != matrix.Length))
			{
				throw new ArgumentException("Invalid rotation matrix given");
			}

			remappedMatrix = new double[16];


			int AXIS_X = 1;
			int AXIS_Z = 3;
			int X = AXIS_Z | 0x80;
			int Y = AXIS_X | 0x80;
			int Z = X ^ Y;
			
			int x = (X & 0x3) - 1;
			int y = (Y & 0x3) - 1;
			int z = (Z & 0x3) - 1;

			int axis_y = (z + 1) % 3;
			int axis_z = (z + 2) % 3;

			if (((x ^ axis_y) | (y ^ axis_z)) != 0)
				Z ^= 0x80;

			
			bool sx = (X >= 0x80);
			bool sy = (Y >= 0x80);
			bool sz = (Z >= 0x80);

			for (int j = 0; j < 3; j++)
			{
				int offset = j * 4;
				for (int i = 0; i < 3; i++)
				{
					if (x == i) remappedMatrix[offset + i] = sx ? -matrix[offset + 0] : matrix[offset + 0];
					if (y == i) remappedMatrix[offset + i] = sy ? -matrix[offset + 1] : matrix[offset + 1];
					if (z == i) remappedMatrix[offset + i] = sz ? -matrix[offset + 2] : matrix[offset + 2];
				}
			}

			remappedMatrix[3] = remappedMatrix[7] = remappedMatrix[11] = remappedMatrix[12] = remappedMatrix[13] = remappedMatrix[14] = 0.0;
			remappedMatrix[15] = 1.0;
		}

		private void GetEulerFromRotationMatrix(ref double[] matrix, ref double[] euler)
		{
			euler[0] = Math.Atan2(matrix[1], matrix[5]) * AbstractTrackSolver.rad2grad;
			euler[1] = Math.Asin(-matrix[9]) * AbstractTrackSolver.rad2grad;
			euler[2] = Math.Atan2(-matrix[8], matrix[10]) * AbstractTrackSolver.rad2grad;

			/*
			if (Math.Abs(Math.Abs(matrix[8]) - 1.0) <= RLCameraTrackSolver.tenthDegreeInRad)
			{
				double theta1 = Math.Asin(matrix[8]);
				double theta2 = Math.PI - theta1;

				double cosTheta1 = Math.Cos(theta1);
				double cosTheta2 = Math.Cos(theta2);

				double psi1 = Math.Atan2(matrix[9] / cosTheta1, matrix[10] / cosTheta1);
				double psi2 = Math.Atan2(matrix[9] / cosTheta2, matrix[10] / cosTheta2);

				double phi1 = Math.Atan2(matrix[4] / cosTheta1, matrix[0] / cosTheta1);
				double phi2 = Math.Atan2(matrix[4] / cosTheta2, matrix[0] / cosTheta2);

				euler[0] = psi1;
				euler[1] = theta1;
				euler[2] = phi1;
			}
			else
			{
				double theta;
				double psi;
				double phi = 0.0;

				if (matrix[8] < 0)
				{
					theta = Math.PI * 0.5;
					psi = Math.Atan2(matrix[1], matrix[2]);
				}
				else
				{
					theta = -Math.PI * 0.5;
					psi = Math.Atan2(-matrix[1], -matrix[2]);
				}

				euler[0] = psi;
				euler[1] = theta;
				euler[2] = phi;
			}

			euler[0] *= AbstractTrackSolver.rad2grad;
			euler[1] *= AbstractTrackSolver.rad2grad;
			euler[2] *= AbstractTrackSolver.rad2grad;
			*/
		}
	}
}
