using EsfLibrary;
using System.Text;

namespace EsfHelper
{
	static public partial class Helper
	{
		static private void AllFactionsReport(StringBuilder report, EsfNode factionArrayNode, ReportConfig cfg)
		{
			GetAllFactions(factionArrayNode);
			ParentNode worldNode = (ParentNode)factionArrayNode.Parent;
			ParentNode FamilyTreeNode = FindChild(worldNode, "FAMILY_TREE");
			List<FamilyMember> familyTree = ScanFamilyTree(FamilyTreeNode);

			List<ParentNode> factions = new List<ParentNode>();
			foreach (ParentNode factionEntryNode in ((ParentNode)factionArrayNode).Children)
			{
				ParentNode factionNode = factionEntryNode.Children[0];
				factions.Add(factionNode);
			}
			ShowFactionsReport(report, factions.ToArray(), familyTree, cfg);
		}

		static public void OneFactionReport(EsfNode factionEntryNode, StringBuilder report)
		{
			GetAllFactions(factionEntryNode.Parent);
			ParentNode worldNode = (ParentNode)factionEntryNode.Parent.Parent;
			ParentNode FamilyTreeNode = FindChild(worldNode, "FAMILY_TREE");
			List<FamilyMember> familyTree = ScanFamilyTree(FamilyTreeNode);

			ParentNode factionNode = ((ParentNode)factionEntryNode).Children[0];
			var nodes = new ParentNode[] { factionNode };
			var cfg = new ReportConfig()
			{
				EconomicReport = true,
				CharacterReport = true,
				ArmyReport = true,
				OmitGarrisons = true,
				ShowDiplomacy = true,
			};
			ShowFactionsReport(report, nodes, familyTree, cfg);
		}

		private struct ReportConfig
		{
			public bool EconomicReport;
			public bool CharacterReport;
			public bool ArmyReport;
			public bool OmitGarrisons;
			public bool ShowDiplomacy;
		}

		public class GlobalEco
		{
			public int Factions;
			public int Treasury;
			public int Income;
			public int Taxes;
			public int Trade;
			public int Expenses;
		}

		static private void ShowFactionsReport(StringBuilder report, ParentNode[] nodes, List<FamilyMember> familyTree, ReportConfig cfg)
		{
			// hack to find the root node
			EsfNode rootNode = nodes[0];
			while (rootNode.Parent != null)
				rootNode = rootNode.Parent;

			uint gameYear, gameMonth;
			reportHeader(rootNode, report, out gameYear, out gameMonth);

			GlobalEco globalEco = new GlobalEco();

			foreach (var factionNode in nodes)
			{
				buildFactionReport(factionNode, report, gameYear, gameMonth, familyTree, cfg, globalEco);
			}

			// If it's a multi-faction economic report, append total GDP report at the end
			if (cfg.EconomicReport && (nodes.Length > 1))
			{
				gdpReport(report, globalEco);
			}
		}

		static private void reportHeader(EsfNode rootNode, StringBuilder report, out uint gameYear, out uint gameMonth)
		{
			var gameData = GetGameData(rootNode);
			gameYear = gameData.GameYear;
			gameMonth = gameData.GameMonth;

			string monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName((int)gameData.GameMonth + 1);
			uint display_year = gameData.GameYear - 752; // hack guess

			report.AppendFormat("Save file: {0}\n    Difficulty {1}\n    Turn {2}, {3} {4}\n    Player faction: {5}\n",
				SaveFileName, gameData.Difficulty, gameData.Turn, display_year, monthName, gameData.PlayerFaction);
		}

		public class GameData 
		{
			public string PlayerFaction;
			public uint Turn;
			public uint GameYear;
			public uint GameMonth;
			public string Difficulty;
		}

