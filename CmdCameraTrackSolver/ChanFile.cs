using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdCameraTrackSolver
{
	public class ChanFile
	{
		private string filepath;
		private SortedSet<ChanFrame> frames;

		public ChanFile(string path)
		{
			this.filepath = path;
			this.frames = new SortedSet<ChanFrame>(new ChanFrame.Comparer());
		}

		public bool AddFrame(ChanFrame frame)
		{
			return this.frames.Add(frame);
		}

		public void AddFrames(SortedSet<ChanFrame> frames)
		{
			foreach (ChanFrame frame in frames)
			{
				this.frames.Add(frame);
			}
		}

		public void WriteFile()
		{
			if (File.Exists(this.Filepath))
			{
				throw new IOException("Chan file already exists");
			}

			StreamWriter sw = new StreamWriter(this.Filepath, false, new UTF8Encoding(false));

			foreach (ChanFrame frame in this.frames)
			{
				sw.Write(frame.ToString());
				sw.Write("\n");
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
