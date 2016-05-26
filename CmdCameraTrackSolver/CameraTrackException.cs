using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdCameraTrackSolver
{
	class CameraTrackException : Exception
	{
		public CameraTrackException() { }

		public CameraTrackException(string message) : base(message) { }

		public CameraTrackException(string message, Exception inner) : base(message, inner) { }
	}
}