		static public GameData GetGameData(EsfNode rootNode)
		{
			GameData gameData = new GameData();

			var saveGameHeader = FindChild((ParentNode)rootNode, "SAVE_GAME_HEADER");
			gameData.PlayerFaction = ((StringNode)saveGameHeader.Values[0]).Value;
			gameData.Turn = ((OptimizedUIntNode)saveGameHeader.Values[2]).Value;

			var dateNode = FindChild(saveGameHeader, "DATE");
			gameData.GameYear = ((OptimizedUIntNode)dateNode.Values[0]).Value;
			gameData.GameMonth = ((OptimizedUIntNode)dateNode.Values[2]).Value; // 0-based

			var campaignSetup = GetCampaignSetupNode(rootNode);
			int diff = ((OptimizedIntNode)campaignSetup.Values[14]).Value;
			gameData.Difficulty = difficultyFromInt(diff);

			return gameData;
		}

		static private void gdpReport(StringBuilder report, GlobalEco globalEco)
		{
			report.AppendFormat("\n=====\n"+"" +
				"  Global Economy ({0} factions)\n" +
				"    Total Treasuries: {1}\n" +
				"        Total Income: {2}, which includes\n" +
				"                              {3} from taxes\n" +
				"                              {4} from trade\n" +
				"      Total Expenses: {5}",
				globalEco.Factions, globalEco.Treasury,
				globalEco.Income, globalEco.Taxes, globalEco.Trade, globalEco.Expenses);
		}

		static private string difficultyFromInt(int diff)
		{
			switch (diff)
			{
				case 1: return "Easy";
				case 0: return "Normal";
				case -1: return "Hard";
				case -2: return "Very Hard";
				case -3: return "Legendary";
				default: return "Unknown";
			}
		}

		static private void buildFactionReport(ParentNode factionNode, StringBuilder report,
			uint game_year, uint game_month, List<FamilyMember> familyTree, ReportConfig cfg,
			GlobalEco globalEco)
		{
			var arrayNode = (ParentNode)factionNode.Parent;
			int index = 0;
			if (arrayNode.Name.StartsWith(FactionArrayTitle))
			{
				string number = arrayNode.Name.Substring(FactionArrayTitle.Length).Trim();
				int.TryParse(number, out index);
			}

			uint factionId = ((OptimizedUIntNode)(factionNode.Values[0])).Value;
			string factionName = ((StringNode)(factionNode.Values[1])).Value;

			// FACTION
			//		CAMPAIGN_EFFECT_BUNDLES
			//			EFFECT_BUNDLE_BLOCK
			//				EFFECT_BUNDLE_BLOCK - 0
			//					CAMPAIGN_EFFECT_BUNDLE
			//						Values[0] = att_bundle_faction_imperium_level_4
			const string imperiumString = "imperium_level";
			const string fameString = "fame_level";
			string factionImperium = "";
			var campaignEffectBundles = FindChild((ParentNode)factionNode, "CAMPAIGN_EFFECT_BUNDLES");
			if (campaignEffectBundles != null)
			{
				foreach (var bundleBlockArray in campaignEffectBundles.Children)
				{
					foreach (var bundleBlock in bundleBlockArray.Children)
					{
						var bundle = bundleBlock.Children[0];
						string effectName = ((StringNode)(bundle.Values[0])).Value;
						if (effectName.Contains(imperiumString) ||
							effectName.Contains(fameString))
						{
							factionImperium += ", " + effectName;
						}
					}
				}
			}

			report.AppendFormat("Faction[{0}]: {1} (id:{2}) {3}\n", index, factionName, factionId, factionImperium);

			var characters = FindChild(factionNode, "CHARACTER_ARRAY");
			if ((characters == null) || (characters.Children == null) || (characters.Children.Count == 0))
			{
				report.AppendLine("    Faction has been destroyed");
				return;
			}

			if (cfg.EconomicReport)
				EconomicReport(factionNode, report, globalEco);

			if (cfg.ShowDiplomacy)
				DiplomacyReport(factionNode, report);

			if (cfg.CharacterReport)
				CharacterReport(factionNode, report, game_year, game_month, familyTree);

			if (cfg.ArmyReport)
				ArmyReport(factionNode, report, cfg);
		}
	}
}
