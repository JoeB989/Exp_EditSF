using EsfLibrary;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
			Dictionary<uint, string> officers = new Dictionary<uint, string>();
			var government = FindChild(factionNode, "GOVERNMENT");
			if (government != null)
			{
				var postsNode = government.Children[0];
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
			}

			report.AppendLine("  Characters");
			var characterArray = FindChild(factionNode, "CHARACTER_ARRAY");
			int charIndex = 0;
			List<CharacterInfo> characters = new List<CharacterInfo>();
			// two passes; 1) read all chars, 2) report each char
			foreach (var charNode in characterArray.Children)
			{
				var characterNode = charNode.Children[0];
				var character = GetCharacterInfo(characterNode, game_year, game_month,
				officers, governors, familyTree);

				character.CharIndex = charIndex++;
				character.CharNode = characterNode;

				characters.Add(character);
			}
			foreach (var character in characters)
			{
				reportCharacter(character.CharNode, character, report,
					game_year, game_month, officers, governors, familyTree, characters);
			}

			// candidates from CHARACTER_RECRUITMENT_POOL
			var recruitmentPool = FindChild(factionNode, "CHARACTER_RECRUITMENT_POOL_MANAGER");
			if (recruitmentPool != null)
			{
				report.AppendLine("  Candidates");
				var poolBlock = recruitmentPool.Children[0].Children[0].Children[0].Children[0];
				charIndex = 0;
				List<CharacterInfo> candidates = new List<CharacterInfo>();
				foreach (var poolEntry in poolBlock.Children)
				{
					var characterNode = poolEntry.Children[0];
					var candidate = GetCharacterInfo(characterNode, game_year, game_month,
					officers, governors, familyTree);

					candidate.CharIndex = charIndex++;
					candidate.CharNode = characterNode;
					candidates.Add(candidate);
				}
				foreach (var candidate in candidates)
				{
					reportCharacter(candidate.CharNode, candidate, report,
						game_year, game_month, officers, governors, familyTree, characters);
				}
			}
		}

		public struct CharacterInfo
		{
			public int CharIndex;	// used for reporting
			public ParentNode CharNode;

			public bool ShowCharacter;
			public uint CharId;
			public bool Deceased;
			public bool Immortal;
			public uint Influence;
			public string Name;
			public uint Rank;
			public int Authority;
			public int Subterfuge;
			public int Zeal;
			public string CurrentFaction;
			public string OriginalFaction;
			public uint BirthYear;
			public uint BirthMonth;
			public int Age;
			public string Occupation;
			public string PoliticalParty;
			public string Office;
			public string GovernorOf;

			//public bool InFamilyTree;
			public FamilyMember FamilyTreeEntry;
		}

		static private void reportCharacter(ParentNode characterNode, CharacterInfo character, StringBuilder report,
			uint game_year, uint game_month,
			Dictionary<uint, string> officers,
			Dictionary<uint, string> governors,
			List<FamilyMember> familyTree, List<CharacterInfo> characters)
		{
			if (!character.ShowCharacter)
				return;

			// TODO: find characters that have the 'ruler' political party but are not parented in the family trree
			//string factionName = ((StringNode)(factionNode.AllNodes[1])).Value;
			//var factionFamily = from member in familyTree
			//					where member.SourceFaction == factionName
			//					select member;

			// Validation for hidden confederated generals
			// These are lost characters that should be rehomed into Other Nobles by giving them the 'council' political party
			//
			// - other nobles have
			//		family parent id 0
			//		_council party
			// - valid family tree members have
			//		family parent id set
			//		_ruler party
			//		parent id is a character in this faction (may be dead)
			// - hidden/lost confederated characters
			//		_ruler party
			//		family parent id set
			//		parent id is a character not in this faction (or 0)
			// - visible confederated characters
			//		_council party
			//		family parent id 0 (or set, doesn't matter)
			//
			string stringFamilyTree = string.Empty;
			if (character.PoliticalParty.ToLower().Contains("_ruler"))
			{
				bool parentInFaction = (from c in characters
										where (character.FamilyTreeEntry != null) && 
											  (c.CharId == character.FamilyTreeEntry.ParentId)
										select c).Count() > 0;
				// TODO: check if
				//  parent is a live character in this faction
				//  parent is a deade character in family, from this faction
				if (!parentInFaction)
					stringFamilyTree = "(LOST)";
			}
			//if (character.FamilyTreeEntry != null)
			//	stringFamilyTree = string.Format(" (family tree[{0}])", character.FamilyTreeEntry.NodeIndex);
			//if (character.InFamilyTree)
			//	stringFamilyTree = " (in family tree)";

			string confederated_from = string.Empty;
			if (character.CurrentFaction != character.OriginalFaction)
				confederated_from = string.Format(" [from {0}]", character.OriginalFaction);
			report.AppendFormat("  [{0}] {1} id:{2} (rank {3} {4})", character.CharIndex,
				character.Name, character.CharId, character.Rank, character.Occupation);

			report.AppendFormat(", {0}{1}{2}", character.PoliticalParty, confederated_from, stringFamilyTree);
			if (character.GovernorOf != null)
				report.AppendFormat(", Governor of {0}", character.GovernorOf);
			if (character.Office != null)
				report.AppendFormat(", {0}", character.Office);
			if (character.Deceased)
				report.Append(" DECEASED");
			report.AppendLine();

			// TEMP: for debugging
			//report.AppendFormat("      Debug info: id:{0} {1}\n", charId, nameKey);

			// add other stuff to help disambiguate when name is wrong
			string immortal_string = character.Immortal ? "IMMORTAL  " : "";
			report.AppendFormat("      Age {0}  {1}Influence {2}", character.Age, immortal_string, character.Influence);
			report.AppendFormat("  Authority(Command) {0}", character.Authority);
			report.AppendFormat("  Cunning(Management) {0}", character.Subterfuge);
			report.AppendFormat("  Zeal(Leadership) {0}", character.Zeal);
			report.AppendLine();

			var details = FindChild(characterNode, "CHARACTER_DETAILS");
			var traitsNode = FindChild(details, "TRAITS");
			var traitNode = traitsNode.Children[0];

			foreach (RecordEntryNode trait in traitNode.Children)
			{
				report.AppendFormat("      {0} = {1}\n", trait.Values[0], trait.Values[1]);
			}
		}

		static public CharacterInfo GetCharacterInfo(ParentNode characterNode,
			uint game_year, uint game_month,
			Dictionary<uint, string> officers,
			Dictionary<uint, string> governors,
			List<FamilyMember> familyTree)
		{
			var character = new CharacterInfo();
			character.CharId = ((OptimizedUIntNode)characterNode.Values[0]).Value;

			character.ShowCharacter = true;
			if (characterNode.Values.Count > 11)    // candidates will not have all these
			{
				float important_value = ((OptimizedFloatNode)characterNode.Values[11]).Value;
				character.ShowCharacter = important_value > 5.5f;    // not sure why, but seems correct so far; 10 = real character, 5 = not real
				if (!character.ShowCharacter)
					return character;
			}

			var details = FindChild(characterNode, "CHARACTER_DETAILS");
			character.Influence = ((OptimizedUIntNode)details.Values[15]).Value;
			character.Immortal = ((OptimizedBoolNode)details.Values[20]).Value;

			string nameKey = readNameKey(details);
			if (string.IsNullOrWhiteSpace(nameKey))
			{
				character.ShowCharacter = false;
				return character;
			}
			if (!TddHardcodedNames.TryGetValue(nameKey, out character.Name))
				character.Name = nameKey;

			var test_attributes = getAgentAttributes(details);
			test_attributes.TryGetValue("authority", out character.Authority);
			test_attributes.TryGetValue("subterfuge", out character.Subterfuge);
			test_attributes.TryGetValue("zeal", out character.Zeal);

			// skip placeholders with influence 0
			// NOTE: Deceased also have influence 0
			// (if this isn't distinct enough, they also have command/management/leadership 0 as well
			character.Deceased = false;
			if (character.Influence <= 0)
			{

				if (character.Authority == 0)
				{
					character.ShowCharacter = false;
					//return character;             // unused placeholder
				}
										//else
										//	deceased = true;	// deceased - nope, not valid
			}

			// NOTE: deceased info is actually stored in family_tree, can look up by faction
			// Deceased characters are no longer in the faction's CHARACTER_ARRAY so don't look for them there

			character.CurrentFaction = ((StringNode)details.Values[1]).Value;
			character.OriginalFaction = ((StringNode)details.Values[26]).Value;

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
			character.BirthYear = ((OptimizedUIntNode)dateNodes[0].Values[0]).Value;
			character.BirthMonth = ((OptimizedUIntNode)dateNodes[0].Values[2]).Value;
			character.Age = computeAge(character.BirthYear, character.BirthMonth, game_year, game_month);

			// 2nd DATE node seems to be the date booted from army
			uint boot_year = ((OptimizedUIntNode)dateNodes[1].Values[0]).Value;
			bool booted = boot_year > 0;

			character.FamilyTreeEntry = (from member in familyTree
										 where member.CharId == character.CharId
										 select member).FirstOrDefault();
			//character.InFamilyTree = (familyMember != null) && familyMember.ThirdLastBool;

			if (characterNode.Values.Count > 1)
				character.Occupation = ((StringNode)characterNode.Values[1]).Value;
			else
				character.Occupation = "candidate";

			character.PoliticalParty = ((StringNode)details.Values[16]).Value;
			//if (string.IsNullOrWhiteSpace(occupation))
			//    occupation = "candidate"; // TODO: not always right (e.g. for wife)
			character.Office = officers.ContainsKey(character.CharId) ? officers[character.CharId] : null;
			character.GovernorOf = null;
			governors.TryGetValue(character.CharId, out character.GovernorOf);

			if ((character.GovernorOf != null) && (character.Occupation == "general"))
				character.Occupation = "governor";

			// A booted character has no LOS but is not deceased
#if NOT_CORRECT_YET // regular statesmen incorrectly show as deceased
			var lineOfSight = FindChild(character, "LINE_OF_SIGHT");
			bool deceased = false;
			if (lineOfSight != null) // candidates won't have this
			{
				bool has_los = ((OptimizedBoolNode)lineOfSight.Value[0]).Value;
				deceased = !has_los && !booted && (governorOf == null);
			}
#endif // NOT_CORRECT_YET

			var campaignSkills = FindChild(details, "CAMPAIGN_SKILLS");
			character.Rank = 1 + ((OptimizedUIntNode)campaignSkills.Value[5]).Value;

			return character;
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

		//[System.Diagnostics.DebuggerDisplay("{Name}")]
		public class FamilyMember
		{
			//public int NodeIndex;	// not needed, FamilyTree is ordered by CharId
			public uint CharId;
			public uint ParentId;
			public bool IsLost;
			public bool Deceased;
			public string SourceFaction;
			public string NameKey;
			public string Name;
			public uint MemberId;
			public string PoliticalParty;
			public bool ThirdLastBool;	// this one seems to be true occasioanlly, figure out what it means
			//public string Name;
			public uint BirthYear;
			public uint BirthMonth;
			//public int Age;
			public ParentNode raw;
		}

		static private List<FamilyMember> ScanFamilyTree(ParentNode FamilyTreeNode)
		{
			List<FamilyMember> familyTree = new List<FamilyMember>();
			int nodeIndex = 0;
			foreach (ParentNode memberNode in FamilyTreeNode.Children)
			{
				// all factions, in case we cross-married
				FamilyMember member = new FamilyMember();
				member.raw = memberNode;
				//member.NodeIndex = nodeIndex++;
				member.CharId = ((OptimizedUIntNode)memberNode.Values[0]).Value;
				member.ParentId = ((OptimizedUIntNode)memberNode.Values[4]).Value;

				// TODO: I DON'T THINK THIS IS RIGHT
				member.Deceased = ((OptimizedBoolNode)memberNode.Values[1]).Value;
				int index = 2;
				if (member.Deceased)
				{
					member.SourceFaction = ((StringNode)memberNode.Values[index++]).Value;
				}

				OptimizedBoolNode thirdLast = memberNode.Values[memberNode.Values.Count - 3] as OptimizedBoolNode;
				if (thirdLast != null)
					member.ThirdLastBool = thirdLast.Value;

				// should only be if deceased
				var detailsNode = FindChild(memberNode, "CHARACTER_DETAILS");
				if (detailsNode != null)
				{
					member.MemberId = ((OptimizedUIntNode)detailsNode.Values[15]).Value;
					member.PoliticalParty = ((StringNode)detailsNode.Values[16]).Value;
					member.NameKey = readNameKey(detailsNode);
					// some have empty name key - should we reject those?
					if (! string.IsNullOrEmpty(member.NameKey))
						TddHardcodedNames.TryGetValue(member.NameKey, out member.Name);
				}

				var dateNode = FindChild(memberNode, "DATE");
				member.BirthYear = ((OptimizedUIntNode)dateNode.Values[0]).Value;
				member.BirthMonth = ((OptimizedUIntNode)dateNode.Values[2]).Value;

				familyTree.Add(member);
			}
			return familyTree;
		}
	}
}
