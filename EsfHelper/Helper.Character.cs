﻿using EsfLibrary;
using System.Text;

namespace EsfHelper
{
	static public partial class Helper
	{
		static private void CharacterReport(ParentNode factionNode, StringBuilder report,
			uint game_year, uint game_month, List<FamilyMember> familyTree)
		{
			// find governors (all provinces and factions)
			Dictionary<uint, string> governors = new Dictionary<uint, string>();
			var worldNode = factionNode.Parent.Parent.Parent; // hack
			var provinceManager = FindChild((ParentNode)worldNode, "PROVINCE_MANAGER");
			var provinceArray = provinceManager.Children[0];
			foreach (var province in provinceArray.Children)
			{
				string provinceName = ((StringNode)province.Values[0]).Value;
				var factionProvinceManager = province.Children[0];//findChild(province, "FACTION_PROVINCE_MANAGER");
				var fpmArray = factionProvinceManager.Children[0];
				foreach (var fpm in fpmArray.Children)
				{
					uint governorId = ((OptimizedUIntNode)(fpm.Children[0]).Values[6]).Value;
					if (governorId > 0)
						governors.Add(governorId, provinceName);
				}
			}

			// find characters that have an office
			var government = FindChild(factionNode, "GOVERNMENT");
			var postsNode = government.Children[0];
			Dictionary<uint, string> officers = new Dictionary<uint, string>();
			foreach (var posts in postsNode.Children)
			{
				foreach (var post in posts.Children)
				{
					string postName = ((StringNode)(post.Value[1])).Value;
					uint charId = ((OptimizedUIntNode)(post.Value[2])).Value;
					if (charId > 0)
						officers.Add(charId, postName);
				}
			}

			// TODO: find characters that have a spouse
			string factionName = ((StringNode)(factionNode.AllNodes[1])).Value;
			var factionFamily = from member in familyTree
								where member.Faction == factionName
								select member;

			report.AppendLine("  Characters");
			var characters = FindChild(factionNode, "CHARACTER_ARRAY");
			int charIndex = 0;
			foreach (var charNode in characters.Children)
			{
				var character = charNode.Children[0];
				reportCharacter(character, charIndex, report,
					game_year, game_month, officers, governors, factionFamily);
				charIndex++;
			}

			// candidates from CHARACTER_RECRUITMENT_POOL
			report.AppendLine("  Candidates");
			var recruitmentPool = FindChild(factionNode, "CHARACTER_RECRUITMENT_POOL_MANAGER");
			var poolBlock = recruitmentPool.Children[0].Children[0].Children[0].Children[0];
			charIndex = 0;
			foreach (var poolEntry in poolBlock.Children)
			{
				var character = poolEntry.Children[0];
				reportCharacter(character, charIndex, report,
					game_year, game_month, officers, governors, factionFamily);
				charIndex++;
			}
		}

		static private void reportCharacter(ParentNode character, int charIndex, StringBuilder report,
			uint game_year, uint game_month,
			Dictionary<uint, string> officers, Dictionary<uint, string> governors,
			IEnumerable<FamilyMember> factionFamily)
		{
			uint charId = ((OptimizedUIntNode)character.Values[0]).Value;

			if (character.Values.Count > 11)    // candidates will not have all these
			{
				float important_value = ((OptimizedFloatNode)character.Values[11]).Value;
				bool showCharacter = important_value > 5.5f;    // not sure why, but seems correct so far; 10 = real character, 5 = not real
				if (!showCharacter)
					return;
			}

			var details = FindChild(character, "CHARACTER_DETAILS");
			uint influence = ((OptimizedUIntNode)details.Values[15]).Value;
			var traitsNode = FindChild(details, "TRAITS");
			var traitNode = traitsNode.Children[0];

			//uint NOT_sex_enum = ((OptimizedUIntNode)details.Values[4]).Value;
			//string NOT_sex;
			//switch (NOT_sex_enum)
			//{
			//	case 0: NOT_sex = "deceased"; break;
			//	case 1: NOT_sex = "male"; break;
			//	case 2: NOT_sex = "female"; break;
			//	default: NOT_sex = string.Format("sex={0}", NOT_sex_enum); break;
			//}

			var dateNodes = findChildren(details, "DATE");
			uint birth_year = ((OptimizedUIntNode)dateNodes[0].Values[0]).Value;
			uint birth_month = ((OptimizedUIntNode)dateNodes[0].Values[2]).Value;
			int age = computeAge(birth_year, birth_month, game_year, game_month);
			//int age = (int)game_year - (int)birth_year; // int just in case goes negative
			//if (game_month < birth_month)
			//	age--;

			// 2nd DATE node seems to be the date booted from army
			uint boot_year = ((OptimizedUIntNode)dateNodes[1].Values[0]).Value;
			bool booted = boot_year > 0;

			// A booted character has no LOS but is not deceased
			var lineOfSight = FindChild(character, "LINE_OF_SIGHT");
			bool deceased = false;
			if (lineOfSight != null) // candidates won't have this
			{
				bool has_los = ((OptimizedBoolNode)lineOfSight.Value[0]).Value;
				deceased = !has_los && !booted;
			}

			string nameKey = readNameKey(details);
			var familyMember = (from member in factionFamily
								where member.NameKey == nameKey
								select member).FirstOrDefault();
			if (familyMember != null)
			{
				;
			}
			string name;
			if (!TddHardcodedNames.TryGetValue(nameKey, out name))
				name = nameKey;

			string politicalParty = null;
			if (character.Values.Count > 1)
				politicalParty = ((StringNode)character.Values[1]).Value;
			else
				politicalParty = "candidate";

			string occupation = ((StringNode)details.Values[16]).Value;
			//if (string.IsNullOrWhiteSpace(occupation))
			//    occupation = "candidate"; // TODO: not always right (e.g. for wife)
			string office = officers.ContainsKey(charId) ? officers[charId] : null;
			string governorOf = null;
			governors.TryGetValue(charId, out governorOf);

			var campaignSkills = FindChild(details, "CAMPAIGN_SKILLS");
			uint rank = 1 + ((OptimizedUIntNode)campaignSkills.Value[5]).Value;

			report.AppendFormat("  [{0}] {1} id:{2} (rank {3} {4})", charIndex, name, charId, rank, politicalParty);
			report.AppendFormat(", {0}", occupation);
			if (governorOf != null)
				report.AppendFormat(", Governor of {0}", governorOf);
			if (office != null)
				report.AppendFormat(", {0}", office);
			if (deceased)
				report.Append(" DECEASED");
			report.AppendLine();

			// TEMP: for debugging
			//report.AppendFormat("      Debug info: id:{0} {1}\n", charId, nameKey);

			// character_details / agent_attributes(for general or governor)
			//	cunning(TDD management)[0] "subterfuge"
			//	zeal(TDD leadership)[1] "zeal"
			// authority(TDD command)[2] "authority"
			var attributes = getAgentAttributes(details);
			int attr;

			// add other stuff to help disambiguate when name is wrong
			report.AppendFormat("      Age {0}  Influence {1}", age, influence);
			if (attributes.TryGetValue("authority", out attr))
				report.AppendFormat("  Authority(Command) {0}", attr);
			if (attributes.TryGetValue("subterfuge", out attr))
				report.AppendFormat("  Cunning(Management) {0}", attr);
			if (attributes.TryGetValue("zeal", out attr))
				report.AppendFormat("  Zeal(Leadership) {0}", attr);
			report.AppendLine();

			foreach (RecordEntryNode trait in traitNode.Children)
			{
				report.AppendFormat("      {0} = {1}\n", trait.Values[0], trait.Values[1]);
			}
		}

