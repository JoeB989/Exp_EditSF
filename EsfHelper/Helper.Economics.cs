using EsfLibrary;
using System.Text;

namespace EsfHelper
{
	static public partial class Helper
	{
		static private void EconomicReport(ParentNode factionNode, StringBuilder report, GlobalEco globalEco)
		{
			var economics = FindChild(factionNode, "FACTION_ECONOMICS");
			int treasury = ((OptimizedIntNode)economics.Values[0]).Value;
			report.AppendLine("  Economics:");
			report.AppendFormat("    Treasury: {0:#,#}\n", treasury);
			if (globalEco != null)
			{
				globalEco.Factions++;
				globalEco.Treasury += treasury;
			}

			var history = economics.Children[0];
			int index = history.Children.Count - 1;
			if (index >= 0)
			{
				var econDataNode = history.Children[index].Children[0];
				var econData = readRawEconData(econDataNode);
				// Disabled because (vanilla) horde factions have 0 tax
				//int taxes = econData.arrays[1][0];
				//if (taxes == 0)
				//{
				//	report.AppendLine("    Faction has been destroyed");
				//	return;
				//}

				report.AppendLine("    Projected this turn:");
				reportEconData(econData, report, globalEco);

				if (index >= 1)
				{
					var priorDataNode = history.Children[index - 1].Children[0];
					var priorData = readRawEconData(priorDataNode);
					report.AppendLine("    Last turn:");
					reportEconData(priorData, report, null);	// null => don't accumulate prev turn in globalEco
				}
			}
		}

		public struct RawEconData
		{
			public Dictionary<int, Dictionary<int, int>> arrays;
		}

		static private RawEconData readRawEconData(ParentNode econDataNode)
		{
			// I thought econ data were always OptimizedArrayNodes but apparently sometimes they are EsfArrayNodes
			// So now need to handle both types
			RawEconData data = new RawEconData();
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

		public class EconData
		{
			public int Taxes;
			public int Slavery;
			public int Trade;
			public int Maint;
			public int ArmyUpkeep;
			public int NavyUpkeep;
			public int Interest;
			public int TributeToOverlord;
			public int TributeFromPuppets;
			public int OtherIncome;
			public int OtherExpenses;

			public int Construction;
			public int AgentCosts;
			public int Recruitment;

			public int TotalIncome;
			public int TotalExpenses;
		}

		static public EconData ReadEconData(RawEconData rawEconData)
		{
			EconData econData = new EconData();

			var array1 = rawEconData.arrays[1];
			var array2 = rawEconData.arrays[2];
			var array4 = rawEconData.arrays[4];
			var array5 = rawEconData.arrays[5];

			econData.Taxes = array1[0];
			econData.Slavery = array1[1];
			econData.Trade = array1[2];
			econData.Maint = -array5[8];
			econData.ArmyUpkeep = -array5[1];
			econData.NavyUpkeep = -array5[2]; // TODO: guess, figure out
			econData.Interest = array1[4];
			econData.TributeToOverlord = -array5[7];
			econData.TributeFromPuppets = array2[3];
			econData.OtherIncome = array2[4];
			econData.OtherExpenses = 0;  // TODO: anything not explicitly called out above

			// Current turn treasury deductions
			// Don't show these as costs, since already deducted from treasury in-game.  Maybe report separately at some point.
			econData.Construction = -array4[0];
			econData.AgentCosts = 0; // TBD
			econData.Recruitment = 0; // TBD

			econData.TotalIncome = econData.Taxes +
								   econData.Slavery +
								   econData.Trade +
								   econData.Interest +
								   econData.TributeFromPuppets +
								   econData.OtherIncome;
			econData.TotalExpenses = econData.ArmyUpkeep +
									 econData.NavyUpkeep +
									 econData.Maint +
									 econData.TributeToOverlord;

			return econData;
		}

		static private void reportEconData(RawEconData rawEconData, StringBuilder report, GlobalEco globalEco)
		{
			EconData econData = ReadEconData(rawEconData);

			report.AppendFormat("      Taxes            {0,10:#,#}     Army Upkeep {1,10:#,#}\n", econData.Taxes, econData.ArmyUpkeep);
			report.AppendFormat("      Slave population {0,10:#,#}     Navy Upkeep {1,10:#,#}\n", econData.Slavery, econData.NavyUpkeep);
			report.AppendFormat("      Trade            {0,10:#,#}     Maintenance {1,10:#,#}\n", econData.Trade, econData.Maint);
			report.AppendFormat("      Tribute received {0,10:#,#}     Tribute paid{1,10:#,#}\n", econData.TributeFromPuppets, econData.TributeToOverlord);
			report.AppendFormat("      Interest         {0,10:#,#}\n", econData.Interest);
#if NO // Don't show construction as a cost, it's already deducted from treasury in-game.  Maybe report separately at some point.
			report.AppendFormat("                                      Construction     {0,10:#,#}\n", construction);
#endif // NO
			report.AppendFormat("      Other            {0,10:#,#}     Other       {1,10:#,#}\n", econData.OtherIncome, econData.OtherExpenses);
			report.  AppendLine("                       ----------                 ----------");
			report.AppendFormat("                       {0,10:#,#}                 {1,10:#,#}    = {2:#,#} net income\n", econData.TotalIncome, econData.TotalExpenses, econData.TotalIncome + econData.TotalExpenses);

			if (globalEco != null)
			{
				globalEco.Taxes += econData.Taxes;
				globalEco.Trade += econData.Trade;
				globalEco.Income += econData.TotalIncome;
				globalEco.Expenses += econData.TotalExpenses;
			}
		}
	}
}
