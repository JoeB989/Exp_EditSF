using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using EsfLibrary;
using CommonDialogs;
using System.Text;
using System.Linq;

namespace EsfControl {
    public partial class EditEsfComponent : UserControl {
        public delegate void Selected(EsfNode node);
        public event Selected NodeSelected;

        TreeEventHandler treeEventHandler;

        EsfTreeNode rootNode;
        public EsfNode RootNode {
            get {
                return rootNode != null ? rootNode.Tag as EsfNode : null;
            }
            set {
                esfNodeTree.Nodes.Clear();
                if (value != null) {
                    rootNode = new EsfTreeNode(value as ParentNode);
                    rootNode.ShowCode = ShowCode;
                    esfNodeTree.Nodes.Add(rootNode);
                    rootNode.Fill();
                    nodeValueGridView.Rows.Clear();
                    value.Modified = false;
                }
            }
        }

        bool showCode;
        public bool ShowCode {
            get { return showCode; }
            set {
                showCode = value;
                if (esfNodeTree.Nodes.Count > 0) {
                    (esfNodeTree.Nodes[0] as EsfTreeNode).ShowCode = value;
                    nodeValueGridView.Columns["Code"].Visible = value;
                }
            }
        }

        public EditEsfComponent() {
            InitializeComponent();
            nodeValueGridView.Rows.Clear();

            treeEventHandler = new TreeEventHandler(nodeValueGridView, this);
            esfNodeTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(treeEventHandler.FillNode);
            esfNodeTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(treeEventHandler.TreeNodeSelected);

            nodeValueGridView.CellValidating += new DataGridViewCellValidatingEventHandler(validateCell);
            nodeValueGridView.CellEndEdit += new DataGridViewCellEventHandler(cellEdited);

            MouseHandler mouseHandler = new MouseHandler();
            esfNodeTree.MouseUp += new MouseEventHandler(mouseHandler.ShowContextMenu);
            
            nodeValueGridView.CellClick += CellClicked;
        }
        
        private void CellClicked(object sender, DataGridViewCellEventArgs args) {
            if (args.ColumnIndex == 1) {
                Console.WriteLine("editing {0}", nodeValueGridView.Rows[args.RowIndex].Cells[0].Value);
            }
        }

        private void validateCell(object sender, DataGridViewCellValidatingEventArgs args) {
            EsfNode valueNode = nodeValueGridView.Rows[args.RowIndex].Tag as EsfNode;
            if (valueNode != null) {
                string newValue = args.FormattedValue.ToString();
                try {
                    if (args.ColumnIndex == 0 && newValue != valueNode.ToString()) {
                        valueNode.FromString(newValue);
                    }
                } catch {
                    Console.WriteLine("Invalid value {0}", newValue);
                    args.Cancel = true;
                }
            } else {
                nodeValueGridView.Rows[args.RowIndex].ErrorText = "Cannot edit this value";
                // args.Cancel = true;
            }
        }
        private void cellEdited(object sender, DataGridViewCellEventArgs args) {
            nodeValueGridView.Rows[args.RowIndex].ErrorText = String.Empty;
        }
        
        public void NotifySelection(EsfNode node) {
            if (NodeSelected != null) {
                NodeSelected(node);
            }
        }
        
        public string SelectedPath {
            get {
                EsfNode node = esfNodeTree.SelectedNode.Tag as EsfNode;
                String result = NodePathCreator.CreatePath(node);
                return result;
            }
            set {
                string[] nodes = value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                TreeNode currentNode = rootNode;
                rootNode.Expand();
                for (int i = 1; i < nodes.Length; i++) {
                    currentNode = FindNode(currentNode.Nodes, nodes[i]);
                    if (currentNode != null) {
                        currentNode.Expand();
                    } else {
                        Console.WriteLine("Cannot find {0} in {1}", nodes[i], nodes[i-1]);
                        break;
                    }
                };
                if (currentNode != null) {
                    esfNodeTree.SelectedNode = currentNode;
                }
            }
        }
        private TreeNode FindNode(TreeNodeCollection collection, string pathSegment) {
            foreach(TreeNode node in collection) {
                if (node.Text.Equals(pathSegment)) {
                    return node;
                }
            }
            return null;
        }
    }

    public class MouseHandler
    {
        public delegate void NodeAction(EsfNode node);

        public void ShowContextMenu(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            TreeView treeView = sender as TreeView;
            if (e.Button == MouseButtons.Right && treeView != null)
            {
                // Point where the mouse is clicked.
                Point p = new Point(e.X, e.Y);
                ContextMenuStrip contextMenu = new ContextMenuStrip();

                // Get the node that the user has clicked.
                TreeNode node = treeView.GetNodeAt(p);
                ParentNode selectedNode = (node != null) ? node.Tag as ParentNode : null;
                if (selectedNode != null && (node.Tag as EsfNode).Parent is RecordArrayNode)
                {
                    treeView.SelectedNode = node;

                    if (node.Text.StartsWith("FACTION_ARRAY"))
                    {
                        var item = CreateMenuItem("Faction Report ...", selectedNode, OneFactionReport);
                        contextMenu.Items.Add(item);
                    }

                    ToolStripItem toolItem = CreateMenuItem("Duplicate", selectedNode, CopyNode);
                    contextMenu.Items.Add(toolItem);
                    toolItem = CreateMenuItem("Delete", selectedNode, DeleteNode);
                    contextMenu.Items.Add(toolItem);
                    toolItem = CreateMenuItem("Move", selectedNode, MoveNode);
                    contextMenu.Items.Add(toolItem);
                }
                else if ((selectedNode != null) && (node.Text == "FACTION_ARRAY"))
                {
                    treeView.SelectedNode = node;
                    var item = CreateMenuItem("All Factions Economics ...", selectedNode, AllFactionEconomicsReport);
                    contextMenu.Items.Add(item);
					item = CreateMenuItem("All Factions Characters ...", selectedNode, AllFactionCharactersReport);
					contextMenu.Items.Add(item);
				}

				if (contextMenu.Items.Count != 0)
                {
                    contextMenu.Show(treeView, p);
                }
            }
        }

        private ToolStripMenuItem CreateMenuItem(String label, EsfNode node, NodeAction action)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(label);
            item.Click += new EventHandler(delegate (object s, EventArgs args) { action(node); });
            return item;
        }

        private void CopyNode(EsfNode node)
        {
            ParentNode toCopy = node as ParentNode;
            ParentNode copy = toCopy.CreateCopy() as ParentNode;
            if (copy != null)
            {
                ParentNode parent = toCopy.Parent as ParentNode;
                if (parent != null)
                {
                    List<EsfNode> nodes = new List<EsfNode>(parent.Value);
                    SetAllModified(copy);
                    int insertAt = parent.Children.IndexOf(toCopy) + 1;
                    nodes.Insert(insertAt, copy);
#if DEBUG
                    Console.Out.WriteLine("new list now {0}", string.Join(",", nodes));
#endif
                    // copy.Modified = true;
                    // copy.AllNodes.ForEach(n => n.Modified = true);
                    parent.Value = nodes;
#if DEBUG
                }
                else
                {
                    Console.WriteLine("no parent to add to");
#endif
                }
#if DEBUG
            }
            else
            {
                Console.WriteLine("couldn't create copy");
#endif
            }
        }

        private void SetAllModified(ParentNode node)
        {
            node.Modified = true;
            node.Children.ForEach(n => SetAllModified(n));
        }

        private void DeleteNode(EsfNode node)
        {
            RecordArrayNode parent = node.Parent as RecordArrayNode;
            if (parent != null)
            {
                List<EsfNode> nodes = new List<EsfNode>(parent.Value);
                nodes.Remove(node);
                parent.Value = nodes;
            }
        }

