namespace CameraTrackSolver.Solvers
{
	interface ICameraTrackSolver
	{
		void WriteChanFile(string path);

		void WriteSensorDataFiles(string path);

		void Solve();
	}
}
