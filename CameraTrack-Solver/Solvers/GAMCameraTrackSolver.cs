using System;

using CameraTrackSolver.Frames;
using CameraTrackSolver.Parsers;

namespace CameraTrackSolver.Solvers
{
	class GAMCameraTrackSolver : AbstractTrackSolver
	{
		private const double freeFallGravitySquared = 0.961703842225;

		private bool gyroInitialized = false;
		private ulong lastGyroTimestamp = 0;
		private double[] gyroRotation = new double[] { 0.0, 0.0, 0.0 };

		private double[] accelMagnetRotation;
		private double[] accelMagnetPosition;
		private SensorEvent lastMagnetEvent = null;

		private ulong fusionLastTimestamp = 0;
		private ulong fusionCounter = 0;

		private double[] dummyData = new double[3];

		public GAMCameraTrackSolver(TrackFileParser tfParser, double fps) : base(tfParser, fps)
		{
			this.fusionLastTimestamp = this.firstTimestamp;
		}

		public override TrackFileParser.CameraTrackerType[] GetSupportedTrackFileTypes()
		{
			return new TrackFileParser.CameraTrackerType[] {
				TrackFileParser.CameraTrackerType.GAM_DEFAULT,
				TrackFileParser.CameraTrackerType.GAM_NATIVE,
			};
		}

		protected override void HandleSensorEvent(SensorEvent sensorEvent)
		{
			switch (sensorEvent.Type)
			{
				case SensorEvent.SensorType.ACCEL:
					this.HandleAccelEvent(sensorEvent);
					break;
				case SensorEvent.SensorType.GYRO:
					this.HandleGyroEvent(sensorEvent);
					break;
				case SensorEvent.SensorType.MAGNETIC:
					this.HandleMagneticEvent(sensorEvent);
					break;
			}

			// this.HandleFusion(sensorEvent.Value);
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
					case SensorEvent.SensorType.ACCEL:
						existingData = existingSolverFrame.Rotation;
						solverFrame = new SolverFrame(timestamp, ref this.accelMagnetPosition, ref existingData);
						break;
					case SensorEvent.SensorType.GYRO:
						existingData = existingSolverFrame.Position;
						solverFrame = new SolverFrame(timestamp, ref existingData, ref this.gyroRotation);
						break;
					default:
						return;
				}

				this.solverData.Remove(timestamp);
			}
			else
			{
				solverFrame = new SolverFrame(timestamp, ref this.accelMagnetPosition, ref this.gyroRotation);
			}

