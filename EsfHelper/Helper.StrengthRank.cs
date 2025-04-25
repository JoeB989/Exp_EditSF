using EsfLibrary;
using System.Text;

namespace EsfHelper
{
	static public partial class Helper
	{
		static private void StrengthRankReport(StringBuilder report, EsfNode rootNode, ParentNode regionArrayNode)
		{
			GetAllFactions(GetFactionArrayNode(rootNode));

			List<ParentNode> regions = new List<ParentNode>();
			foreach (var arrayNode in regionArrayNode.Children)
			{
				regions.Add(arrayNode.Children[0]);
			}

			ShowStrengthRankReport(report, rootNode, regions.ToArray());
		}

		public class Region
		{
			public string Name;
			public uint FactionId;
			//string FactionName;
		}

		static private void ShowStrengthRankReport(StringBuilder report, EsfNode rootNode, ParentNode[] regionNodes)
		{
			uint gameYear, gameMonth;
			reportHeader(rootNode, report, out gameYear, out gameMonth);
			report.AppendLine();

			List<Region> regions = new List<Region>();
			string[] buildingNodeHierarchy =
			{
				"REGION_SLOT_MANAGER",
				"REGION_SLOT_ARRAY",
				"REGION_SLOT_ARRAY - 0",
				"REGION_SLOT",
				"REGION_BUILDING_MANAGER",
			};
			string[] garrisonResidenceHierarchy =
			{
				"SETTLEMENT",
				"GARRISON_RESIDENCE",
			};

			foreach (var regionNode in regionNodes)
			{
				string name = ((StringNode)regionNode.Values[1]).Value;
				uint factionId = ((OptimizedUIntNode)regionNode.Values[10]).Value;
				//uint wealth = ((OptimizedUIntNode)regionNode.Values[3]).Value;
				//bool desolate = (wealth == 0);
				regions.Add(new Region() { Name = name, FactionId = factionId });
			}

			foreach (var faction in Factions)
			{
				faction.Strength =
					(from region in regions
					 where region.FactionId == faction.Id
					 select region).Count();
				// TODO: + army units / 10
			}

			var ranked = from faction in Factions
						 orderby faction.Strength descending
						 select faction;

			//string calc = "regions + units/10";
			string calc = "regions";

			report.AppendFormat("Faction strength ({0}) and Rank:\n", calc);
			int rank = 1;
			bool first0 = true;
			foreach (var faction in ranked)
			{
				if (faction.Strength == 0)
				{
					if (first0)
					{
						report.AppendLine("  ---------");
						first0 = false;
					}
					report.AppendFormat("  {0}: {1}\n", faction.Name, faction.Strength);
				}
				else
					report.AppendFormat ("  {0}: {1} ({2})\n", faction.Name, faction.Strength, rank++);
			}

			report.AppendLine("--------- As Tab-Separated Values ---------");
			rank = 1;
			foreach (var faction in ranked)
			{
				report.AppendFormat("{0}\t{1}\t{2}\n", faction.Name, faction.Strength, rank++);
			}
		}
	}
}
