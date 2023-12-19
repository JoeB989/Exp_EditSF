using EsfLibrary;
using System.Text;

namespace EsfHelper
{
	static public partial class Helper
	{
		static private void ArmyReport(ParentNode factionNode, StringBuilder report, ReportConfig cfg)
		{
			report.Append("  Armies (");
			if (cfg.OmitGarrisons)
				report.Append("garrisons omitted, ");
			report.AppendLine("player-named in quotes)");
			var armies = FindChild(factionNode, "ARMY_ARRAY");
			int armyIndex = 0;
			foreach (var armyNode in armies.Children)
			{
				var militaryForce = FindChild(armyNode, "MILITARY_FORCE");
				var militaryForceLegacy = FindChild(militaryForce, "MILITARY_FORCE_LEGACY");
				reportArmy(militaryForceLegacy, armyIndex, report, cfg.OmitGarrisons);
				armyIndex++;
			}

			report.AppendLine("  Legacy armies");
			var legacyPool = FindChild(factionNode, "MILITARY_FORCE_LEGACY_POOL");
			var legacies = FindChild(legacyPool, "LEGACIES");
			armyIndex = 0;
			foreach (var legacyNode in legacies.Children)
			{
				var militaryForceLegacy = legacyNode.Children[0];
				reportArmy(militaryForceLegacy, armyIndex, report, cfg.OmitGarrisons);
				armyIndex++;
			}
		}

		static private void reportArmy(ParentNode militaryForceLegacy, int armyIndex, StringBuilder report, bool omitGarrisons)
		{
			uint armyId = ((OptimizedUIntNode)militaryForceLegacy.Values[0]).Value;
			string armyName;
			bool isGarrison = GetArmyName(militaryForceLegacy, out armyName);
			if (isGarrison && omitGarrisons)
				return;

			var campaignSkills = FindChild(militaryForceLegacy, "CAMPAIGN_SKILLS");
			uint rank = ((OptimizedUIntNode)campaignSkills.Values[5]).Value + 1;    // 0-based
			uint experience = ((OptimizedUIntNode)campaignSkills.Values[6]).Value;

			report.AppendFormat("  [{0}] {1} id:{2} rank:{3} experience:{4}\n", armyIndex, armyName, armyId, rank, experience);

			// skills
			//report.AppendLine("    skills:");
			var skillsBlock = FindChild(campaignSkills, "CAMPAIGN_SKILLS_BLOCK");
			foreach (ParentNode skill in skillsBlock.Children)
			{
				string name = ((StringNode)skill.Values[0]).Value;
				uint level = ((OptimizedUIntNode)skill.Values[3]).Value;
				report.AppendFormat("      {0}  level:{1}\n", name, level);
			}

			// units - sibling of military force legacy, may not be present (for legacy armies)
			ParentNode parent = (ParentNode)militaryForceLegacy.Parent;
			var unitContainer = FindChild(parent, "UNIT_CONTAINER");
			if (unitContainer != null)
			{
				var unitsArray = unitContainer.Children[0];
				if (unitsArray.Children.Count > 0)
				{
					Dictionary<string, int> units = new Dictionary<string, int>();
					foreach (var unitEntry in unitsArray.Children)
					{
						var unit = unitEntry.Children[0];
						var unitRecordKey = FindChild(unit, "UNIT_RECORD_KEY");
						var unitType = ((StringNode)unitRecordKey.Values[0]).Value;

						if (units.ContainsKey(unitType))
							units[unitType]++;
						else
							units[unitType] = 1;
					}

					report.AppendFormat("      {0} unit", unitsArray.Children.Count);
					if (unitsArray.Children.Count > 1)
						report.Append("s");
					report.Append(":");
					reportUnits(units, report);
				}
			}

			// siege equipment if present
			var siege = FindChild(parent, "SIEGE");
			if (siege != null)
			{
				var siegeMgr = FindChild(siege, "SIEGE_EQUIPMENT_BUILD_MANAGER");
				var builtArray = ((EsfArrayNode<string>)(siegeMgr.Values[0])).Value;

				Dictionary<string, int> siegeEquipment = new Dictionary<string, int>();
				foreach (string eq in builtArray)
				{
					if (siegeEquipment.ContainsKey(eq))
						siegeEquipment[eq]++;
					else
						siegeEquipment[eq] = 1;
				}

				report.AppendFormat("      {0} siege equipment:", builtArray.Length);
				reportUnits(siegeEquipment, report);

				// siege build queue
				var buildQueue = FindChild(siegeMgr, "SIEGE_ITEM_ARRAY");
				if (buildQueue != null)
				{
					report.Append("      siege build queue:");
					int i = 0;
					foreach (var siegeItem in buildQueue.Children)
					{
						string name = ((StringNode)(siegeItem.Children[0]).Values[1]).Value;
						if (i > 0)
							report.Append(",");
						report.AppendFormat(" {0}", name);
						i++;
					}
					report.AppendLine();
				}
			}
		}

		static private void reportUnits(Dictionary<string, int> units, StringBuilder report)
		{
			int i = 0;
			foreach (KeyValuePair<string, int> unit in units)
			{
				if (i > 0)
				{
					if ((i & 3) == 0)
						report.AppendFormat("\n               ");
					else
						report.AppendFormat(",");
				}
				report.AppendFormat(" {0} {1}", unit.Value, unit.Key);
				i++;
			}
			report.AppendLine();
		}

		static private bool GetArmyName(ParentNode militaryForceLegacy, out string name)
		{
			const string GarrisonName = "random_localisation_strings_string_military_force_legacy_name_garrison_army";
			const string LegioHeader = "region_groups_localised_name_roman_legacy_generic_";

			var localization = FindChild(militaryForceLegacy, "CAMPAIGN_LOCALISATION");
			name = ((StringNode)localization.Values[0]).Value;
			bool mrGarrison = false;
			if (name == GarrisonName)
			{
				name = "Garrison Army";
				mrGarrison = true;
			}
			else if (string.IsNullOrWhiteSpace(name))
				name = "\"" + ((StringNode)localization.Values[1]).Value + "\"";
			else if (name.StartsWith(LegioHeader))
			{
				// TEMPORARY: convert a stock name to "Legio nn <name>"
				uint legio = ((OptimizedUIntNode)militaryForceLegacy.Values[4]).Value;
				string actual = name.Substring(LegioHeader.Length, 1).ToUpper() + name.Substring(LegioHeader.Length + 1); ;
				name = string.Format("Legio {0} {1}", ToRoman(legio), actual);
			}

			return mrGarrison;
		}
	}
}
