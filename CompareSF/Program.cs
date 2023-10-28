using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompareSFns
{
	static class Program
	{
		/// <summary>
		/// Total War (Attila) save file comparator.  Compares 2 or more save game files and displays
		/// their differences.
		/// 
		/// Optionally supply filenames on command line to pre-populate the file list
		///		CompareSF  [file1.save  file2.save  ...]
		/// </summary>
		/// <remarks>
		/// So far only tested with TW Attila save files
		/// 
		/// TODO:
		///		1. Allow choosing files from menu command
		///		2. Show parallel trees with
		///			a. only differences
		///			b. full files with differences highlighted, navigate differences buttons
		///		3. Save a project with multiple file paths, so can re-examine as the game progresses
		/// </remarks>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Application.Run(new CompareSF(args));
		}
	}
}
