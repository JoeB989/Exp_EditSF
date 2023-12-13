using EsfLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace EsfControl
{
	static internal partial class Helper
	{
		static private void AllFactionsReport(EsfNode factionArrayNode, ReportConfig cfg)
		{
			GetAllFactions(factionArrayNode);
			ParentNode worldNode = (ParentNode)factionArrayNode.Parent;
			ParentNode FamilyTreeNode = findChild(worldNode, "FAMILY_TREE");
			List<FamilyMember> familyTree = ScanFamilyTree(FamilyTreeNode);

			List<ParentNode> factions = new List<ParentNode>();
			foreach (ParentNode factionEntryNode in ((ParentNode)factionArrayNode).Children)
			{
				ParentNode factionNode = factionEntryNode.Children[0];
				factions.Add(factionNode);
			}
			ShowFactionsReport(factions.ToArray(), familyTree, cfg);
		}

		static public void OneFactionReport(EsfNode factionEntryNode)
		{
			GetAllFactions(factionEntryNode.Parent);
			ParentNode worldNode = (ParentNode)factionEntryNode.Parent.Parent;
			ParentNode FamilyTreeNode = findChild(worldNode, "FAMILY_TREE");
			List<FamilyMember> familyTree = ScanFamilyTree(FamilyTreeNode);

			ParentNode factionNode = ((ParentNode)factionEntryNode).Children[0];
			var nodes = new ParentNode[] { factionNode };
			var cfg = new ReportConfig()
			{
				EconomicReport = true,
				CharacterReport = true,
				ArmyReport = true,
				OmitGarrisons = false,//true,
				ShowDiplomacy = true,
			};
			ShowFactionsReport(nodes, familyTree, cfg);
		}

		private struct ReportConfig
		{
			public bool EconomicReport;
			public bool CharacterReport;
			public bool ArmyReport;
			public bool OmitGarrisons;
			public bool ShowDiplomacy;
		}

		static private void ShowFactionsReport(ParentNode[] nodes, List<FamilyMember> familyTree, ReportConfig cfg)
		{
			// hack to find the root node
			EsfNode rootNode = nodes[0];
			while (rootNode.Parent != null)
				rootNode = rootNode.Parent;

			StringBuilder report = new StringBuilder();
			uint gameYear, gameMonth;
			reportHeader(rootNode, report, out gameYear, out gameMonth);

			foreach (var factionNode in nodes)
			{
				buildFactionReport(factionNode, report, gameYear, gameMonth, familyTree, cfg);
			}

			var ret = MessageBox.Show(report.ToString(), "Click OK to copy report to clipboard", MessageBoxButtons.OKCancel);
			if (ret == DialogResult.OK)
			{
				Clipboard.SetText(report.ToString());
			}
		}

		static private void reportHeader(EsfNode rootNode, StringBuilder report, out uint gameYear, out uint gameMonth)
		{
			string title = Application.OpenForms[0].Text; // hack
			int dotsave = title.IndexOf(".save", StringComparison.OrdinalIgnoreCase);
			string savefile = (dotsave > 0) ? title.Substring(0, dotsave + 5) : title;

			var saveGameHeader = findChild((ParentNode)rootNode, "SAVE_GAME_HEADER");
			string playerFaction = ((StringNode)saveGameHeader.Values[0]).Value;
			uint turn = ((OptimizedUIntNode)saveGameHeader.Values[2]).Value;
			var dateNode = findChild(saveGameHeader, "DATE");
			gameYear = ((OptimizedUIntNode)dateNode.Values[0]).Value;
			gameMonth = ((OptimizedUIntNode)dateNode.Values[2]).Value; // 0-based
			string monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName((int)gameMonth + 1);
			uint display_year = gameYear - 752; // hack guess

			report.AppendFormat("Save file: {0}\n    Turn {1}, {2} {3}\n    Player faction: {4}\n",
				savefile, turn, display_year, monthName, playerFaction);
		}

		static private void buildFactionReport(ParentNode factionNode, StringBuilder report,
			uint game_year, uint game_month, List<FamilyMember> familyTree, ReportConfig cfg)
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
			report.AppendFormat("Faction[{0}]: {1} (id:{2})\n", index, factionName, factionId);

			if (cfg.EconomicReport)
				EconomicReport(factionNode, report);

			if (cfg.ShowDiplomacy)
				DiplomacyReport(factionNode, report);

			if (cfg.CharacterReport)
				CharacterReport(factionNode, report, game_year, game_month, familyTree);

			if (cfg.ArmyReport)
				ArmyReport(factionNode, report, cfg);
		}
	}
}
