using EsfLibrary;
using System.Diagnostics.Metrics;
using System.Text;
using static EsfHelper.Helper;

namespace EsfHelper
{
	public class SeriesTurn
	{
		public uint Turn;
		public List<SeriesTurnFaction> Factions = new List<SeriesTurnFaction>();
	}
	public class SeriesTurnFaction
	{
		public uint FactionId;
		public string Faction;
		public uint Prestige;
		public int Regions;

		public int Treasury;
		public int TotalIncome;
		public int TotalExpenses;
		public int TradeIncome;

		public int FieldArmies, FieldLandUnits;
		public int GarrisonArmies, GarrisonLandUnits;
		public int FieldNavies, FieldNavalUnits;
		public int GarrisonNavies, GarrisonNavalUnits;

		public const string FactionStrengthDescription = "Regions + units/10  (including armies, navies, and garrisons)";
		public double Strength
		{
			get
			{
				return (double)Regions +
					(double)(FieldLandUnits + GarrisonLandUnits + FieldNavalUnits + GarrisonNavalUnits) / 10.0;
			}
		}
	}

	static public partial class Helper
	{
		static public List<Region> GetRegions(ParentNode regionArrayNode)
		{
			List<Region> regions = new List<Region>();
			List<ParentNode> regionNodes = new List<ParentNode>();
			foreach (var arrayNode in regionArrayNode.Children)
			{
				regionNodes.Add(arrayNode.Children[0]);
			}

			foreach (var regionNode in regionNodes)
			{
				string name = ((StringNode)regionNode.Values[1]).Value;
				uint factionId = ((OptimizedUIntNode)regionNode.Values[10]).Value;
				//uint wealth = ((OptimizedUIntNode)regionNode.Values[3]).Value;
				//bool desolate = (wealth == 0);
				regions.Add(new Region() { Name = name, FactionId = factionId });
			}
			return regions;
		}

		static public SeriesTurnFaction GetFactionDetails(ParentNode factionNode, List<Region> regions)
		{
			SeriesTurnFaction faction = new SeriesTurnFaction();

			// Faction info
			faction.FactionId = ((OptimizedUIntNode)(factionNode.Values[0])).Value;
			faction.Faction = ((StringNode)(factionNode.Values[1])).Value;
			faction.Regions = (from region in regions
							   where region.FactionId == faction.FactionId
							   select faction).Count();
			faction.Prestige = GetPrestige(factionNode);

			// Economics
			var economics = FindChild(factionNode, "FACTION_ECONOMICS");
			faction.Treasury = ((OptimizedIntNode)economics.Values[0]).Value;

			var history = economics.Children[0];
			int index = history.Children.Count - 1;
			if (index >= 0)
			{
				var econDataNode = history.Children[index].Children[0];
				var rawEconData = readRawEconData(econDataNode);
				var econData = ReadEconData(rawEconData);

				faction.TradeIncome = econData.Trade;
				faction.TotalIncome = econData.TotalIncome;
				faction.TotalExpenses = econData.TotalExpenses;
			}

			// Military (break into armies/fleets and field/garrison)
			var forces = Helper.GetMilitaryForces(factionNode);
			var subset = from force in forces
						 where force.Type == MilitaryForce.ForceType.FieldArmy
						 select force;
			faction.FieldArmies = subset.Count();
			faction.FieldLandUnits = (from force in subset
									  select force.Units).Sum();

			subset = from force in forces
					 where force.Type == MilitaryForce.ForceType.GarrisonArmy
					 select force;
			faction.GarrisonArmies = subset.Count();
			faction.GarrisonLandUnits = (from force in subset
									  select force.Units).Sum();

			subset = from force in forces
					 where force.Type == MilitaryForce.ForceType.FieldNavy
					 select force;
			faction.FieldNavies = subset.Count();
			faction.FieldNavalUnits = (from force in subset
									  select force.Units).Sum();

			subset = from force in forces
					 where force.Type == MilitaryForce.ForceType.GarrisonNavy
					 select force;
			faction.GarrisonNavies = subset.Count();
			faction.GarrisonNavalUnits = (from force in subset
									  select force.Units).Sum();

			return faction;
		}

		static private uint GetPrestige(ParentNode factionNode)
		{
			// /FACTION_ARRAY - 0/FACTION/PRESTIGE/PRESTIGE_TYPE/PRESTIGE_TYPE - 8/
			string[] prestigeHierarchy =
			{
				"PRESTIGE",
				"PRESTIGE_TYPE",
			};

			var prestigeType = WalkChildren(factionNode, prestigeHierarchy);
			var prestige8 = prestigeType.Children[8];
			var prestigeValues = ((OptimizedArrayNode<uint>)(prestige8.Values[0])).Value;

			uint prestige = 0;
			if (prestigeValues.Length > 0)
				prestige = prestigeValues[prestigeValues.Length - 1];

			return prestige;
		}
	}
}
