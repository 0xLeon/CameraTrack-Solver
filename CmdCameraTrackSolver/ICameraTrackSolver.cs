using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdCameraTrackSolver
{
	interface ICameraTrackSolver
	{
		void WriteTrackFile(string path);

		void Solve();
	}
}