        private void MoveNode(EsfNode node)
        {
            RecordArrayNode parent = node.Parent as RecordArrayNode;
            if (parent != null)
            {
                InputBox input = new InputBox
                {
                    Input = "Move to index"
                };
                if (input.ShowDialog() == DialogResult.OK)
                {
                    int moveToIndex = -1;
                    List<EsfNode> nodes = new List<EsfNode>(parent.Value);
                    if (int.TryParse(input.Input, out moveToIndex))
                    {
                        if (moveToIndex >= 0 && moveToIndex < nodes.Count)
                        {
                            nodes.Remove(node);
                            nodes.Insert(moveToIndex, node);
#if DEBUG
                            Console.Out.WriteLine("new list now {0}", string.Join(",", nodes));
#endif
                            parent.Value = nodes;
                        }
                        else
                        {
                            MessageBox.Show(string.Format("Entry only valid between 0 and {0}", nodes.Count - 1),
                                       "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Enter index (between 0 and {0})", nodes.Count - 1),
                                        "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

		private void AllFactionEconomicsReport(EsfNode factionArrayNode)
		{
			AllFactionsReport(factionArrayNode, new ReportConfig() { EconomicReport = true });
		}

		private void AllFactionCharactersReport(EsfNode factionArrayNode)
		{
			AllFactionsReport(factionArrayNode, new ReportConfig() { CharacterReport = true });
		}

		private void AllFactionsReport(EsfNode factionArrayNode, ReportConfig cfg)
        {
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

        private void OneFactionReport(EsfNode factionEntryNode)
        {
            ParentNode worldNode = (ParentNode)factionEntryNode.Parent.Parent;
            ParentNode FamilyTreeNode = findChild(worldNode, "FAMILY_TREE");
            List<FamilyMember> familyTree = ScanFamilyTree(FamilyTreeNode);

            ParentNode factionNode = ((ParentNode)factionEntryNode).Children[0];
            var nodes = new ParentNode[] { factionNode };
            var cfg = new ReportConfig() {
				EconomicReport = true,
				CharacterReport = true,
				ArmyReport = true,
				OmitGarrisons = true,
			};
            ShowFactionsReport(nodes, familyTree, cfg);
        }

        private struct ReportConfig
        {
            public bool EconomicReport;
            public bool CharacterReport;
			public bool ArmyReport;
			public bool OmitGarrisons;
		}


		private void ShowFactionsReport(ParentNode[] nodes, List<FamilyMember> familyTree, ReportConfig cfg)
		{
			// hack to find the root node
			EsfNode rootNode = nodes[0];
			while (rootNode.Parent != null)
				rootNode = rootNode.Parent;

			string title = System.Windows.Forms.Application.OpenForms[0].Text; // hack
			int dotsave = title.IndexOf(".save", System.StringComparison.OrdinalIgnoreCase);
			string savefile = (dotsave > 0) ? title.Substring(0, dotsave + 5) : title;

			var saveGameHeader = findChild((ParentNode)rootNode, "SAVE_GAME_HEADER");
			string playerFaction = ((StringNode)saveGameHeader.Values[0]).Value;
			uint turn = ((OptimizedUIntNode)saveGameHeader.Values[2]).Value;
			var dateNode = findChild(saveGameHeader, "DATE");
			uint year = ((OptimizedUIntNode)dateNode.Values[0]).Value;
			uint month = ((OptimizedUIntNode)dateNode.Values[2]).Value; // 0-based
			string monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName((int)month + 1);
			uint display_year = year - 752; // hack guess

			StringBuilder report = new StringBuilder();
			report.AppendFormat("Save file: {0}\n    Turn {1}, Year {2}, Month {3}\n    Player faction: {4}\n",
				savefile, turn, display_year, monthName, playerFaction);

			foreach (var factionNode in nodes)
			{
				buildFactionReport(factionNode, report, year, month, familyTree, cfg);
			}

			var ret = MessageBox.Show(report.ToString(), "Click OK to copy report to clipboard", MessageBoxButtons.OKCancel);
			if (ret == DialogResult.OK)
			{
				Clipboard.SetText(report.ToString());
			}
		}

		private void buildFactionReport(ParentNode factionNode, StringBuilder report,
			uint game_year, uint game_month, List<FamilyMember> familyTree, ReportConfig cfg)
		{
			string factionName = ((StringNode)(factionNode.AllNodes[1])).Value;
			report.AppendFormat("Faction: {0}\n", factionName);

			if (cfg.EconomicReport)
				EconomicReport(factionNode, report);

			if (cfg.CharacterReport)
				CharacterReport(factionNode, report, game_year, game_month, familyTree);

			if (cfg.ArmyReport)
				ArmyReport(factionNode, report, cfg);
		}

		private void EconomicReport(ParentNode factionNode, StringBuilder report)
		{
			var economics = findChild(factionNode, "FACTION_ECONOMICS");
			int treasury = ((OptimizedIntNode)economics.Values[0]).Value;
			report.AppendLine("  Economics:");
			report.AppendFormat("    Treasury: {0:#,#}\n", treasury);

			var history = economics.Children[0];
			int index = history.Children.Count - 1;
			if (index >= 0)
			{
				var econData = history.Children[index].Children[0];

				var array1 = ((OptimizedArrayNode<int>)econData.Values[1]).Value;
				int taxes = (int)array1.GetValue(0);
				if (taxes == 0)
				{
					report.AppendLine("    Faction has been destroyed");
					return;
				}

				report.AppendLine("    Projected this turn:");
				reportEconData(econData, report);

				if (index >= 1)
				{
					var priorData = history.Children[index - 1].Children[0];
					report.AppendLine("    Last turn:");
					reportEconData(priorData, report);
				}
			}
		}

		private void reportEconData(ParentNode econData, StringBuilder report)
		{
			var array1 = ((OptimizedArrayNode<int>)econData.Values[1]).Value;
			var array2 = ((OptimizedArrayNode<int>)econData.Values[2]).Value;
			var array4 = ((OptimizedArrayNode<int>)econData.Values[4]).Value;
			var array5 = ((OptimizedArrayNode<int>)econData.Values[5]).Value;

			int taxes = (int)array1.GetValue(0);
			int slavery = (int)array1.GetValue(1);
			int trade = (int)array1.GetValue(2);
			int otherIncome = (int)array2.GetValue(4);
			int maint = -(int)array5.GetValue(8);
			int armyUpkeep = -(int)array5.GetValue(1);
			int navyUpkeep = -(int)array5.GetValue(2); // TODO: guess, figure out
			int otherExpense = 0; // TODO: figure out
			int interest = (int)array1.GetValue(4);

			// Current turn treasury deductions
			// Don't show these as costs, since already deducted from treasury in-game.  Maybe report separately at some point.
			int construction = -(int)array4.GetValue(0);
			int agentCosts = 0; // TBD
			int recruitment = 0; // TBD

			int totalIncome = taxes + slavery + trade + interest + otherIncome;
			int totalExpense = armyUpkeep + navyUpkeep + maint + otherExpense;
			report.AppendFormat("      Taxes            {0,10:#,#}   Army Upkeep {1,10:#,#}\n", taxes, armyUpkeep);
			report.AppendFormat("      Slave population {0,10:#,#}   Navy Upkeep {1,10:#,#}\n", slavery, navyUpkeep);
			report.AppendFormat("      Trade            {0,10:#,#}   Maintenance {1,10:#,#}\n", trade, maint);
			report.AppendFormat("      Interest         {0,10:#,#}\n", interest);
#if NO	// Don't show construction as a cost, it's already deducted from treasury in-game.  Maybe report separately at some point.
			report.AppendFormat("                                    Construction     {0,10:#,#}\n", construction);
#endif // NO
			report.AppendFormat("      Other            {0,10:#,#}   Other       {1,10:#,#}\n", otherIncome, otherExpense);
			report.AppendLine  ("                       ----------               ----------");
			report.AppendFormat("                       {0,10:#,#}               {1,10:#,#}    = {2:#,#} net income\n", totalIncome, totalExpense, totalIncome + totalExpense);
		}

		private void CharacterReport(ParentNode factionNode, StringBuilder report,
			uint game_year, uint game_month, List<FamilyMember> familyTree)
		{
			// find governors (all provinces and factions)
			Dictionary<uint, string> governors = new Dictionary<uint, string>();
			var worldNode = factionNode.Parent.Parent.Parent; // hack
			var provinceManager = findChild((ParentNode)worldNode, "PROVINCE_MANAGER");
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
			var government = findChild(factionNode, "GOVERNMENT");
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
			var characters = findChild(factionNode, "CHARACTER_ARRAY");
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
			var recruitmentPool = findChild(factionNode, "CHARACTER_RECRUITMENT_POOL_MANAGER");
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

		private void reportCharacter(ParentNode character, int charIndex, StringBuilder report,
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

			var details = findChild(character, "CHARACTER_DETAILS");
			uint influence = ((OptimizedUIntNode)details.Values[15]).Value;
			var traitsNode = findChild(details, "TRAITS");
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
			var lineOfSight = findChild(character, "LINE_OF_SIGHT");
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
			if (!hardcoded_names.TryGetValue(nameKey, out name))
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

			var campaignSkills = findChild(details, "CAMPAIGN_SKILLS");
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

			// add other stuff to help disambiguate when name is wrong
			report.AppendFormat("      Age {0}  Influence {1}\n", age, influence);

			foreach (RecordEntryNode trait in traitNode.Children)
			{
				report.AppendFormat("      {0} = {1}\n", trait.Values[0], trait.Values[1]);
			}
		}

		static private string readNameKey(ParentNode detailsNode)
		{
			var nameNode = findChild(detailsNode, "CHARACTER_NAME");
			var namesBlock = nameNode.Children[0];
			var block0 = namesBlock.Children[0];
			var localization0 = block0.Children[0];
			string nameKey = ((StringNode)localization0.Value[0]).Value;
			return nameKey;
		}

		static private int computeAge(uint birth_year, uint birth_month, uint game_year, uint game_month)
		{
			int age = (int)game_year - (int)birth_year; // int just in case goes negative
			if (game_month < birth_month)
				age--;
			return age;
		}

		private void ArmyReport(ParentNode factionNode, StringBuilder report, ReportConfig cfg)
		{
			report.Append("  Armies (");
			if (cfg.OmitGarrisons)
				report.Append("garrisons omitted, ");
			report.AppendLine("player-named in quotes)");
			var armies = findChild(factionNode, "ARMY_ARRAY");
			int armyIndex = 0;
			foreach (var armyNode in armies.Children)
			{
				var militaryForce = findChild(armyNode, "MILITARY_FORCE");
				var militaryForceLegacy = findChild(militaryForce, "MILITARY_FORCE_LEGACY");
				reportArmy(militaryForceLegacy, armyIndex, report, cfg.OmitGarrisons);
				armyIndex++;
			}

			report.AppendLine("  Legacy armies");
			var legacyPool = findChild(factionNode, "MILITARY_FORCE_LEGACY_POOL");
			var legacies = findChild(legacyPool, "LEGACIES");
			armyIndex = 0;
			foreach (var legacyNode in legacies.Children)
			{
				var militaryForceLegacy = legacyNode.Children[0];
				reportArmy(militaryForceLegacy, armyIndex, report, cfg.OmitGarrisons);
				armyIndex++;
			}
		}

		private void reportArmy(ParentNode militaryForceLegacy, int armyIndex, StringBuilder report, bool omitGarrisons)
		{
			uint armyId = ((OptimizedUIntNode)militaryForceLegacy.Values[0]).Value;
			string armyName;
			bool isGarrison = GetArmyName(militaryForceLegacy, out armyName);
			if (isGarrison && omitGarrisons)
				return;

			var campaignSkills = findChild(militaryForceLegacy, "CAMPAIGN_SKILLS");
			uint rank = ((OptimizedUIntNode)campaignSkills.Values[5]).Value + 1;	// 0-based
			uint experience = ((OptimizedUIntNode)campaignSkills.Values[6]).Value;

			report.AppendFormat("  [{0}] {1} id:{2} rank:{3} experience:{4}\n", armyIndex, armyName, armyId, rank, experience);

			// skills
			//report.AppendLine("    skills:");
			var skillsBlock = findChild(campaignSkills, "CAMPAIGN_SKILLS_BLOCK");
			foreach (ParentNode skill in skillsBlock.Children)
			{
				string name = ((StringNode)skill.Values[0]).Value;
				uint level = ((OptimizedUIntNode)skill.Values[3]).Value;
				report.AppendFormat("      {0}  level:{1}\n", name, level);
			}
		}

		static private bool GetArmyName(ParentNode militaryForceLegacy, out string name)
		{
			const string GarrisonName = "random_localisation_strings_string_military_force_legacy_name_garrison_army";
			const string LegioHeader = "region_groups_localised_name_roman_legacy_generic_";

			var localization = findChild(militaryForceLegacy, "CAMPAIGN_LOCALISATION");
			name = ((StringNode)localization.Values[0]).Value;
			bool garry = false;
			if (name == GarrisonName)
			{
				name = "Garrison Army";
				garry = true;
			}
			else if (string.IsNullOrWhiteSpace(name))
				name = "\"" + ((StringNode)localization.Values[1]).Value + "\"";
			else if (name.StartsWith(LegioHeader))
			{
				// TEMPORARY: convert a stock name to "Legio nn <name>"
				//var history = findChild(militaryForceLegacy, "MILITARY_FORCE_LEGACY_HISTORY");
				//uint legio = ((OptimizedUIntNode)history.Values[2]).Value;
				uint legio = ((OptimizedUIntNode)militaryForceLegacy.Values[4]).Value;
				string actual = name.Substring(LegioHeader.Length, 1).ToUpper() + name.Substring(LegioHeader.Length+1); ;
				name = string.Format("Legio {0} {1}", ToRoman(legio), actual);
			}

			return garry;
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

		static private ParentNode findChild(ParentNode node, string childName)
		{
			foreach (var child in node.Children)
			{
				if (child.Name == childName)
					return child;
			}
			return null;
		}

		private ParentNode[] findChildren(ParentNode node, string childName)
		{
			List<ParentNode> nodes = new List<ParentNode>();
			foreach (var child in node.Children)
			{
				if (child.Name == childName)
					nodes.Add(child);
			}
			return nodes.ToArray();
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

		private List<FamilyMember> ScanFamilyTree(ParentNode FamilyTreeNode)
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

		// hack! from
		//	!rom_campaign_28_08_2023
		//		text/db/!rom_names.loc
		// (either need the key names to change to have character name included,
		// or use rpfm code to parse the appropriate campaign db)
		static private Dictionary<string, string> hardcoded_names = new Dictionary<string, string>
		{
			{"names_name_2147363948",   "Vinithar"},
			{"names_name_2147364248",   "Gínar"},
			{"names_name_2147365048",   "Galdor"},
			{"names_name_2147365248",   "Eärwen"},
			{"names_name_2147367348",   "Khalidun"},
			{"names_name_2147368048",   "Valdale"},
			{"names_name_2147368248",   "Targor"},
			{"names_name_2147363449",   "Ingold"},
			{"names_name_2147363649",   "Lothíriel"},
			{"names_name_2147363849",   "Marhari"},
			{"names_name_2147364549",   "Baldor"},
			{"names_name_2147364649",   "Ethelwine"},
			{"names_name_2147365149",   "Galathil"},
			{"names_name_2147365449",   "Daeron"},
			{"names_name_2147365749",   "Istiel"},
			{"names_name_2147365849",   "Míriel"},
			{"names_name_2147366049",   "Bóth"},
			{"names_name_2147366349",   "Bruglug"},
			{"names_name_2147367549",   "Marvane"},
			{"names_name_2147363146",   "Baragund"},
			{"names_name_2147363746",   "Demlid"},
			{"names_name_2147364246",   "Gimli"},
			{"names_name_2147365546",   "Mengir"},
			{"names_name_2147366146",   "Khun"},
			{"names_name_2147366546",   "Borgakh"},
			{"names_name_2147367346",   "Amirkhan"},
			{"names_name_2147367746",   "Beric"},
			{"names_name_2147368546",   "Narodwen"},
			{"names_name_2147365147",   "Darthaion"},
			{"names_name_2147365747",   "Idril"},
			{"names_name_2147366247",   "Lugbúrz"},
			{"names_name_2147366347",   "Grukthak"},
			{"names_name_2147366447",   "Dulruk"},
			{"names_name_2147366747",   "Shadgana"},
			{"names_name_2147366847",   "Durgok"},
			{"names_name_2147367047",   "Thragul"},
			{"names_name_2147367147",   "Thrazgasha"},
			{"names_name_2147367447",   "Jibral"},
			{"names_name_2147363144",   "Aradan"},
			{"names_name_2147363244",   "Denethor"},
			{"names_name_2147363744",   "Demhar"},
			{"names_name_2147364044",   "Nordir"},
			{"names_name_2147364344",   "Óin"},
			{"names_name_2147364444",   "Grís"},
			{"names_name_2147364744",   "Hádofel"},
			{"names_name_2147364844",   "Cwendar"},
			{"names_name_2147365244",   "Celebrían"},
			{"names_name_2147365644",   "Hirmion"},
			{"names_name_2147366444",   "Vorgul"},
			{"names_name_2147366644",   "Snudra"},
			{"names_name_2147366844",   "Thragrok"},
			{"names_name_2147367044",   "Grishnak"},
			{"names_name_2147367244",   "Huwair"},
			{"names_name_2147367644",   "Yustrana"},
			{"names_name_2147363345",   "Galador"},
			{"names_name_2147363845",   "Marwin"},
			{"names_name_2147364345",   "Onar"},
			{"names_name_2147364545",   "Arodhelm"},
			{"names_name_2147365445",   "Beleg"},
			{"names_name_2147365945",   "Khusad"},
			{"names_name_2147366145",   "Barud"},
			{"names_name_2147366845",   "Vargash"},
			{"names_name_2147366945",   "Grishok"},
			{"names_name_2147367045",   "Skargol"},
			{"names_name_2147367745",   "Halric"},
			{"names_name_2147367845",   "Torbrand"},
			{"names_name_2147363542",   "Beldis"},
			{"names_name_2147364442",   "Glós"},
			{"names_name_2147365342",   "Raudhil"},
			{"names_name_2147365742",   "Handriel"},
			{"names_name_2147366042",   "Khoddorth"},
			{"names_name_2147366142",   "Khót"},
			{"names_name_2147366342",   "Durgash"},
			{"names_name_2147366642",   "Thrazna"},
			{"names_name_2147368042",   "Sigdrifa"},
			{"names_name_2147368142",   "Caladon"},
			{"names_name_2147364943",   "Merewyn"},
			{"names_name_2147365443",   "Andion"},
			{"names_name_2147365543",   "Maeglin"},
			{"names_name_2147366543",   "Snarvok"},
			{"names_name_2147367043",   "Varguk"},
			{"names_name_2147368143",   "Elmarion"},
			{"names_name_2147368343",   "Maladriel"},
			{"names_name_2147363440",   "Imrahil"},
			{"names_name_2147363940",   "Viduric"},
			{"names_name_2147364340",   "Nyrath"},
			{"names_name_2147364540",   "Aldwine"},
			{"names_name_2147366740",   "Ulfhara"},
			{"names_name_2147367140",   "Skargola"},
			{"names_name_2147367440",   "Jamir"},
			{"names_name_2147368040",   "Astridale"},
			{"names_name_2147363141",   "Angbor"},
			{"names_name_2147363341",   "Forlong"},
			{"names_name_2147363641",   "Lalaith"},
			{"names_name_2147364141",   "Bori"},
			{"names_name_2147364641",   "Ethelward"},
			{"names_name_2147364941",   "Meregith"},
			{"names_name_2147365041",   "Faeldir"},
			{"names_name_2147366241",   "Snargút"},
			{"names_name_2147366841",   "Grakhorn"},
			{"names_name_2147367341",   "Fahazar"},
			{"names_name_2147367541",   "Khaliran"},
			{"names_name_2147367841",   "Harmond"},
			{"names_name_2147367941",   "Rowan"},
			{"names_name_2147368041",   "Lagertha"},
			{"names_name_2147368141",   "Ragnhilda"},
			{"names_name_2147368341",   "Armarion"},
			{"names_name_2147368441",   "Maladra"},
			{"names_name_2147363758",   "Halric"},
			{"names_name_2147365058",   "Gildor"},
			{"names_name_2147366658",   "Skulra"},
			{"names_name_2147367058",   "Lugdusha"},
			{"names_name_2147363459",   "Iorlas"},
			{"names_name_2147367159",   "Urglaka"},
			{"names_name_2147367659",   "Razindra"},
			{"names_name_2147367859",   "Gorstan"},
			{"names_name_2147363256",   "Dervorin"},
			{"names_name_2147363556",   "Beril"},
			{"names_name_2147363656",   "Morwen"},
			{"names_name_2147364656",   "Felaróf"},
			{"names_name_2147366156",   "Bolg"},
			{"names_name_2147367056",   "Urglok"},
			{"names_name_2147368056",   "Haelga"},
			{"names_name_2147363857",   "Megewis"},
			{"names_name_2147364957",   "Théodwyn"},
			{"names_name_2147366057",   "Lur"},
			{"names_name_2147366457",   "Zogthak"},
			{"names_name_2147366757",   "Uglasha"},
			{"names_name_2147367757",   "Torlond"},
			{"names_name_2147368257",   "Carindor"},
			{"names_name_2147368457",   "Eldramira"},
			{"names_name_2147368557",   "Armandra"},
			{"names_name_2147363154",   "Baranor"},
			{"names_name_2147364154",   "Buri"},
			{"names_name_2147364254",   "Glóin"},
			{"names_name_2147364454",   "Hona"},
			{"names_name_2147364954",   "Théodhild"},
			{"names_name_2147366354",   "Drubosh"},
			{"names_name_2147363355",   "Golasgil"},
			{"names_name_2147363755",   "Furic"},
			{"names_name_2147365455",   "Díor"},
			{"names_name_2147365755",   "Laichiel"},
			{"names_name_2147365855",   "Nerdanel"},
			{"names_name_2147366255",   "Mugash"},
			{"names_name_2147366555",   "Grizna"},
			{"names_name_2147366655",   "Grozna"},
			{"names_name_2147366855",   "Gorgakh"},
			{"names_name_2147367055",   "Krulmok"},
			{"names_name_2147367455",   "Bilash"},
			{"names_name_2147367855",   "Eldric"},
			{"names_name_2147367955",   "Ormand"},
			{"names_name_2147368555",   "Calandria"},
			{"names_name_2147363252",   "Derufin"},
			{"names_name_2147363952",   "Weslid"},
			{"names_name_2147364052",   "Sigrid"},
			{"names_name_2147364152",   "Bruni"},
			{"names_name_2147364352",   "Ori"},
			{"names_name_2147364952",   "Rosefled"},
			{"names_name_2147365552",   "Aegnor"},
			{"names_name_2147365652",   "Maedhros"},
			{"names_name_2147365752",   "Ivréniel"},
			{"names_name_2147365952",   "Bósun"},
			{"names_name_2147366352",   "Gulrak"},
			{"names_name_2147366652",   "Zogmasha"},
			{"names_name_2147366752",   "Durmara"},
			{"names_name_2147367252",   "Ûgaid"},
			{"names_name_2147367352",   "Hamzad"},
			{"names_name_2147367852",   "Thedric"},
			{"names_name_2147368352",   "Durian"},
			{"names_name_2147363653",   "Míliel"},
			{"names_name_2147364353",   "Thorin"},
			{"names_name_2147364553",   "Brego"},
			{"names_name_2147364753",   "Haldar"},
			{"names_name_2147364853",   "Cwenfled"},
			{"names_name_2147365053",   "Galion"},
			{"names_name_2147365253",   "Estelwen"},
			{"names_name_2147367553",   "Saydan"},
			{"names_name_2147367653",   "Nadrina"},
			{"names_name_2147363150",   "Barahir"},
			{"names_name_2147363650",   "Meleth"},
			{"names_name_2147363750",   "Forthwin"},
			{"names_name_2147364150",   "Borin"},
			{"names_name_2147366250",   "Búrzthrok"},
			{"names_name_2147366350",   "Nargol"},
			{"names_name_2147368550",   "Maeloria"},
			{"names_name_2147363551",   "Beleth"},
			{"names_name_2147365151",   "Haldir"},
			{"names_name_2147365351",   "Sídhril"},
			{"names_name_2147365651",   "Laicun"},
			{"names_name_2147366851",   "Snarlug"},
			{"names_name_2147366951",   "Muzgok"},
			{"names_name_2147367151",   "Krulmoka"},
			{"names_name_2147367551",   "Mustal"},
			{"names_name_2147367751",   "Aradan"},
			{"names_name_2147367951",   "Aldrick"},
			{"names_name_2147368151",   "Valandor"},
			{"names_name_2147368451",   "Arvandriel"},
			{"names_name_2147363468",   "Mablung"},
			{"names_name_2147365068",   "Gwainion"},
			{"names_name_2147365568",   "Angrod"},
			{"names_name_2147365968",   "Loth"},
			{"names_name_2147367068",   "Drubasha"},
			{"names_name_2147367668",   "Samiritha"},
			{"names_name_2147367868",   "Borin"},
			{"names_name_2147368168",   "Arvandir"},
			{"names_name_2147363169",   "Belegorn"},
			{"names_name_2147363469",   "Magor"},
			{"names_name_2147363569",   "Cólwen"},
			{"names_name_2147363969",   "Angrid"},
			{"names_name_2147365369",   "Elloth"},
			{"names_name_2147365769",   "Melgwen"},
			{"names_name_2147366769",   "Narbula"},
			{"names_name_2147366969",   "Ulfthor"},
			{"names_name_2147367269",   "Farhan"},
			{"names_name_2147368269",   "Tarandor"},
			{"names_name_2147363166",   "Bereg"},
			{"names_name_2147363266",   "Eärnur"},
			{"names_name_2147365166",   "Legolas"},
			{"names_name_2147365666",   "Maracir"},
			{"names_name_2147366066",   "Khóg"},
			{"names_name_2147363767",   "Hamgard"},
			{"names_name_2147364067",   "Thiudir"},
			{"names_name_2147364467",   "Íra"},
			{"names_name_2147365567",   "Amrod"},
			{"names_name_2147365767",   "Megilwen"},
			{"names_name_2147365867",   "Lathon"},
			{"names_name_2147366067",   "Khorar"},
			{"names_name_2147366267",   "Snarlak"},
			{"names_name_2147366467",   "Grobakh"},
			{"names_name_2147367467",   "Ahman"},
			{"names_name_2147368567",   "Durionwen"},
			{"names_name_2147363864",   "Megrid"},
			{"names_name_2147364464",   "Hara"},
			{"names_name_2147365764",   "Maeriel"},
			{"names_name_2147366364",   "Uglakh"},
			{"names_name_2147367764",   "Haldric"},
			{"names_name_2147367964",   "Torlond"},
			{"names_name_2147368364",   "Valorian"},
			{"names_name_2147368464",   "Caladwen"},
			{"names_name_2147363265",   "Duinhir"},
			{"names_name_2147363565",   "Bregil"},
			{"names_name_2147365165",   "Idhrennon"},
			{"names_name_2147365465",   "Edrahil"},
			{"names_name_2147366265",   "Dargoth"},
			{"names_name_2147367165",   "Lugthaka"},
			{"names_name_2147367365",   "Ramal"},
			{"names_name_2147367465",   "Imranak"},
			{"names_name_2147368065",   "Eldruna"},
			{"names_name_2147363162",   "Belecthor"},
			{"names_name_2147363362",   "Hador"},
			{"names_name_2147364362",   "Thráin"},
			{"names_name_2147364562",   "Brytta"},
			{"names_name_2147364762",   "Haleth"},
			{"names_name_2147365062",   "Guilin"},
			{"names_name_2147366162",   "Uglúk"},
			{"names_name_2147364163",   "Dáin"},
			{"names_name_2147364263",   "Gróin"},
			{"names_name_2147364563",   "Ceorl"},
			{"names_name_2147364663",   "Fengel"},
			{"names_name_2147364863",   "Cwenmund"},
			{"names_name_2147365263",   "Faeliel"},
			{"names_name_2147365963",   "Lug"},
			{"names_name_2147366563",   "Lugmasha"},
			{"names_name_2147366763",   "Grishara"},
			{"names_name_2147367063",   "Skarnoka"},
			{"names_name_2147367563",   "Hishanar"},
			{"names_name_2147367663",   "Javarah"},
			{"names_name_2147367963",   "Harlan"},
			{"names_name_2147368263",   "Eldrial"},
			{"names_name_2147363260",   "Duilin"},
			{"names_name_2147363960",   "Wesmund"},
			{"names_name_2147365560",   "Amras"},
			{"names_name_2147365860",   "Nimloth"},
			{"names_name_2147366060",   "Khath"},
			{"names_name_2147366260",   "Shagdûsh"},
			{"names_name_2147367160",   "Grubasha"},
			{"names_name_2147367360",   "Farosir"},
			{"names_name_2147368160",   "Maelor"},
			{"names_name_2147368360",   "Alcarion"},
			{"names_name_2147363661",   "Nienor"},
			{"names_name_2147364061",   "Stegrid"},
			{"names_name_2147364861",   "Cwenhild"},
			{"names_name_2147365161",   "Handion"},
			{"names_name_2147365361",   "Elbereth"},
			{"names_name_2147365661",   "Maglor"},
			{"names_name_2147365861",   "Kharûr"},
			{"names_name_2147365961",   "Khorûch"},
			{"names_name_2147366661",   "Gulra"},
			{"names_name_2147366861",   "Ulfgrim"},
			{"names_name_2147366961",   "Lurgash"},
			{"names_name_2147367261",   "Hisham"},
			{"names_name_2147367561",   "Nasilak"},
			{"names_name_2147364178",   "Durin"},
			{"names_name_2147364578",   "Déorwine"},
			{"names_name_2147365978",   "Bad"},
			{"names_name_2147366678",   "Urgana"},
			{"names_name_2147367178",   "Ulfthara"},
			{"names_name_2147367778",   "Corbin"},
			{"names_name_2147367878",   "Caradoc"},
			{"names_name_2147363279",   "Egalmoth"},
			{"names_name_2147363779",   "Harwic"},
			{"names_name_2147364879",   "Darfled"},
			{"names_name_2147365079",   "Lemion"},
			{"names_name_2147366179",   "Grishnákh"},
			{"names_name_2147366879",   "Durthak"},
			{"names_name_2147367079",   "Ulfgrimma"},
			{"names_name_2147367379",   "Azahir"},
			{"names_name_2147367979",   "Eorlind"},
			{"names_name_2147368279",   "Calanon"},
			{"names_name_2147368379",   "Narodan"},
			{"names_name_2147363476",   "Manthor"},
			{"names_name_2147363776",   "Harlid"},
			{"names_name_2147364276",   "Hánar"},
			{"names_name_2147364376",   "Víli"},
			{"names_name_2147366576",   "Shadra"},
			{"names_name_2147367076",   "Gorgula"},
			{"names_name_2147367276",   "Ammar"},
			{"names_name_2147363577",   "Emeldir"},
			{"names_name_2147363677",   "Rían"},
			{"names_name_2147365377",   "Galadriel"},
			{"names_name_2147365577",   "Argon"},
			{"names_name_2147365777",   "Néniel"},
			{"names_name_2147366077",   "Batad"},
			{"names_name_2147366277",   "Uruksh"},
			{"names_name_2147366877",   "Krulgash"},
			{"names_name_2147367477",   "Adnal"},
			{"names_name_2147368077",   "Ragnhilda"},
			{"names_name_2147364474",   "Lís"},
			{"names_name_2147365174",   "Malgalad"},
			{"names_name_2147365474",   "Enerdhil"},
			{"names_name_2147367674",   "Faisalyn"},
			{"names_name_2147368174",   "Eldracon"},
			{"names_name_2147363175",   "Belegund"},
			{"names_name_2147363875",   "Niukind"},
			{"names_name_2147363975",   "Blaidir"},
			{"names_name_2147364675",   "Folcwine"},
			{"names_name_2147365275",   "Handriel"},
			{"names_name_2147365675",   "Nortandil"},
			{"names_name_2147365775",   "Mereneth"},
			{"names_name_2147367375",   "Zaynir"},
			{"names_name_2147368475",   "Carindra"},
			{"names_name_2147363372",   "Haldad"},
			{"names_name_2147363872",   "Merlid"},
			{"names_name_2147364072",   "Thiumavi"},
			{"names_name_2147364172",   "Dori"},
			{"names_name_2147364272",   "Grór"},
			{"names_name_2147364372",   "Thrór"},
			{"names_name_2147364872",   "Cynered"},
			{"names_name_2147366572",   "Morga"},
			{"names_name_2147367172",   "Snaragora"},
			{"names_name_2147367272",   "Waleed"},
			{"names_name_2147367372",   "Hasin"},
			{"names_name_2147367572",   "Talibor"},
			{"names_name_2147367772",   "Rowan"},
			{"names_name_2147367972",   "Garred"},
			{"names_name_2147368072",   "Vigdisa"},
			{"names_name_2147363373",   "Hallas"},
			{"names_name_2147364773",   "Hásufax"},
			{"names_name_2147364973",   "Amroth"},
			{"names_name_2147365073",   "Inglor"},
			{"names_name_2147366473",   "Skulrak"},
			{"names_name_2147366773",   "Grukasha"},
			{"names_name_2147366973",   "Bruglok"},
			{"names_name_2147368073",   "Thora"},
			{"names_name_2147368473",   "Tarasuleth"},
			{"names_name_2147363670",   "Nimloth"},
			{"names_name_2147365270",   "Gilriel"},
			{"names_name_2147365870",   "Bogal"},
			{"names_name_2147366170",   "Gorbag"},
			{"names_name_2147366370",   "Krugdur"},
			{"names_name_2147366670",   "Morgula"},
			{"names_name_2147368270",   "Arathir"},
			{"names_name_2147368370",   "Elmarion"},
			{"names_name_2147363271",   "Ecthelion"},
			{"names_name_2147364571",   "Déor"},
			{"names_name_2147364671",   "Folca"},
			{"names_name_2147364771",   "Háma"},
			{"names_name_2147364871",   "Cynefled"},
			{"names_name_2147365071",   "Gwindor"},
			{"names_name_2147366871",   "Brakthor"},
			{"names_name_2147367171",   "Muzgula"},
			{"names_name_2147363308",   "Enthor"},
			{"names_name_2147364008",   "Heirod"},
			{"names_name_2147364608",   "Éadig"},
			{"names_name_2147365708",   "Araenel"},
			{"names_name_2147366308",   "Zargûl"},
			{"names_name_2147366608",   "Durmasha"},
			{"names_name_2147367508",   "Zafaran"},
			{"names_name_2147368408",   "Arvandil"},
			{"names_name_2147363509",   "Tuor"},
			{"names_name_2147364609",   "Éadmód"},
			{"names_name_2147364709",   "Garmund"},
			{"names_name_2147365009",   "Díor"},
			{"names_name_2147365809",   "Edhellos"},
			{"names_name_2147367309",   "Malik"},
			{"names_name_2147367409",   "Jawahad"},
			{"names_name_2147368109",   "Brunnhilda"},
			{"names_name_2147368209",   "Alcaron"},
			{"names_name_2147368309",   "Elrion"},
			{"names_name_2147364006",   "Goldar"},
			{"names_name_2147364106",   "Austri"},
			{"names_name_2147364906",   "Estrun"},
			{"names_name_2147365306",   "Megilwen"},
			{"names_name_2147365906",   "Khórûd"},
			{"names_name_2147366106",   "Bóddor"},
			{"names_name_2147366506",   "Gorgakh"},
			{"names_name_2147367106",   "Durthoka"},
			{"names_name_2147367606",   "Khalisara"},
			{"names_name_2147368306",   "Malachor"},
			{"names_name_2147363407",   "Hundad"},
			{"names_name_2147364807",   "Leofwine"},
			{"names_name_2147364907",   "Estwyn"},
			{"names_name_2147365107",   "Amdír"},
			{"names_name_2147365507",   "Gildor"},
			{"names_name_2147365907",   "Khalon"},
			{"names_name_2147366807",   "Grozgana"},
			{"names_name_2147368107",   "Svanhilda"},
			{"names_name_2147368207",   "Castamir"},
			{"names_name_2147363104",   "Aglahad"},
			{"names_name_2147363204",   "Borlas"},
			{"names_name_2147363304",   "Eluréd"},
			{"names_name_2147363804",   "Hermgard"},
			{"names_name_2147364404",   "Dís"},
			{"names_name_2147364704",   "Gamling"},
			{"names_name_2147364804",   "Leofric"},
			{"names_name_2147365204",   "Rómdir"},
			{"names_name_2147365404",   "Mithrellas"},
			{"names_name_2147365704",   "Alphiel"},
			{"names_name_2147366204",   "Ufthak"},
			{"names_name_2147366604",   "Snarlina"},
			{"names_name_2147366704",   "Grozula"},
			{"names_name_2147367204",   "Urgik"},
			{"names_name_2147367304",   "Suladan"},
			{"names_name_2147367604",   "Amirelle"},
			{"names_name_2147367804",   "Rodric"},
			{"names_name_2147367904",   "Roderin"},
			{"names_name_2147368004",   "Brynhilda"},
			{"names_name_2147363705",   "Bain"},
			{"names_name_2147367205",   "Mûndad"},
			{"names_name_2147367305",   "Fahad"},
			{"names_name_2147367505",   "Rafiqar"},
			{"names_name_2147368105",   "Thordale"},
			{"names_name_2147363602",   "Gilwen"},
			{"names_name_2147364202",   "Farin"},
			{"names_name_2147364502",   "Náva"},
			{"names_name_2147365302",   "Maeriel"},
			{"names_name_2147365602",   "Círdan"},
			{"names_name_2147365802",   "Aredhel"},
			{"names_name_2147366202",   "Durbûrz"},
			{"names_name_2147366602",   "Gorgasha"},
			{"names_name_2147366902",   "Thrakhul"},
			{"names_name_2147368502",   "Alcarinë"},
			{"names_name_2147363403",   "Hirluin"},
			{"names_name_2147364303",   "Lóni"},
			{"names_name_2147366003",   "Balud"},
			{"names_name_2147367103",   "Varguka"},
			{"names_name_2147367203",   "Adad"},
			{"names_name_2147367703",   "Eorlric"},
			{"names_name_2147364100",   "An"},
			{"names_name_2147364600",   "Dúnhere"},
			{"names_name_2147364900",   "Éowyn"},
			{"names_name_2147366300",   "Gromgash"},
			{"names_name_2147366500",   "Shadrok"},
			{"names_name_2147367400",   "Razael"},
			{"names_name_2147368200",   "Malachor"},
			{"names_name_2147368400",   "Valendil"},
			{"names_name_2147363501",   "Thorondir"},
			{"names_name_2147363901",   "Thiuman"},
			{"names_name_2147365801",   "Aniel"},
			{"names_name_2147366101",   "Bróthûl"},
			{"names_name_2147366201",   "Narzug"},
			{"names_name_2147366401",   "Grakthok"},
			{"names_name_2147367001",   "Snargash"},
			{"names_name_2147363118",   "Amlach"},
			{"names_name_2147363518",   "Turgon"},
			{"names_name_2147364518",   "Vális"},
			{"names_name_2147365518",   "Gwindor"},
			{"names_name_2147365818",   "Elwing"},
			{"names_name_2147366018",   "Bolur"},
			{"names_name_2147366318",   "Thraznakh"},
			{"names_name_2147368318",   "Tarmion"},
			{"names_name_2147363419",   "Hunthor"},
			{"names_name_2147363619",   "Hiril"},
			{"names_name_2147364319",   "Níthi"},
			{"names_name_2147364619",   "Éoláf"},
			{"names_name_2147364819",   "Thengel"},
			{"names_name_2147365019",   "Edrahil"},
			{"names_name_2147365319",   "Mereneth"},
			{"names_name_2147366119",   "Khorud"},
			{"names_name_2147366419",   "Mogdush"},
			{"names_name_2147366819",   "Gulmasha"},
			{"names_name_2147367419",   "Faisalim"},
			{"names_name_2147367819",   "Thulion"},
			{"names_name_2147368119",   "Audale"},
			{"names_name_2147364316",   "Nár"},
			{"names_name_2147364516",   "Thrána"},
			{"names_name_2147365216",   "Saeros"},
			{"names_name_2147365416",   "Nellas"},
			{"names_name_2147365716",   "Celebrían"},
			{"names_name_2147365916",   "Khóthûr"},
			{"names_name_2147366816",   "Skulina"},
			{"names_name_2147367016",   "Grakhorn"},
			{"names_name_2147368416",   "Maelion"},
			{"names_name_2147364317",   "Narvi"},
			{"names_name_2147366117",   "Khóm"},
			{"names_name_2147366217",   "Lagduf"},
			{"names_name_2147366517",   "Ulfthok"},
			{"names_name_2147366917",   "Urgash"},
			{"names_name_2147367617",   "Ramiyah"},
			{"names_name_2147367817",   "Garrick"},
			{"names_name_2147368517",   "Naramira"},
			{"names_name_2147363314",   "Eradan"},
			{"names_name_2147363714",   "Barhalm"},
			{"names_name_2147364514",   "Thrís"},
			{"names_name_2147365114",   "Andréthion"},
			{"names_name_2147365514",   "Gwainion"},
			{"names_name_2147366514",   "Nagluk"},
			{"names_name_2147366614",   "Uglasha"},
			{"names_name_2147366714",   "Durgasha"},
			{"names_name_2147367414",   "Samirad"},
			{"names_name_2147367614",   "Faridra"},
			{"names_name_2147368214",   "Valorian"},
			{"names_name_2147363915",   "Thiuwin"},
			{"names_name_2147364215",   "Frár"},
			{"names_name_2147364615",   "Éofor"},
			{"names_name_2147364815",   "Swithláf"},
			{"names_name_2147366315",   "Borgúl"},
			{"names_name_2147367215",   "Ûmuk"},
			{"names_name_2147367515",   "Shakin"},
			{"names_name_2147367715",   "Baldor"},
			{"names_name_2147363112",   "Alphros"},
			{"names_name_2147363412",   "Hundar"},
			{"names_name_2147364112",   "Bifur"},
			{"names_name_2147364312",   "Náli"},
			{"names_name_2147364712",   "Gárulf"},
			{"names_name_2147364912",   "Ethelfled"},
			{"names_name_2147365512",   "Guilin"},
			{"names_name_2147366012",   "Badun"},
			{"names_name_2147366112",   "Lodor"},
			{"names_name_2147366212",   "Radbug"},
			{"names_name_2147367212",   "Hulid"},
			{"names_name_2147368012",   "Runadale"},
			{"names_name_2147368312",   "Naronir"},
			{"names_name_2147363213",   "Boromir"},
			{"names_name_2147364013",   "Hartrid"},
			{"names_name_2147364213",   "Flói"},
			{"names_name_2147364413",   "Dwala"},
			{"names_name_2147364613",   "Elfhelm"},
			{"names_name_2147364813",   "Sigeweard"},
			{"names_name_2147365213",   "Rúmil"},
			{"names_name_2147365313",   "Melgwen"},
			{"names_name_2147366913",   "Zogmok"},
			{"names_name_2147367013",   "Muzgul"},
			{"names_name_2147367113",   "Zogthara"},
			{"names_name_2147367313",   "Rashal"},
			{"names_name_2147363610",   "Glóredhel"},
			{"names_name_2147363910",   "Thiumar"},
			{"names_name_2147364110",   "Balin"},
			{"names_name_2147364210",   "Fíli"},
			{"names_name_2147364310",   "Náin"},
			{"names_name_2147365810",   "Eldis"},
			{"names_name_2147366910",   "Drubash"},
			{"names_name_2147367010",   "Lugthak"},
			{"names_name_2147367610",   "Hamzira"},
			{"names_name_2147367710",   "Thedric"},
			{"names_name_2147363611",   "Hareth"},
			{"names_name_2147363811",   "Horman"},
			{"names_name_2147364511",   "Oda"},
			{"names_name_2147365411",   "Narwen"},
			{"names_name_2147365611",   "Curufin"},
			{"names_name_2147366311",   "Snarzug"},
			{"names_name_2147366411",   "Luglak"},
			{"names_name_2147366711",   "Skulgana"},
			{"names_name_2147367211",   "Huwab"},
			{"names_name_2147367811",   "Erland"},
			{"names_name_2147367911",   "Ulmar"},
			{"names_name_2147368511",   "Valorië"},
			{"names_name_2147363628",   "Hunleth"},
			{"names_name_2147364028",   "Lifdir"},
			{"names_name_2147364828",   "Théoden"},
			{"names_name_2147364928",   "Hild"},
			{"names_name_2147365528",   "Lindir"},
			{"names_name_2147365728",   "Estelwen"},
			{"names_name_2147366428",   "Snargol"},
			{"names_name_2147367328",   "Tarikar"},
			{"names_name_2147367828",   "Haldan"},
			{"names_name_2147363329",   "Estelmo"},
			{"names_name_2147363529",   "Adanel"},
			{"names_name_2147363729",   "Brand"},
			{"names_name_2147364029",   "Lithwes"},
			{"names_name_2147365329",   "Néniel"},
			{"names_name_2147366829",   "Lurtz"},
			{"names_name_2147367029",   "Zogthor"},
			{"names_name_2147367329",   "Malaki"},
			{"names_name_2147367729",   "Dorian"},
			{"names_name_2147368129",   "Hildra"},
			{"names_name_2147363126",   "Amrothos"},
			{"names_name_2147363426",   "Huor"},
			{"names_name_2147363826",   "Garman"},
			{"names_name_2147364626",   "Éomund"},
			{"names_name_2147365526",   "Lemion"},
			{"names_name_2147366526",   "Morgul"},
			{"names_name_2147367626",   "Hasinya"},
			{"names_name_2147363427",   "Húrin"},
			{"names_name_2147363727",   "Borgard"},
			{"names_name_2147363827",   "Germund"},
			{"names_name_2147364427",   "Freris"},
			{"names_name_2147364527",   "Vída"},
			{"names_name_2147364727",   "Gram"},
			{"names_name_2147364927",   "Gledswith"},
			{"names_name_2147365127",   "Astoron"},
			{"names_name_2147365227",   "Alphiel"},
			{"names_name_2147366127",   "Botûr"},
			{"names_name_2147367227",   "Arib"},
			{"names_name_2147368327",   "Carandor"},
			{"names_name_2147363624",   "Hirwen"},
			{"names_name_2147365424",   "Nimrodel"},
			{"names_name_2147365824",   "Finduilas"},
			{"names_name_2147367524",   "Salahin"},
			{"names_name_2147363225",   "Brandir"},
			{"names_name_2147363925",   "Valder"},
			{"names_name_2147364325",   "Nori"},
			{"names_name_2147364425",   "Frána"},
			{"names_name_2147365425",   "Tauriel"},
			{"names_name_2147365525",   "Inglor"},
			{"names_name_2147365725",   "Eärwen"},
			{"names_name_2147366025",   "Khasal"},
			{"names_name_2147366725",   "Lugmara"},
			{"names_name_2147366825",   "Grishnákh"},
			{"names_name_2147366925",   "Snagruk"},
			{"names_name_2147367825",   "Elmar"},
			{"names_name_2147368225",   "Maladon"},
			{"names_name_2147363722",   "Barlid"},
			{"names_name_2147364722",   "Goldwine"},
			{"names_name_2147365222",   "Thranduil"},
			{"names_name_2147365622",   "Ereinion"},
			{"names_name_2147367922",   "Haldred"},
			{"names_name_2147368522",   "Caranorë"},
			{"names_name_2147363523",   "Túrin"},
			{"names_name_2147364223",   "Frerin"},
			{"names_name_2147364623",   "Éomer"},
			{"names_name_2147365123",   "Annael"},
			{"names_name_2147366023",   "Lógûd"},
			{"names_name_2147366223",   "Muzgash"},
			{"names_name_2147366323",   "Mugluk"},
			{"names_name_2147366623",   "Grishna"},
			{"names_name_2147366723",   "Gorgula"},
			{"names_name_2147366923",   "Grukthar"},
			{"names_name_2147367023",   "Drubak"},
			{"names_name_2147367223",   "Nuab"},
			{"names_name_2147367423",   "Anwarak"},
			{"names_name_2147367723",   "Eldan"},
			{"names_name_2147368223",   "Arvandor"},
			{"names_name_2147368423",   "Naramir"},
			{"names_name_2147363320",   "Erchirion"},
			{"names_name_2147363820",   "Husgard"},
			{"names_name_2147365620",   "Egalmoth"},
			{"names_name_2147367120",   "Snarlasha"},
			{"names_name_2147367320",   "Karizan"},
			{"names_name_2147368420",   "Targon"},
			{"names_name_2147363221",   "Borondir"},
			{"names_name_2147364021",   "Holmod"},
			{"names_name_2147364121",   "Bildri"},
			{"names_name_2147364421",   "Fína"},
			{"names_name_2147364921",   "Frithild"},
			{"names_name_2147365021",   "Enerdhil"},
			{"names_name_2147365221",   "Thingol"},
			{"names_name_2147365921",   "Khadul"},
			{"names_name_2147366821",   "Uglúk"},
			{"names_name_2147367921",   "Elric"},
			{"names_name_2147368021",   "Sigrun"},
			{"names_name_2147368221",   "Thandor"},
			{"names_name_2147368421",   "Carindor"},
			{"names_name_2147363738",   "Demgard"},
			{"names_name_2147365438",   "Amroth"},
			{"names_name_2147365538",   "Mablung"},
			{"names_name_2147366338",   "Narbul"},
			{"names_name_2147366438",   "Snarlug"},
			{"names_name_2147366638",   "Drubina"},
			{"names_name_2147366738",   "Snarlara"},
			{"names_name_2147367038",   "Durthok"},
			{"names_name_2147368238",   "Naromir"},
			{"names_name_2147363639",   "Ivriniel"},
			{"names_name_2147364039",   "Nilaug"},
			{"names_name_2147364239",   "Fundin"},
			{"names_name_2147364639",   "Erkenbrand"},
			{"names_name_2147364739",   "Guthláf"},
			{"names_name_2147364939",   "Merefled"},
			{"names_name_2147365139",   "Conúion"},
			{"names_name_2147365839",   "Lúthien"},
			{"names_name_2147365939",   "Khagad"},
			{"names_name_2147366239",   "Gorbakh"},
			{"names_name_2147367239",   "Ûib"},
			{"names_name_2147368439",   "Valariana"},
			{"names_name_2147368539",   "Targoria"},
			{"names_name_2147363336",   "Findegil"},
			{"names_name_2147363536",   "Andreth"},
			{"names_name_2147365336",   "Raewen"},
			{"names_name_2147365636",   "Glorfindel"},
			{"names_name_2147366336",   "Skulg"},
			{"names_name_2147366636",   "Grukasha"},
			{"names_name_2147366836",   "Gorbag"},
			{"names_name_2147367036",   "Snagor"},
			{"names_name_2147367436",   "Saifal"},
			{"names_name_2147367636",   "Azarael"},
			{"names_name_2147367736",   "Aldred"},
			{"names_name_2147368036",   "Gudruna"},
			{"names_name_2147363737",   "Bronhar"},
			{"names_name_2147363837",   "Gilding"},
			{"names_name_2147364037",   "Mildrid"},
			{"names_name_2147365037",   "Esgalion"},
			{"names_name_2147365837",   "Lalwen"},
			{"names_name_2147367637",   "Nabilis"},
			{"names_name_2147363134",   "Anborn"},
			{"names_name_2147363334",   "Faramir"},
			{"names_name_2147364134",   "Bombur"},
			{"names_name_2147364334",   "Northi"},
			{"names_name_2147364734",   "Grimbold"},
			{"names_name_2147364934",   "Ides"},
			{"names_name_2147365134",   "Celeborn"},
			{"names_name_2147365734",   "Faeliel"},
			{"names_name_2147367234",   "Harak"},
			{"names_name_2147367334",   "Idrik"},
			{"names_name_2147367534",   "Tameri"},
			{"names_name_2147367634",   "Zaylira"},
			{"names_name_2147363235",   "Damrod"},
			{"names_name_2147363635",   "Ioreth"},
			{"names_name_2147364035",   "Mathilda"},
			{"names_name_2147364335",   "Nyr"},
			{"names_name_2147364535",   "Aldo"},
			{"names_name_2147364835",   "Théoric"},
			{"names_name_2147365235",   "Araenel"},
			{"names_name_2147365635",   "Finrod"},
			{"names_name_2147365735",   "Gilriel"},
			{"names_name_2147366135",   "Khadûd"},
			{"names_name_2147366235",   "Golfimbul"},
			{"names_name_2147366835",   "Shagrat"},
			{"names_name_2147366935",   "Throzgash"},
			{"names_name_2147367535",   "Farhanir"},
			{"names_name_2147368135",   "Helgadale"},
			{"names_name_2147363932",   "Vidugavia"},
			{"names_name_2147364232",   "Frór"},
			{"names_name_2147364432",   "Gloris"},
			{"names_name_2147365632",   "Fingon"},
			{"names_name_2147366532",   "Drulmog"},
			{"names_name_2147366832",   "Mauhúr"},
			{"names_name_2147367032",   "Thrakgash"},
			{"names_name_2147367432",   "Karimun"},
			{"names_name_2147367932",   "Bryngar"},
			{"names_name_2147368332",   "Valendor"},
			{"names_name_2147368432",   "Narothel"},
			{"names_name_2147364233",   "Frosti"},
			{"names_name_2147365433",   "Rhosgwen"},
			{"names_name_2147366033",   "Bogurth"},
			{"names_name_2147366333",   "Ulfhak"},
			{"names_name_2147366433",   "Grishak"},
			{"names_name_2147366533",   "Gurghak"},
			{"names_name_2147366733",   "Morgasha"},
			{"names_name_2147367333",   "Zahmir"},
			{"names_name_2147367833",   "Eogan"},
			{"names_name_2147368233",   "Elendir"},
			{"names_name_2147363230",   "Cirion"},
			{"names_name_2147363430",   "Imlach"},
			{"names_name_2147364130",   "Bofur"},
			{"names_name_2147365030",   "Erestor"},
			{"names_name_2147366430",   "Urglok"},
			{"names_name_2147366630",   "Narbula"},
			{"names_name_2147367130",   "Grishnaka"},
			{"names_name_2147367230",   "Adag"},
			{"names_name_2147367430",   "Raheez"},
			{"names_name_2147367930",   "Thrandan"},
			{"names_name_2147368130",   "Gyrda"},
			{"names_name_2147368330",   "Eldrin"},
			{"names_name_2147363631",   "Idril"},
			{"names_name_2147364631",   "Éothain"},
			{"names_name_2147365131",   "Brúndir"},
			{"names_name_2147365831",   "Indis"},
			{"names_name_2147365931",   "Lathal"},
			{"names_name_2147366231",   "Guritz"},
			{"names_name_2147366731",   "Grizula"},
			{"names_name_2147367231",   "Awik"},
			{"names_name_2147367831",   "Alden"},
			{"names_name_2147368031",   "Eowar"},
			{"names_name_2147368431",   "Elendiriel"},
			{"names_name_2147368531",   "Elmariona"},
			{"names_name_2147363588",   "Fíriel"},
			{"names_name_2147364088",   "Varmdir"},
			{"names_name_2147364688",   "Fréaláf"},
			{"names_name_2147365088",   "Maeglin"},
			{"names_name_2147365488",   "Esgalion"},
			{"names_name_2147365888",   "Lóchud"},
			{"names_name_2147366688",   "Gorbulina"},
			{"names_name_2147367788",   "Thrandir"},
			{"names_name_2147367888",   "Brynmund"},
			{"names_name_2147364089",   "Vidumavi"},
			{"names_name_2147364589",   "Dernhelm"},
			{"names_name_2147365289",   "Ivréniel"},
			{"names_name_2147365389",   "Gwathrel"},
			{"names_name_2147365689",   "Pengolod"},
			{"names_name_2147366889",   "Snarhak"},
			{"names_name_2147367789",   "Brynhelm"},
			{"names_name_2147367989",   "Haldor"},
			{"names_name_2147368289",   "Maelendil"},
			{"names_name_2147368489",   "Eldriona"},
			{"names_name_2147363386",   "Hathol"},
			{"names_name_2147363486",   "Pelendur"},
			{"names_name_2147364386",   "Balis"},
			{"names_name_2147365086",   "Mablung"},
			{"names_name_2147366286",   "Groznak"},
			{"names_name_2147366886",   "Muzgur"},
			{"names_name_2147367586",   "Tarysia"},
			{"names_name_2147368486",   "Valendilwen"},
			{"names_name_2147364087",   "Varmhar"},
			{"names_name_2147365787",   "Raudhil"},
			{"names_name_2147365987",   "Khal"},
			{"names_name_2147366587",   "Skulgasha"},
			{"names_name_2147367087",   "Bruglaka"},
			{"names_name_2147367487",   "Nasil"},
			{"names_name_2147368087",   "Dalefrida"},
			{"names_name_2147368487",   "Arathira"},
			{"names_name_2147363184",   "Beregond"},
			{"names_name_2147363484",   "Orodreth"},
			{"names_name_2147365384",   "Glandiel"},
			{"names_name_2147366184",   "Golfimbul"},
			{"names_name_2147366384",   "Rukdug"},
			{"names_name_2147366984",   "Skarnok"},
			{"names_name_2147367184",   "Bruglaka"},
			{"names_name_2147363285",   "Elboron"},
			{"names_name_2147363685",   "Aric"},
			{"names_name_2147363885",   "Norric"},
			{"names_name_2147363985",   "Blainil"},
			{"names_name_2147364485",   "Lora"},
			{"names_name_2147364785",   "Hildmund"},
			{"names_name_2147364885",   "Deorwyn"},
			{"names_name_2147365085",   "Lindir"},
			{"names_name_2147365285",   "Istiel"},
			{"names_name_2147365585",   "Avalin"},
			{"names_name_2147366085",   "Khuth"},
			{"names_name_2147366685",   "Snagula"},
			{"names_name_2147368385",   "Caranon"},
			{"names_name_2147363382",   "Hardang"},
			{"names_name_2147363582",   "Elwing"},
			{"names_name_2147363682",   "Almar"},
			{"names_name_2147364182",   "Dwalin"},
			{"names_name_2147366482",   "Snudush"},
			{"names_name_2147366582",   "Ulfhura"},
			{"names_name_2147366782",   "Drubara"},
			{"names_name_2147367182",   "Skarnoka"},
			{"names_name_2147367282",   "Raza"},
			{"names_name_2147367482",   "Ammari"},
			{"names_name_2147363583",   "Finduilas"},
			{"names_name_2147363783",   "Holkin"},
			{"names_name_2147364383",   "Ada"},
			{"names_name_2147364583",   "Déothain"},
			{"names_name_2147364683",   "Fréa"},
			{"names_name_2147364783",   "Hereward"},
			{"names_name_2147364883",   "Dawyn"},
			{"names_name_2147364983",   "Andion"},
			{"names_name_2147365183",   "Merdir"},
			{"names_name_2147365283",   "Idril"},
			{"names_name_2147365783",   "Raewen"},
			{"names_name_2147366183",   "Lugdush"},
			{"names_name_2147367083",   "Snagora"},
			{"names_name_2147367583",   "Karindra"},
			{"names_name_2147367683",   "Anwarae"},
			{"names_name_2147368183",   "Naroth"},
			{"names_name_2147368483",   "Maelorwen"},
			{"names_name_2147363380",   "Halmir"},
			{"names_name_2147363680",   "Ríliel"},
			{"names_name_2147364080",   "Thiumor"},
			{"names_name_2147365480",   "Erestor"},
			{"names_name_2147365880",   "Bratûd"},
			{"names_name_2147366180",   "Shagrat"},
			{"names_name_2147366380",   "Gorthuk"},
			{"names_name_2147366680",   "Shagdra"},
			{"names_name_2147366980",   "Gorgul"},
			{"names_name_2147367380",   "Nabir"},
			{"names_name_2147367580",   "Rashara"},
			{"names_name_2147364281",   "Hór"},
			{"names_name_2147364481",   "Lóna"},
			{"names_name_2147365181",   "Meldiron"},
			{"names_name_2147365681",   "Orodreth"},
			{"names_name_2147368381",   "Taralion"},
			{"names_name_2147363298",   "Elurín"},
			{"names_name_2147364098",   "Wesmod"},
			{"names_name_2147364398",   "Dava"},
			{"names_name_2147364798",   "Léodred"},
			{"names_name_2147365298",   "Laichiel"},
			{"names_name_2147365698",   "Turgon"},
			{"names_name_2147366798",   "Zogmara"},
			{"names_name_2147367098",   "Grakhorna"},
			{"names_name_2147367898",   "Halvard"},
			{"names_name_2147367998",   "Freydisa"},
			{"names_name_2147368498",   "Malachora"},
			{"names_name_2147363199",   "Bergil"},
			{"names_name_2147363799",   "Herming"},
			{"names_name_2147364999",   "Daeron"},
			{"names_name_2147365899",   "Bor"},
			{"names_name_2147366099",   "Lubud"},
			{"names_name_2147367599",   "Fahrendra"},
			{"names_name_2147368199",   "Carandil"},
			{"names_name_2147368399",   "Malachor"},
			{"names_name_2147363096",   "Adrahil"},
			{"names_name_2147363496",   "Saelon"},
			{"names_name_2147363896",   "Sigmund"},
			{"names_name_2147365396",   "Lúchendriel"},
			{"names_name_2147365896",   "Lorûng"},
			{"names_name_2147366696",   "Uruksha"},
			{"names_name_2147367496",   "Yasim"},
			{"names_name_2147367796",   "Eldan"},
			{"names_name_2147367996",   "Bryndan"},
			{"names_name_2147368096",   "Alruna"},
			{"names_name_2147363097",   "Agathor"},
			{"names_name_2147363997",   "Geldrid"},
			{"names_name_2147364597",   "Dernláf"},
			{"names_name_2147364697",   "Frumgar"},
			{"names_name_2147365097",   "Alagron"},
			{"names_name_2147365197",   "Raudir"},
			{"names_name_2147365497",   "Galion"},
			{"names_name_2147365597",   "Celegorm"},
			{"names_name_2147366097",   "Khón"},
			{"names_name_2147367397",   "Nadiran"},
			{"names_name_2147367597",   "Idritha"},
			{"names_name_2147368297",   "Valendur"},
			{"names_name_2147363294",   "Elros"},
			{"names_name_2147363394",   "Hirgon"},
			{"names_name_2147363594",   "Gaerwen"},
			{"names_name_2147364794",   "Holdred"},
			{"names_name_2147365094",   "Mengir"},
			{"names_name_2147365194",   "Orophin"},
			{"names_name_2147365994",   "Lotod"},
			{"names_name_2147367194",   "Inwir"},
			{"names_name_2147363695",   "Bard"},
			{"names_name_2147365795",   "Sídhril"},
			{"names_name_2147366095",   "Khór"},
			{"names_name_2147366595",   "Durgana"},
			{"names_name_2147367295",   "Hassan"},
			{"names_name_2147363392",   "Herion"},
			{"names_name_2147363892",   "Segric"},
			{"names_name_2147364192",   "Dwari"},
			{"names_name_2147364892",   "Elfhild"},
			{"names_name_2147365192",   "Oropher"},
			{"names_name_2147366892",   "Skargor"},
			{"names_name_2147367492",   "Waliq"},
			{"names_name_2147367592",   "Zahiriya"},
			{"names_name_2147367892",   "Erwin"},
			{"names_name_2147367992",   "Thulric"},
			{"names_name_2147363293",   "Elphir"},
			{"names_name_2147363793",   "Holwic"},
			{"names_name_2147364093",   "Vidurid"},
			{"names_name_2147364293",   "Lofar"},
			{"names_name_2147364493",   "Nona"},
			{"names_name_2147364993",   "Beleg"},
			{"names_name_2147366093",   "Bal"},
			{"names_name_2147366193",   "Mauhúr"},
			{"names_name_2147366293",   "Drughûl"},
			{"names_name_2147366793",   "Snudara"},
			{"names_name_2147366993",   "Ulfgrim"},
			{"names_name_2147367093",   "Thrakgasha"},
			{"names_name_2147367693",   "Karisah"},
			{"names_name_2147368393",   "Eldrion"},
			{"names_name_2147363190",   "Berelach"},
			{"names_name_2147363990",   "Blaukin"},
			{"names_name_2147364390",   "Bora"},
			{"names_name_2147365490",   "Faeldir"},
			{"names_name_2147366090",   "Brór"},
			{"names_name_2147366490",   "Thruggash"},
			{"names_name_2147367290",   "Youssef"},
			{"names_name_2147367390",   "Yussan"},
			{"names_name_2147367590",   "Malakira"},
			{"names_name_2147368190",   "Tarasul"},
			{"names_name_2147364291",   "Kíli"},
			{"names_name_2147364691",   "Fréawine"},
			{"names_name_2147364891",   "Eafled"},
			{"names_name_2147365491",   "Galdor"},
			{"names_name_2147365591",   "Caranthir"},
			{"names_name_2147365691",   "Penlod"},
			{"names_name_2147366391",   "Thrakgash"},
			{"names_name_2147366791",   "Thrazgana"},
			{"names_name_2147366891",   "Lugdush"},
			{"names_name_2147367491",   "Abdan"},
			{"names_name_2147367691",   "Rahimira"},
			{"names_name_2147366149",   "Théodred"},
			{"names_name_2147366372",   "Witch-king of Angmar"},
			{"names_name_2147366373",   "Khamûl the Easterling"},
			{"names_name_2147366377",   "Nazgûl"},
			{"names_name_2147366381",   "Mouth of Sauron"},
			{"names_name_2147366387",   "Gothmog"},
			{"names_name_2147366424",   "Azgarzîr"},
			{"names_name_2147366451",   "Dôlguzagar"},
			{"names_name_2147366472",   "Gimlân"},
			{"names_name_2147366487",   "Pharazôn"},
			{"names_name_2147366503",   "Tarîkmagân"},
			{"names_name_2147366519",   "Zadnazîr"},
			{"names_name_2147366525",   "Zâinabên"},
			{"names_name_2147366529",   "Zimrathôn"},
			{"names_name_2147366544",   "Zôrzagar"},
			{"names_name_2147366554",   "Shagrat"},
			{"names_name_2147366560",   "Grishnákh"},
			{"names_name_2147366565",   "Gorbag"},
			{"names_name_2147366610",   "Atalnancar"},
			{"names_name_2147366619",   "Saruman"},
			{"names_name_2147366628",   "Lúrtz"},
			{"names_name_2147366632",   "Uglúk"},
			{"names_name_2147366640",   "Mauhúr"},
			{"names_name_2147366649",   "Lugdush"},
			{"names_name_2147366365",   "Sauron"},
			{"names_name_2147366653",   "I"},
			{"names_name_2147366663",   "II"},
			{"names_name_2147366673",   "III"},
			{"names_name_2147366679",   "Ironfoot"},
			{"names_name_2147366686",   "Stonehelm"},
		};
	}

	public class EsfTreeNode : TreeNode {
        private bool showCode;
        public bool ShowCode {
            get { return showCode; }
            set {
                ParentNode node = Tag as ParentNode;
                if (node != null) {
                    string baseName = (node as INamedNode).GetName();
                    Text = value ? string.Format("{0} - {1}", baseName, node.TypeCode) : baseName;
                    showCode = value;
                    foreach (TreeNode child in Nodes) {
                        (child as EsfTreeNode).ShowCode = value;
                    }
                }
            }
        }
        public EsfTreeNode(ParentNode node, bool showC = false) {
            Tag = node;
            Text = (node as INamedNode).GetName();
            node.ModifiedEvent += NodeChange;
            ForeColor = node.Modified ? Color.Red : Color.Black;
            ShowCode = showC;
            
            node.RenameEvent += delegate(EsfNode n) {
                Text = node.Name;
            };
        }
        public void Fill() {
            if (Nodes.Count == 0) {
#if DEBUG
                Console.WriteLine("filling list for {0}", (Tag as ParentNode).Name);
#endif
                ParentNode parentNode = (Tag as ParentNode);
                foreach (ParentNode child in parentNode.Children) {
                    EsfTreeNode childNode = new EsfTreeNode(child, ShowCode);
                    Nodes.Add(childNode);
                }
            }
        }
        public void NodeChange(EsfNode n) {
            ForeColor = n.Modified ? Color.Red : Color.Black;
            if (!n.Modified) {
                return;
            }
            ParentNode node = (Tag as ParentNode);
            bool sameChildren = node.Children.Count == this.Nodes.Count;
            for (int i = 0; sameChildren && i < node.Children.Count; i++) {
                sameChildren &= node.Children[i].Name.Equals(Nodes[i].Text);
            }
            if (node != null) {
                if (!sameChildren) {
                    Nodes.Clear();
                    Fill();
                    if (IsExpanded) {
                        foreach (TreeNode child in Nodes) {
                            (child as EsfTreeNode).Fill();
                        }
                    }
                } else {
                    for(int i = 0; i < node.Children.Count; i++) {
                        Nodes[i].Text = node.Children[i].Name;
                    }
                }
            }
        }
    }

    public class TreeEventHandler {
        private List<ModificationColorizer> registeredEvents = new List<ModificationColorizer>();
        private EditEsfComponent component;
        
        DataGridView nodeValueGridView;
        public TreeEventHandler(DataGridView view, EditEsfComponent c) {
            nodeValueGridView = view;
            component = c;
        }
        /*
         * Fill the event's target tree node's children with their children
         * (to show the [+] if they contain child nodes).
         */
        public void FillNode(object sender, TreeViewCancelEventArgs args) {
            foreach (TreeNode child in args.Node.Nodes) {
                EsfTreeNode esfNode = child as EsfTreeNode;
                if (esfNode != null) {
                    esfNode.Fill();
                }
            }
        }

        /*
         * Render the data cell view, preparing the red color for modified entries.
         */
        public void TreeNodeSelected(object sender, TreeViewEventArgs args) {
            ParentNode node = args.Node.Tag as ParentNode;
            try {
                nodeValueGridView.Rows.Clear();
                registeredEvents.ForEach(handler => { (handler.row.Tag as EsfNode).ModifiedEvent -= handler.ChangeColor; });
                registeredEvents.Clear();
                foreach (EsfNode value in node.Values) {
                    int index = nodeValueGridView.Rows.Add(value.ToString(), value.SystemType.ToString(), value.TypeCode.ToString());
                    DataGridViewRow newRow = nodeValueGridView.Rows [index];
                    ModificationColorizer colorizer = new ModificationColorizer(newRow);
                    registeredEvents.Add(colorizer);
                    foreach (DataGridViewCell cell in newRow.Cells) {
                        cell.Style.ForeColor = value.Modified ? Color.Red : Color.Black;
                    }
                    value.ModifiedEvent += colorizer.ChangeColor;
                    
                    newRow.Tag = value;
                }
                component.NotifySelection(node);
            } catch {
            }
        }
    }    
    
    public class ModificationColorizer {
        public DataGridViewRow row;
        public ModificationColorizer(DataGridViewRow r) {
            row = r;
        }
        public void ChangeColor(EsfNode node) {
            foreach (DataGridViewCell cell in row.Cells) {
                cell.Style.ForeColor = node.Modified ? Color.Red : Color.Black;
            }
        }
    }

    public class BookmarkItem : ToolStripMenuItem {
        string openPath;
        EditEsfComponent component;
        public BookmarkItem(string label, string path, EditEsfComponent c) : base(label) {
            openPath = path;
            component = c;
            Click += OpenPath;
        }
        private void OpenPath(object sender, EventArgs args) {
            component.SelectedPath = openPath;
        }
    }
}