			this.solverData.Add(solverFrame.Timestamp, solverFrame);
		}

		private void HandleAccelEvent(SensorEvent sensorEvent)
		{
			if (null == this.lastMagnetEvent)
			{
				return;
			}

			if (null == this.accelMagnetRotation)
			{
				this.accelMagnetRotation = new double[3];
			}

			/*double Ax = sensorEvent.X;
			double Ay = sensorEvent.Y;
			double Az = sensorEvent.Z;

			double normsqA = Ax * Ax + Ay * Ay + Az * Az;

			if (normsqA < CameraTrackSolver.freeFallGravitySquared)
			{
				return;
			}

			double Ex = this.lastMagnetEvent.X;
			double Ey = this.lastMagnetEvent.Y;
			double Ez = this.lastMagnetEvent.Z;
			double Hx = Ey * Az - Ez * Ay;
			double Hy = Ez * Ax - Ex * Az;
			double Hz = Ex * Ay - Ey * Ax;
			double normH = Math.Sqrt(Hx * Hx + Hy * Hy + Hz * Hz);

			if (normH < 0.1)
			{
				// you're probably in space or at the north pole
				// apparently this tracking doesn't really work at either place
				return;
			}

			double invH = 1.0 / normH;
			Hx *= invH;
			Hy *= invH;
			Hz *= invH;
			double invA = 1.0 / Math.Sqrt(normsqA);
			Ax *= invA;
			Ay *= invA;
			Az *= invA;

			double Mx = Ay * Hz - Az * Hy;
			double My = Az * Hx - Ax * Hz;
			double Mz = Ax * Hy - Ay * Hx;

			double[] R = new double[9];
			R[0] = Hx; R[1] = Hy; R[2] = Hz;
			R[3] = Mx; R[4] = My; R[5] = Mz;
			R[6] = Ax; R[7] = Ay; R[8] = Az;


			int x = 0x00;
			int y = 0x02;
			int z = 0x01;

			double[] remapR = new double[9];
			for (int j = 0; j < 3; j++)
			{
				int offset = j * 3;
				for (int i = 0; i < 3; i++)
				{
					if (x == i) remapR[offset + i] = R[offset + 0];
					if (y == i) remapR[offset + i] = -R[offset + 1];
					if (z == i) remapR[offset + i] = R[offset + 2];
				}
			}

			if (Math.Abs(Math.Abs(remapR[3]) - 1.0) < double.Epsilon)
			{
				this.accelMagnetRotation[0] = 0.0;
				this.accelMagnetRotation[1] = Math.Atan2(remapR[2], remapR[8]);
				this.accelMagnetRotation[2] = Math.PI / 2.0;

				if (remapR[3] < 0.0)
				{
					this.accelMagnetRotation[2] = -this.accelMagnetRotation[2];
				}
			}
			else
			{
				this.accelMagnetRotation[0] = Math.Atan2(-remapR[5], remapR[4]);
				this.accelMagnetRotation[1] = Math.Atan2(-remapR[6], remapR[0]);
				this.accelMagnetRotation[2] = Math.Asin(remapR[3]);
			}*/


			double accelX = -sensorEvent.Y;
			double accelY = sensorEvent.X;
			double accelZ = -sensorEvent.Z;
			double magnetX = -this.lastMagnetEvent.Y;
			double magnetY = this.lastMagnetEvent.X;
			double magnetZ = -this.lastMagnetEvent.Z;

			double hX = accelY * magnetZ - accelZ * magnetY;
			double hY = accelZ * magnetX - accelX * magnetZ;
			double hZ = accelX * magnetY - accelY * magnetX;
			double normH = System.Math.Sqrt(hX * hX + hY * hY + hZ * hZ);

			if (normH < 0.1)
			{
				return;
			}

			double invH = 1.0 / normH;
			hX *= invH;
			hY *= invH;
			hZ *= invH;

			double invAccel = 1.0 / System.Math.Sqrt(accelX * accelX + accelY * accelY + accelZ * accelZ);
			accelX *= invAccel;
			accelY *= invAccel;
			accelZ *= invAccel;

			double mX = accelY * hZ - accelZ * hY;
			double mY = accelZ * hX - accelX * hZ;
			double mZ = accelX * hY - accelY * hX;

			double trace = hX + mY + accelZ;
			double s;
			double x;
			double y;
			double z;
			double w;

			if (trace > 0)
			{
				s = 0.5 / System.Math.Sqrt(trace + 1.0);

				w = 0.25 / s;
				x = (accelY - mZ) * s;
				y = (hZ - accelX) * s;
				z = (mX - hY) * s;
			}
			else if ((hX > mY) && (hX > accelZ))
			{
				s = 0.5 / System.Math.Sqrt(1.0 + hX - mY - accelZ);

				w = (accelY - mZ) * s;
				x = 0.25 / s;
				y = (hY + mX) * s;
				z = (hZ + accelX) * s;
			}
			else if (mY > accelZ)
			{
				s = 0.5 / System.Math.Sqrt(1.0 - hX + mY - accelZ);

				w = (hZ - accelX) * s;
				x = (hY + mX) * s;
				y = 0.25 / s;
				z = (mZ + accelY) * s;
			}
			else
			{
				s = 0.5 / System.Math.Sqrt(1.0 - hX - mY + accelZ);

				w = (mX - hY) * s;
				x = (hZ + accelX) * s;
				y = (mZ + accelY) * s;
				z = 0.25 / s;
			}


			double sqw = w * w;
			double sqx = x * x;
			double sqy = y * y;
			double sqz = z * z;
			double unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
			double invUnit = 1.0 / unit;
			double test = (x * y + z * w) * invUnit;

			if (Math.Abs(Math.Abs(test) - 0.5) < double.Epsilon)
			{
				this.accelMagnetRotation[0] = 0.0;
				this.accelMagnetRotation[1] = 2.0 * Math.Atan2(x, w);
				this.accelMagnetRotation[2] = Math.PI * 0.5;

				if (test < 0.0)
				{
					this.accelMagnetRotation[1] = -this.accelMagnetRotation[1];
					this.accelMagnetRotation[2] = -this.accelMagnetRotation[2];
				}
			}
			else
			{
				this.accelMagnetRotation[1] = Math.Atan2(2.0 * y * w - 2.0 * x * z, sqx - sqy - sqz + sqw);
				this.accelMagnetRotation[2] = Math.Asin(2.0 * test);
				this.accelMagnetRotation[0] = Math.Atan2(2.0 * x * w - 2.0 * y * z, -sqx + sqy - sqz + sqw);
			}

			this.accelMagnetRotation[0] *= AbstractTrackSolver.rad2grad;
			this.accelMagnetRotation[1] *= AbstractTrackSolver.rad2grad;
			this.accelMagnetRotation[2] *= AbstractTrackSolver.rad2grad;


			this.PushSolverFrame(sensorEvent);
		}

		private void HandleGyroEvent(SensorEvent sensorEvent)
		{
			if ((null == this.accelMagnetRotation) || (0 == this.lastGyroTimestamp))
			{
				this.lastGyroTimestamp = sensorEvent.Timestamp;
				return;
			}

			if (!this.gyroInitialized)
			{
				Array.Copy(this.accelMagnetRotation, 0, this.gyroRotation, 0, 3);
				this.gyroInitialized = true;
			}

			double deltaT = (sensorEvent.Timestamp - this.lastGyroTimestamp) * AbstractTrackSolver.ns2s * AbstractTrackSolver.rad2grad;

			this.gyroRotation[0] +=  sensorEvent.X * deltaT;
			this.gyroRotation[1] +=  sensorEvent.Z * deltaT;
			this.gyroRotation[2] += -sensorEvent.Y * deltaT;

			this.lastGyroTimestamp = sensorEvent.Timestamp;

			this.PushSolverFrame(sensorEvent);
		}

		private void HandleMagneticEvent(SensorEvent sensorEvent)
		{
			this.lastMagnetEvent = sensorEvent;
		}

		private void HandleFusion(SensorEvent sensorEvent)
		{
			if (null == this.accelMagnetRotation)
			{
				return;
			}

			this.fusionCounter += sensorEvent.Timestamp - this.fusionLastTimestamp;
			this.fusionLastTimestamp = sensorEvent.Timestamp;

			if (this.fusionCounter >= 10000000)
			{
				this.fusionCounter = 0;

				// do fusion
				double t = 0.008;
				double invt = 1.0 - t;
				ulong timestamp = sensorEvent.Timestamp - this.firstTimestamp;

				this.gyroRotation[0] = invt * this.gyroRotation[0] + t * this.accelMagnetRotation[0];
				this.gyroRotation[1] = invt * this.gyroRotation[1] + t * this.accelMagnetRotation[1];
				this.gyroRotation[2] = invt * this.gyroRotation[2] + t * this.accelMagnetRotation[2];

				if (this.solverData.ContainsKey(timestamp))
				{
					this.solverData.Remove(timestamp);
				}

				this.solverData.Add(timestamp, new SolverFrame(timestamp, ref this.accelMagnetPosition, ref this.gyroRotation));
			}
		}
	}
}
