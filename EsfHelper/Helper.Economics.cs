using EsfLibrary;
using System.Text;

namespace EsfHelper
{
	static public partial class Helper
	{
		static private void EconomicReport(ParentNode factionNode, StringBuilder report)
		{
			var economics = FindChild(factionNode, "FACTION_ECONOMICS");
			int treasury = ((OptimizedIntNode)economics.Values[0]).Value;
			report.AppendLine("  Economics:");
			report.AppendFormat("    Treasury: {0:#,#}\n", treasury);

			var history = economics.Children[0];
			int index = history.Children.Count - 1;
			if (index >= 0)
			{
				var econDataNode = history.Children[index].Children[0];
				var econData = readEconData(econDataNode);
				int taxes = econData.arrays[1][0];
				if (taxes == 0)
				{
					report.AppendLine("    Faction has been destroyed");
					return;
				}

				report.AppendLine("    Projected this turn:");
				reportEconData(econData, report);

				if (index >= 1)
				{
					var priorDataNode = history.Children[index - 1].Children[0];
					var priorData = readEconData(priorDataNode);
					report.AppendLine("    Last turn:");
					reportEconData(priorData, report);
				}
			}
		}

		struct EconData
		{
			public Dictionary<int, Dictionary<int, int>> arrays;
		}

		static private EconData readEconData(ParentNode econDataNode)
		{
			// I thought econ data were always OptimizedArrayNodes but apparently sometimes they are EsfArrayNodes
			// So now need to handle both types
			EconData data = new EconData();
			data.arrays = new Dictionary<int, Dictionary<int, int>>();
			for (int n = 0; n < econDataNode.Values.Count; n++)
			{
				var node = econDataNode.Values[n];
				data.arrays[n] = new Dictionary<int, int>();
				if (node is OptimizedArrayNode<int>)
				{
					var oan = node as OptimizedArrayNode<int>;
					for (int i = 0; i < oan.Value.Length; i++)
						data.arrays[n][i] = (int)oan.Value.GetValue(i);
				}
				else if (node is EsfArrayNode<int>)
				{
					var ean = node as EsfArrayNode<int>;
					for (int i = 0; i < ean.Value.Length; i++)
						data.arrays[n][i] = ean.Value[i];
				}
				else
				{
					throw new NotSupportedException("readEconData can't read node " + node.ToString());
				}
			}
			return data;
		}

		static private void reportEconData(EconData econData, StringBuilder report)
		{
			var array1 = econData.arrays[1];
			var array2 = econData.arrays[2];
			var array4 = econData.arrays[4];
			var array5 = econData.arrays[5];

			int taxes = array1[0];
			int slavery = array1[1];
			int trade = array1[2];
			int maint = -array5[8];
			int armyUpkeep = -array5[1];
			int navyUpkeep = -array5[2]; // TODO: guess, figure out
			int interest = array1[4];
			int tributeToOverlord = -array5[7];
			int tributeFromPuppets = array2[3];
			int otherIncome = array2[4];
			int otherExpenses = 0;	// TODO: anything not explicitly called out above

			// Current turn treasury deductions
			// Don't show these as costs, since already deducted from treasury in-game.  Maybe report separately at some point.
			int construction = -array4[0];
			int agentCosts = 0; // TBD
			int recruitment = 0; // TBD

			int totalIncome = taxes + slavery + trade + interest + tributeFromPuppets + otherIncome;
			int totalExpense = armyUpkeep + navyUpkeep + maint + tributeToOverlord;
			report.AppendFormat("      Taxes            {0,10:#,#}     Army Upkeep {1,10:#,#}\n", taxes, armyUpkeep);
			report.AppendFormat("      Slave population {0,10:#,#}     Navy Upkeep {1,10:#,#}\n", slavery, navyUpkeep);
			report.AppendFormat("      Trade            {0,10:#,#}     Maintenance {1,10:#,#}\n", trade, maint);
			report.AppendFormat("      Tribute received {0,10:#,#}     Tribute paid{1,10:#,#}\n", tributeFromPuppets, tributeToOverlord);
			report.AppendFormat("      Interest         {0,10:#,#}\n", interest);
#if NO // Don't show construction as a cost, it's already deducted from treasury in-game.  Maybe report separately at some point.
			report.AppendFormat("                                      Construction     {0,10:#,#}\n", construction);
#endif // NO
			report.AppendFormat("      Other            {0,10:#,#}     Other       {1,10:#,#}\n", otherIncome, otherExpenses);
			report.  AppendLine("                       ----------                 ----------");
			report.AppendFormat("                       {0,10:#,#}                 {1,10:#,#}    = {2:#,#} net income\n", totalIncome, totalExpense, totalIncome + totalExpense);
		}
	}
}
