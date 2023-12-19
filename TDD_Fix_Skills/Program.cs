using EsfHelper;
using EsfLibrary;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace TDD_Fix_Skills
{
	/// <summary>
	/// OBSOLETE - functionality moved to TDD_FixSave
	/// </summary>
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

			Stopwatch timer = Stopwatch.StartNew();
			EsfFile file = EsfCodecUtil.LoadEsfFile(inputFile);
			TimeSpan time_to_load = timer.Elapsed;

			timer.Restart();
			StringBuilder report = new StringBuilder();
			Helper.FixCharacterSkillsFromRoot(file.RootNode, report);
			TimeSpan time_to_modify = timer.Elapsed;

			timer.Restart();
			EsfCodecUtil.WriteEsfFile(outputFile, file);
			TimeSpan time_to_write = timer.Elapsed;

			var assemblyConfigurationAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyConfigurationAttribute>();
			if (assemblyConfigurationAttribute != null)
				Console.WriteLine("{0} build", assemblyConfigurationAttribute.Configuration.ToUpper());

#if DEBUG
			Console.WriteLine(report.ToString());
#endif

			Console.WriteLine("Time to load: {0}\nTime to modify: {1}\nTime to save file: {2}",
				time_to_load.ToString(),
				time_to_modify.ToString(),
				time_to_write.ToString());
		}
	}
}