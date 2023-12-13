using EsfLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EsfControl
{
	static internal partial class Helper
	{
		/// <summary>
		/// Fix out-of-order character skills
		/// Caution: hard-coded TDD knowledge
		/// </summary>
		/// <remarks>
		/// TDD characters skills CAMPAIGN_SKILLS_BLOCK are ordered like this
		///    rom_general_follower_dwarves_erebor_forager_2   8  1  1
		///    rom_general_follower_dwarves_erebor_forager_1   8  0  1
		/// but need to be reversed in order to load properly
		///    rom_general_follower_dwarves_erebor_forager_1   8  0  1
		///    rom_general_follower_dwarves_erebor_forager_2   8  1  1
		///
		/// There may be up to 5 such skill entries
		/// We need to rearrange the order if wrong, but not change it if correct
		/// </remarks>
		/// <param name="rootNode"></param>
		static private void FixCharacterSkills(EsfNode rootNode)
		{
			// TEMP: For now, only do player faction
			ParentNode[] factions = new ParentNode[] { getFactionNode(rootNode, 0) };

			StringBuilder report = new StringBuilder();
			bool any = false;
			for (int f = 0; f < factions.Length; f++)
			{
				var characters = findChild(factions[f].Children[0], "CHARACTER_ARRAY");
				int charIndex = 0;
				foreach (var charNode in characters.Children)
				{
					var character = charNode.Children[0];
					any |= fixCharacterSkills(f, charIndex, character, report);
					charIndex++;
				}

				// TODO: also need to address army skills??

			}

			if (!any)
				report.AppendLine("No character skills needed reordering");

			var ret = MessageBox.Show(report.ToString(), "Click OK to copy report to clipboard", MessageBoxButtons.OKCancel);
			if (ret == DialogResult.OK)
			{
				Clipboard.SetText(report.ToString());
			}
		}

		private class MoveSkill
		{
			public string BaseName;     // rom_general_follower_dwarves_erebor_forager
			public List<OneSkill> SkillNodes;
		}
		private class OneSkill
		{
			public string Name;         // rom_general_follower_dwarves_erebor_forager_2
			public uint Number;         // 2
			public ParentNode Node;
			public int NodeIndex;
		}

		static private bool fixCharacterSkills(int factionIndex, int characterIndex, ParentNode character, StringBuilder report)
		{
			string[] skillsHierarchy =
			{
				"CHARACTER_DETAILS",
				"CAMPAIGN_SKILLS",
				"CAMPAIGN_SKILLS_BLOCK",
			};
			var skillsBlock = WalkChildren(character, skillsHierarchy);
			bool any = false;
			if (skillsBlock == null)
				return any;

			// categorize skills
			List<MoveSkill> skills = new List<MoveSkill>();
			for (int i = 0; i < skillsBlock.Children.Count; i++)
			{
				ParentNode skillNode = skillsBlock.Children[i];
				string skillName = ((StringNode)skillNode.Values[0]).Value;
				int under = skillName.LastIndexOf('_');
				if (under > 0)
				{
					string baseName = skillName.Substring(0, under);
					string numString = skillName.Substring(under + 1);
					uint num;
					if (uint.TryParse(numString, out num))
					{
						// skill name fits the patterns we're looking for
						var found = (from skill in skills
									 where skill.BaseName == baseName
									 select skill).FirstOrDefault();
						if (found == null)
						{
							found = new MoveSkill();
							found.BaseName = baseName;
							found.SkillNodes = new List<OneSkill>();
							skills.Add(found);
						}
						OneSkill newSkill = new OneSkill();
						newSkill.Name = skillName;
						newSkill.Number = num;
						newSkill.Node = skillNode;
						newSkill.NodeIndex = i;
						found.SkillNodes.Add(newSkill);
					}
				}
			}

			// determine which skills need reordering
			foreach (var skillSet in skills)
			{
				if (skillSet.SkillNodes.Count > 1)
				{
					// numbers should be in increasing node-index order; those that aren't need to move
					var ordered = from skillNode in skillSet.SkillNodes
								  orderby skillNode.Number
								  select skillNode;
					bool needsFixing = false;
					OneSkill nodePrev = ordered.ElementAt(0);
					for (int i = 1; (i < ordered.Count()) && (!needsFixing); i++)
					{
						OneSkill nodeI = ordered.ElementAt(i);
						if (nodeI.NodeIndex < nodePrev.NodeIndex)
						{
							// report the issue
							//   Faction[n] Character[n] skills out of order:
							//		[n]actual ==> [n]desired
							//		[n]list       [n]list
							if (!any)
							{
								report.AppendFormat("Faction[{0}] Character[{1}] skills out of order:\n", factionIndex, characterIndex);
								any = true;
							}
							string arrow = "==>";
							for (int n = 0; n < skillSet.SkillNodes.Count; n++)
							{
								OneSkill actual = skillSet.SkillNodes[n];
								OneSkill desired = ordered.ElementAt(n);
								report.AppendFormat("    [{0}]{1} {4} [{2}]{3}\n", actual.NodeIndex, actual.Name, actual.NodeIndex, desired.Name, arrow);
								arrow = "   ";
							}
							needsFixing = true; // also breaks out of the for loop
						}
						nodePrev = nodeI;
					}

					if (needsFixing)
					{
						// Remove and reinsert these skills in the right order
						// Algorithm is similar to EditEsfComponent::MoveNode but for multiple nodes
						int insertAt = skillSet.SkillNodes[0].NodeIndex;
						RecordArrayNode parent = (RecordArrayNode)skillsBlock;
						List<EsfNode> nodes = new List<EsfNode>(parent.Value);
						foreach (var skill in ordered)
						{
							nodes.Remove(skill.Node);
							nodes.Insert(insertAt++, skill.Node);
						}
						parent.Value = nodes;
					}
				}
			}

			return any;
		}
	}
}
