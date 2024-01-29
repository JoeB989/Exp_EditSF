using EsfHelper;
using EsfLibrary;
using System.Diagnostics;
using System.Text;

namespace TDD_FixSave
{
	/// <summary>
	/// The Dawnless Days save file fixer
	/// 
	/// Usage:
	///		TDD_FixSave  [-f]  [rotation-fix-list]
	///		
	///	Optional flags:
	///		-f	force the immediate processing of the latest save file, regardless of how old it is
	///	
	/// This app:
	///	 1. Fixes the ordering of character follower skills and army traditions,
	///		needed due to some Attila pecularities with horizontal dependencies between skills
	///	 2. Optionally fixes the rotation of custom city models on the campaign map if a rotation-fix-list is supplied,
	///		because Attila generates random city rotation on every new game.
	///	 3. Automatically fixes all save game types
	///		a. named saves
	///		b. quick saves
	///		c. auto-saves
	///		d. manual battle saves
	///  4.	Overwrites the original save file so the fix process is transparent to the player.
	///		a. saves a backup of the original save so a corrupted fix can be manually unwound by the player
	/// </summary>
	/// <remarks>
	/// 
	/// The rotation-fix-list has the format:
	///		region,slot,direction;region,slot,direction;...
	///	region is the region name
	///	slot is the building slot number.  Currently we only anticipate adjusting the settlement building in slot 0, but this allows for future expansion
	///	direction is 0-5
	///	
	///	Known TDD region settlement rotations:
	///		rom_reg_anorien_minas_tirith, 5
	///		rom_reg_westfold_hornburg, 0 or 1 (pick 1 for now)
	///		rom_reg_gap_of_rohan_isengard, 5
	///	which resolves to a fix list:
	///		rom_reg_anorien_minas_tirith,0,5;rom_reg_westfold_hornburg,0,1;rom_reg_gap_of_rohan_isengard,0,5
	/// 
	/// The region & slot are used to navigate the save file like this:
	/// CAMPAIGN_SAVE_GAME/
	///   COMPRESSED_DATA/
	///     CAMPAIGN_ENV/
	/// 	  CAMPAIGN_MODEL/
	/// 	    WORLD/
	/// 		  REGION_MANAGER/
	/// 		    REGIONS_ARRAY/
	/// 			  REGIONS_ARRAY - id/
	/// 			    REGION/
	/// 				  SETTLEMENT/
	/// 				    SETTLEMENT_EXPANSION_MANAGER/
	/// 					  SLOTS_ARRAY/
	/// 					    SLOTS_ARRAY - slot/
	/// 						  SETTLEMENT_EXPANSION_SLOT
	/// 							Value[1] = direction
	/// </remarks>
	internal class Program
	{
		static void Main(string[] args)
		{
			EsfFile file;
			bool changes = false;
			string saveFile, backupFile;
			StringBuilder report = new StringBuilder();

			bool force = false;
			string fixList = string.Empty;

			if (args.Length >= 1)
			{
				if (args[0] == "-f")
				{
					force = true;
#if DEBUG
					Console.WriteLine("  Forcing immediate processing of latest file");
#endif
					if (args.Length >= 2)
					{
						fixList = args[1];
					}
				}
				else
				{
					fixList = args[0];
				}
			}

			if (FindFileToEdit(out saveFile, out backupFile, force))
			{
#if DEBUG
				Console.WriteLine("  Found {0}\n  Backing up to {1}", saveFile, Path.GetFileName(backupFile));
#endif
				Helper.SaveFileName = Path.GetFileName(saveFile);	

				Stopwatch timer = Stopwatch.StartNew();
				file = OpenAndSaveBackup(saveFile, backupFile);
				TimeSpan time_to_backup = timer.Elapsed;

				timer.Restart();
				changes = FixSkills(file, report);
				TimeSpan time_to_fix_skills = timer.Elapsed;

				timer.Restart();
				if (!string.IsNullOrEmpty(fixList))
				{
					changes |= FixRotations(file.RootNode, fixList, report);
				}
				TimeSpan time_to_fix_rotations = timer.Elapsed;

				timer.Restart();
				if (changes)
				{
					EsfCodecUtil.WriteEsfFile(saveFile, file);
				}
				TimeSpan time_to_save = timer.Elapsed;

#if DEBUG
				Console.WriteLine(report.ToString());
#endif
				Console.WriteLine("Time to back up:        {0}", time_to_backup.ToString());
				Console.WriteLine("Time to scan/fix skills:{0}", time_to_fix_skills.ToString());
				Console.WriteLine("Time to fix rotations:  {0}", time_to_fix_rotations.ToString());
				if (changes)
					Console.WriteLine("Time to save:           {0}", time_to_save.ToString());
				else
					Console.WriteLine("Time to save:           NOT SAVED, no changes");
			}
		}

