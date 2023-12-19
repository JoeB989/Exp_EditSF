using EsfLibrary;
using System.Text;

namespace EsfHelper
{
	static public partial class Helper
	{
		static private void VerificationReport(StringBuilder report, EsfNode rootNode, ParentNode regionArrayNode)
		{
			GetAllFactions(GetFactionArrayNode(rootNode));

			List<ParentNode> regions = new List<ParentNode>();
			foreach (var arrayNode in regionArrayNode.Children)
			{
				regions.Add(arrayNode.Children[0]);
			}

			ShowRegionVerificationReport(report, rootNode, regions.ToArray());
		}

		static private void ShowRegionVerificationReport(StringBuilder report, EsfNode rootNode, ParentNode[] regionNodes)
		{
			uint gameYear, gameMonth;
			reportHeader(rootNode, report, out gameYear, out gameMonth);
			report.AppendLine();

			int errors = 0;
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

			foreach (var region in regionNodes)
			{
				string name = ((StringNode)region.Values[1]).Value;
				uint wealth = ((OptimizedUIntNode)region.Values[3]).Value;
				bool desolate = (wealth == 0);

				if (!desolate)
				{
					var slot0_BuildingManager = WalkChildren(region, buildingNodeHierarchy);
					if (slot0_BuildingManager.Children.Count == 0)
					{
						report.AppendFormat("{0} has no settlement building\n", name);
						errors++;
					}
					else
					{
						var garrisonResidence = WalkChildren(region, garrisonResidenceHierarchy);
						uint armyId = ((OptimizedUIntNode)garrisonResidence.Values[6]).Value;
						if (armyId == 0)
						{
							report.AppendFormat("{0} has no garrison\n", name);
							errors++;
						}

						// TODO: wounded garrison
					}
				}
			}

			if (errors == 0)
				report.AppendLine("No verification errors found");
		}
	}
}
