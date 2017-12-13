using System;

namespace CameraTrackSolver.Exceptions
{
	class CameraTrackException : Exception
	{
		public CameraTrackException() { }

		public CameraTrackException(string message) : base(message) { }

		public CameraTrackException(string message, Exception inner) : base(message, inner) { }
	}
}
