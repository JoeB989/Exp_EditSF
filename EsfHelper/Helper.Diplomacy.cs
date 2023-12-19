using EsfLibrary;
using System.Text;

namespace EsfHelper
{
	static public partial class Helper
	{
		static private void DiplomacyReport(ParentNode factionNode, StringBuilder report)
		{
			const string NEUTRAL = "neutral";
			readDiplomacy(factionNode);
			Dictionary<string, string> relationships = new Dictionary<string, string>();
			foreach (var faction in Factions)
			{
				if (faction.Relationship != null)
				{
					if (relationships.ContainsKey(faction.Relationship))
						relationships[faction.Relationship] += ", " + faction.Name;
					else
						relationships[faction.Relationship] = faction.Name;
				}
			}

			uint warCoord_TargetSettlementId = ((OptimizedUIntNode)(factionNode.Values[46])).Value;
			uint warCoord_TargetArmyId = ((OptimizedUIntNode)(factionNode.Values[47])).Value;
			bool alliesCoordinating = ((OptimizedBoolNode)(factionNode.Values[48])).Value;

			report.AppendLine("  Diplomacy:");
			var list = from rel in relationships
					   where rel.Key != NEUTRAL
					   orderby rel.Key
					   select rel;
			foreach (var rel in list)
			{
				report.AppendFormat("    {0}: {1}\n", rel.Key, rel.Value);
			}

			if (warCoord_TargetSettlementId != 0)
				report.AppendFormat("    War Coordination targeting settlement {0}\n", warCoord_TargetSettlementId);
			if (warCoord_TargetArmyId != 0)
				report.AppendFormat("    War Coordination targeting army {0}\n", warCoord_TargetArmyId);
			if (alliesCoordinating)
			{
				report.AppendFormat("    Allies have joined War Coordination\n");
			}
		}

		static private void readDiplomacy(ParentNode factionNode)
		{
			foreach (Faction faction in Factions)
				faction.Relationship = null;

			var diploManager = FindChild(factionNode, "OLD_DIPLOMACY_MANAGER");
			var relationshipArray = diploManager.Children[0];
			foreach (var arrayEntry in relationshipArray.Children)
			{
				var factionRelationship = arrayEntry.Children[0];
				uint factionId = ((OptimizedUIntNode)(factionRelationship.Values[0])).Value;
				string relationship = ((StringNode)(factionRelationship.Values[3])).Value;

				var f = (from faction in Factions
						 where faction.Id == factionId
						 select faction).FirstOrDefault();
				if (f != null)
					f.Relationship = relationship;
			}
		}
	}
}