		static private string readNameKey(ParentNode detailsNode)
		{
			var nameNode = FindChild(detailsNode, "CHARACTER_NAME");
			var namesBlock = nameNode.Children[0];
			var block0 = namesBlock.Children[0];
			var localization0 = block0.Children[0];
			string nameKey = ((StringNode)localization0.Value[0]).Value;
			return nameKey;
		}

		static private Dictionary<string, int> getAgentAttributes(ParentNode detailsNode)
		{
			Dictionary<string, int> attributes = new Dictionary<string, int>();

			var attributesNode = FindChild(detailsNode, "AGENT_ATTRIBUTES");
			foreach (var attrib in attributesNode.Children)
			{
				string name = ((StringNode)attrib.Value[0]).Value;
				int value = ((OptimizedIntNode)attrib.Value[1]).Value;
				attributes[name] = value;
			}

			return attributes;
		}

		static private int computeAge(uint birth_year, uint birth_month, uint game_year, uint game_month)
		{
			int age = (int)game_year - (int)birth_year; // int just in case goes negative
			if (game_month < birth_month)
				age--;
			return age;
		}

		[System.Diagnostics.DebuggerDisplay("{Name}")]
		private class FamilyMember
		{
			public uint MemberId;
			public string Faction;
			public string NameKey;
			public string Name;
			public uint CharId;
			//public string Name;
			public uint BirthYear;
			public uint BirthMonth;
			//public int Age;
			public ParentNode raw;
		}

		static private List<FamilyMember> ScanFamilyTree(ParentNode FamilyTreeNode)
		{
			List<FamilyMember> familyTree = new List<FamilyMember>();
#if NOT_READY
			foreach (ParentNode memberNode in FamilyTreeNode.Children)
			{
				bool realPerson = ((OptimizedBoolNode)memberNode.Values[1]).Value;
				if (realPerson)
				{
					// all factions, in case we cross-married
					FamilyMember member = new FamilyMember();
					member.raw = memberNode;
					member.MemberId = ((OptimizedUIntNode)memberNode.Values[0]).Value;
					member.Faction = ((StringNode)memberNode.Values[2]).Value;

					var detailsNode = findChild(memberNode, "CHARACTER_DETAILS");
					member.CharId = ((OptimizedUIntNode)detailsNode.Values[15]).Value;
					member.NameKey = readNameKey(detailsNode);
					// somne have empty name key - should we reject those?
					if (string.IsNullOrEmpty(member.NameKey))
						continue;
					hardcoded_names.TryGetValue(member.NameKey, out member.Name);

					var dateNode = findChild(memberNode, "DATE");
					member.BirthYear = ((OptimizedUIntNode)dateNode.Values[0]).Value;
					member.BirthMonth = ((OptimizedUIntNode)dateNode.Values[2]).Value;

					familyTree.Add(member);
				}
			}
#endif // NOT_READY
			return familyTree;
		}
	}
}
