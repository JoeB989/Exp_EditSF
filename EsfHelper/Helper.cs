using EsfLibrary;
using System.Text;

namespace EsfHelper
{
	static public partial class Helper
	{
		/// <summary>
		/// Set the save file name you want listed in report headers
		/// </summary>
		static public string SaveFileName { get; set; }

		static public void AllFactionEconomicsReportFromRoot(EsfNode rootNode, StringBuilder report)
		{
			if (rootNode == null)
				return;
			AllFactionEconomicsReport(GetFactionArrayNode(rootNode), report);
		}

		static public void AllFactionCharactersReportFromRoot(EsfNode rootNode, StringBuilder report)
		{
			if (rootNode == null)
				return;
			AllFactionCharactersReport(GetFactionArrayNode(rootNode), report);
		}

		static public void OneFactionReportFromRoot(EsfNode rootNode, int index, StringBuilder report)
		{
			if (rootNode == null)
				return;
			var factionNode = getFactionNode(rootNode, index);
			OneFactionReport(factionNode, report);
		}

		static public void AllFactionEconomicsReport(EsfNode factionArrayNode, StringBuilder report)
		{
			AllFactionsReport(report, factionArrayNode,
				new ReportConfig() { EconomicReport = true, ShowDiplomacy = true });
		}

		static public void AllFactionCharactersReport(EsfNode factionArrayNode, StringBuilder report)
		{
			AllFactionsReport(report, factionArrayNode,
				new ReportConfig() { CharacterReport = true });
		}

		static public void VerificationReportFromRoot(EsfNode rootNode, StringBuilder report)
		{
			if (rootNode == null)
				return;
			VerificationReport(report, rootNode, GetRegionArrayNode(rootNode));
		}

		static public bool FixCharacterSkillsFromRoot(EsfNode rootNode, StringBuilder report)
		{
			if (rootNode == null)
				return false;

			return FixCharacterSkills(report, rootNode);
		}

		//
		//  Common utility methods
		//
		const string FactionArrayTitle = "FACTION_ARRAY - ";

		static private ParentNode getFactionNode(EsfNode rootNode, int index)
		{
			var factionArray = GetFactionArrayNode(rootNode);
			string name = string.Format("{0}{1}", FactionArrayTitle, index);
			return FindChild(factionArray, name);
		}

		static private ParentNode GetFactionArrayNode(EsfNode rootNode)
		{
			string[] nodeHierarchy =
			{
				"COMPRESSED_DATA",
				"CAMPAIGN_ENV",
				"CAMPAIGN_MODEL",
				"WORLD",
				"FACTION_ARRAY",
			};
			return WalkChildren(rootNode, nodeHierarchy);
		}

		static public ParentNode GetRegionArrayNode(EsfNode rootNode)
		{
			string[] nodeHierarchy =
			{
				"COMPRESSED_DATA",
				"CAMPAIGN_ENV",
				"CAMPAIGN_MODEL",
				"WORLD",
				"REGION_MANAGER",
				"REGIONS_ARRAY",
			};
			return WalkChildren(rootNode, nodeHierarchy);
		}

		static public ParentNode GetCampaignSetupNode(EsfNode rootNode)
		{
			string[] nodeHierarchy =
			{
				"COMPRESSED_DATA",
				"CAMPAIGN_ENV",
				"CAMPAIGN_SETUP",
			};
			return WalkChildren(rootNode, nodeHierarchy);
		}

		static private ParentNode? WalkChildren(EsfNode rootNode, string[] nodeHierarchy)
		{
			ParentNode node = (ParentNode)rootNode;
			for (int i = 0; (i < nodeHierarchy.Length) && (node != null); i++)
				node = FindChild(node, nodeHierarchy[i]);
			return node;
		}

		static public ParentNode? FindChild(ParentNode node, string childName)
		{
			foreach (var child in node.Children)
			{
				if (child.Name == childName)
					return child;
			}
			return null;
		}

		static private ParentNode[] findChildren(ParentNode node, string childName)
		{
			List<ParentNode> nodes = new List<ParentNode>();
			foreach (var child in node.Children)
			{
				if (child.Name == childName)
					nodes.Add(child);
			}
			return nodes.ToArray();
		}

		private class Faction
		{
			// one faction
			public string Name;
			public uint Id;
			public ParentNode FactionArrayNode;

			// this faction's armies
			//public List<ParentNode> MilitaryForceLegacyNodes;

			// temporary - test faction's relations with this faction
			public string Relationship;
		}

		static private List<Faction> Factions = new List<Faction>();

		static private void GetAllFactions(EsfNode factionArrayNode)
		{
			//Factions.Clear();
			if (Factions.Count == 0)
			{
				foreach (ParentNode factionEntryNode in ((ParentNode)factionArrayNode).Children)
				{
					Faction faction = new Faction();
					faction.FactionArrayNode = factionEntryNode;

					ParentNode factionNode = factionEntryNode.Children[0];
					faction.Id = ((OptimizedUIntNode)(factionNode.Values[0])).Value;
					faction.Name = ((StringNode)(factionNode.Values[1])).Value;

					Factions.Add(faction);
				}
			}
		}

		// from https://stackoverflow.com/questions/7040289/converting-integers-to-roman-numerals?page=1&tab=scoredesc#tab-top
		static private string ToRoman(uint number)
		{
			if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException(nameof(number), "insert value between 1 and 3999");
			if (number < 1) return string.Empty;
			if (number >= 1000) return "M" + ToRoman(number - 1000);
			if (number >= 900) return "CM" + ToRoman(number - 900);
			if (number >= 500) return "D" + ToRoman(number - 500);
			if (number >= 400) return "CD" + ToRoman(number - 400);
			if (number >= 100) return "C" + ToRoman(number - 100);
			if (number >= 90) return "XC" + ToRoman(number - 90);
			if (number >= 50) return "L" + ToRoman(number - 50);
			if (number >= 40) return "XL" + ToRoman(number - 40);
			if (number >= 10) return "X" + ToRoman(number - 10);
			if (number >= 9) return "IX" + ToRoman(number - 9);
			if (number >= 5) return "V" + ToRoman(number - 5);
			if (number >= 4) return "IV" + ToRoman(number - 4);
			if (number >= 1) return "I" + ToRoman(number - 1);
			throw new InvalidOperationException("Impossible state reached");
		}
	}
}
