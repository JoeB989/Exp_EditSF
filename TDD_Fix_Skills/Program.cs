using EsfControl;
using EsfLibrary;
using System;

namespace TDD_Fix_Skills
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 3)
			{
				Console.WriteLine("Usage:\n  TDD_Fix_Skills  Attila  inputFile  outputFile");
				return;
			}

			string game = args[0];
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string saveGamePath = $@"{appDataPath}\The Creative Assembly\{game}\save_games";

			string inputFile = Path.Combine(saveGamePath, args[1]);
			string outputFile = Path.Combine(saveGamePath, args[2]);

			DateTime start = DateTime.Now;

			EsfFile file = EsfCodecUtil.LoadEsfFile(inputFile);
			string report = Helper.FixCharacterSkillsFromRoot(file.RootNode);

			TimeSpan time_to_modify = DateTime.Now - start;
			start = DateTime.Now;

			EsfCodecUtil.WriteEsfFile(outputFile, file);

			TimeSpan time_to_write = DateTime.Now - start;

#if DEBUG
			Console.WriteLine("DEBUG build");
#else
			Console.WriteLine("RELEASE build");
#endif

//#if DEBUG
			Console.WriteLine("Time to modify: {0}\nTime to save file: {1}",
				time_to_modify.ToString(), time_to_write.ToString());

			Console.WriteLine(report);
//#endif
		}
	}
}