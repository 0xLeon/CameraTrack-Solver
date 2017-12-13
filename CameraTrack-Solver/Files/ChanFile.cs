using System.Collections.Generic;
using System.IO;
using System.Text;

using CameraTrackSolver.Frames;

namespace CameraTrackSolver.Files
{
	public class ChanFile
	{
		private string filepath;
		private HashSet<ChanFrame> frames;

		public ChanFile(string path)
		{
			this.filepath = path;
			this.frames = new HashSet<ChanFrame>();
		}

		public bool AddFrame(ChanFrame frame)
		{
			return this.frames.Add(frame);
		}

		public void AddFrames(IEnumerable<ChanFrame> frames)
		{
			this.frames.UnionWith(frames);
		}

		public void WriteFile()
		{
			if (File.Exists(this.Filepath))
			{
				File.Delete(this.Filepath);

				if (File.Exists(this.Filepath))
				{
					throw new IOException("Chan file already exists");
				}
			}

			StreamWriter sw = new StreamWriter(this.Filepath, false, new UTF8Encoding(false));

			foreach (ChanFrame frame in this.frames)
			{
				sw.Write(frame.ToString());
				sw.Write("\r\n");
			}

			sw.Flush();
			sw.Close();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			foreach (ChanFrame frame in this.frames)
			{
				sb.Append(frame.ToString());
				sb.Append("\n");
			}

			return sb.ToString();
		}

		public string Filepath
		{
			get
			{
				return this.filepath;
			}
			set
			{
				this.filepath = value;
			}
		}
	}
}
