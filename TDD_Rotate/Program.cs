using EsfLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDD_Rotate
{
	/// <summary>
	/// The Dawnless Days Building Rotator
	/// An app to edit a save file and adjust the rotation of specific buildings' visual models.
	/// Usage:
	///		TDD_Rotate.exe inputFile.save  outputFile.save  "list of buildings and hex directions"
	///	For streamlined testing, use this batch file to 'fix' a specific save with latest fix list
	///		TDD_Fix.bat  savefile.save		==> writes savefile_fixed.save
	/// </summary>
	/// <remarks>
	/// The "list of buildings and angles" has the format:
	///		id,slot,direction;id,slot,direction;...
	///	id is the region id
	///	slot is the building slot number.  Currently we only anticipate adjusting the settlement building in slot 0, but this allows for future expansion
	///	direction is 0-5
	///	
	///	Known TDD region settlement rotations:
	///		43, rom_reg_anorien_minas_tirith, 5
	///		93, rom_reg_westfold_hornburg, 0 or 1 (pick 1 for now)
	///		155, rom_reg_gap_of_rohan_isengard, 5
	///	which resolves to a fix list:
	///		43,0,5;93,0,1;155,0,5
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
	///
	/// To package TDD_Rotate.exe into a Pack file for use by a LUA script
	///		1. build the release version to minimize its size
	///		2. use an online hex editor like https://hexed.it/ to display the exe's bytes in hex
	///		3. use notepad++ to convert the hex to a format like slots_binaries
	///		4. store the binary string in a db entry
	///		5. write a LUA script to extract the db entry to an exe in the Attila folder
	///		6. use os.execute to run the appropriate command line
	/// </remarks>
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 3)
				Console.WriteLine("Usage:\n  TDD_Rotate.exe inputFile.save outputFile.save id,slot,direction;id,slot,direction;...");
			else
				RotateBuildings(args[0], args[1], args[2]);
		}

		class OneFix
		{
			public uint Id;
			public uint Slot;
			public uint Direction;

			public OneFix(uint id, uint slot, uint direction) { Id = id; Slot = slot; Direction = direction; }
		}

		static private void RotateBuildings(string inputFile, string outputFile, string fixList)
		{
			OneFix[] fixes = ParseFixes(fixList);
			EsfFile file = EsfCodecUtil.LoadEsfFile(inputFile);

			ParentNode RegionsArray = FindRegionsArray(file);

			foreach (OneFix fix in fixes)
			{
				var Value = FindSettlementExpansionSlotValue(RegionsArray, fix.Id, fix.Slot, 1);
				if (Value != null)
					Value.Value = fix.Direction;
			}

			EsfCodecUtil.WriteEsfFile(outputFile, file);
		}

		static private OneFix[] ParseFixes(string fixList)
		{
			string[] entries = fixList.Split(';');
			OneFix[] fixes = new OneFix[entries.Length];
			for (int i=0; i < entries.Length; i++)
			{
				string[] pieces = entries[i].Split(',');
				fixes[i] = new OneFix(uint.Parse(pieces[0]), uint.Parse(pieces[1]), uint.Parse(pieces[2]));
			}
			return fixes;
		}

		static private ParentNode FindRegionsArray(EsfFile file)
		{
			string[] nodeHierarchy =
			{
				//"CAMPAIGN_SAVE_GAME",		// not needed, this is root node
				"COMPRESSED_DATA",
				"CAMPAIGN_ENV",
				"CAMPAIGN_MODEL",
				"WORLD",
				"REGION_MANAGER",
				"REGIONS_ARRAY",
			};

			ParentNode node = (ParentNode)file.RootNode;
			for (int i = 0; i < nodeHierarchy.Length; i++)
				node = FindChild(node, nodeHierarchy[i]);
			return node;
		}

		static private ParentNode FindChild(ParentNode parent, string name)
		{
			foreach (var child in parent.Children)
			{
				if (child.Name == name)
					return child;
			}
			return null;
		}

		static private EsfValueNode<UInt32> FindSettlementExpansionSlotValue(
			ParentNode RegionsArray, uint Id, uint Slot, uint Value)
		{
			// region Id is 1-based, REGIONS_ARRAY is 0-based
			string regionsArrayId = string.Format("REGIONS_ARRAY - {0}", Id-1);
			string slotsArraySlot = string.Format("SLOTS_ARRAY - {0}", Slot);
			string[] nodeHierarchy =
			{
				regionsArrayId,
				"REGION",
				"SETTLEMENT",
				"SETTLEMENT_EXPANSION_MANAGER",
				"SLOTS_ARRAY",
				slotsArraySlot,
				"SETTLEMENT_EXPANSION_SLOT"
			};

			ParentNode node = RegionsArray;
			for (int i = 0; i < nodeHierarchy.Length; i++)
				node = FindChild(node, nodeHierarchy[i]);

			return (EsfValueNode<UInt32>) node.Values[(int)Value];
		}
	}
}