		private static bool FindFileToEdit(out string saveFile, out string backupFile, bool force)
		{
			// wait for a new save file to appear in Attila save_games or resume folders
			// criteria
			//	- wait 10 seconds for Attila to finish saving
			//	- select latest file within last 30 seconds

			bool found = false;
			saveFile = string.Empty;
			backupFile = string.Empty;

			const string game = "Attila";
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string saveGamePath = $@"{appDataPath}\The Creative Assembly\{game}\save_games";
			string battleSavePath = $@"{appDataPath}\The Creative Assembly\{game}\resume";

			// too hardcoded?
			TimeSpan delayTime = new TimeSpan(0, 0, 10);
			TimeSpan maxAge = new TimeSpan(0, 0, 30);
			//TimeSpan maxAge = new TimeSpan(9999, 0, 0, 30);   // TEMP: for testing, always get latest no matter how old
			if (! force)
			{
#if DEBUG
				Console.WriteLine("Waiting {0} for {1} to finish saving...", delayTime.ToString(), game);
#endif
				Thread.Sleep(delayTime);    // too brute force?
			}

			var now = DateTime.Now;

			DirectoryInfo saveDir = new DirectoryInfo(saveGamePath);
			DirectoryInfo battleDir = new DirectoryInfo(battleSavePath);
			var files = saveDir.GetFiles("*.save").Union(battleDir.GetFiles("resume"));
			var ordered = from file in files
						  orderby file.LastWriteTime descending
						  select file;
			FileInfo latest;
			if (force)
				latest = ordered.FirstOrDefault();
			else
				latest = (from file in ordered
						  where now - file.LastWriteTime <= maxAge
						  select file).FirstOrDefault();

			if (latest != null)
			{
				saveFile = latest.FullName;
				backupFile = GetBackName(saveFile);
				found = true;
			}
			else
			{
				Console.WriteLine("No recent save-games found in last {0}\n  {1}\n  {2}",
					maxAge.ToString(), saveGamePath, battleSavePath);
			}
			return found;
		}

		private static string GetBackName(string saveFile)
		{
			string ext = Path.GetExtension(saveFile);
			string newName = Path.GetFileNameWithoutExtension(saveFile) + " BACKUP" + ext;
			string path = Path.GetDirectoryName(saveFile);
			return Path.Combine(path, newName);
		}

		private static EsfFile OpenAndSaveBackup(string saveFile, string backupFile)
		{
			EsfFile file = EsfCodecUtil.LoadEsfFile(saveFile);
			EsfCodecUtil.WriteEsfFile(backupFile, file);
			return file;
		}

		private static bool FixSkills(EsfFile file, StringBuilder report)
		{
			return Helper.FixCharacterSkillsFromRoot(file.RootNode, report);
		}

		private class OneFix
		{
			public string Region;
			public uint Slot;
			public uint Direction;

			public OneFix(string region, uint slot, uint direction) { Region = region; Slot = slot; Direction = direction; }
		}

		private static bool FixRotations(EsfNode rootNode, string fixList, StringBuilder report)
		{
			report.AppendLine();
			bool changes = false;

			OneFix[] fixes = ParseFixes(fixList);
			ParentNode RegionsArray = Helper.GetRegionArrayNode(rootNode);

			foreach (OneFix fix in fixes)
			{
				var Value = FindSettlementExpansionSlotValue(RegionsArray, fix.Region, fix.Slot, 1);
				if ((Value != null) && (Value.Value != fix.Direction))
				{
					report.AppendFormat("{0} slot {1} building rotation changed from {2} to {3}\n",
						fix.Region, fix.Slot, Value.Value, fix.Direction);
					Value.Value = fix.Direction;
					changes = true;
				}
			}
			return changes;
		}

		static private OneFix[] ParseFixes(string fixList)
		{
			string[] entries = fixList.Split(';');
			OneFix[] fixes = new OneFix[entries.Length];
			for (int i = 0; i < entries.Length; i++)
			{
				string[] pieces = entries[i].Split(',');
				fixes[i] = new OneFix(pieces[0], uint.Parse(pieces[1]), uint.Parse(pieces[2]));
			}
			return fixes;
		}

		static private EsfValueNode<UInt32> FindSettlementExpansionSlotValue(
			ParentNode RegionsArray, string region, uint slot, uint value)
		{
			ParentNode regionNode = null;
			foreach (var child in RegionsArray.Children)
			{
				var test = child.Children[0];
				string r = ((StringNode)test.Values[1]).Value;
				if (r == region)
				{
					regionNode = test;
					break;
				}
			}
			if (regionNode == null)
				return null;

			string slotsArraySlot = string.Format("SLOTS_ARRAY - {0}", slot);
			string[] nodeHierarchy =
			{
				"SETTLEMENT",
				"SETTLEMENT_EXPANSION_MANAGER",
				"SLOTS_ARRAY",
				slotsArraySlot,
				"SETTLEMENT_EXPANSION_SLOT"
			};

			ParentNode node = regionNode;
			for (int i = 0; i < nodeHierarchy.Length; i++)
				node = Helper.FindChild(node, nodeHierarchy[i]);

			return (EsfValueNode<UInt32>)node.Values[(int)value];
		}
	}
}