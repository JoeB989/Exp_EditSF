using EsfLibrary;
using EsfHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CampaignReportNs
{
	public class Report
	{
		private List<SeriesTurn> _turns = new List<SeriesTurn>();
		private Helper.GameData _gameData;

		public bool AddSaveGame(FileInfo inputFile, ref int duplicates)
		{
			EsfFile file = EsfCodecUtil.LoadEsfFile(inputFile.FullName);

			// get turn number, discard if duplicate
			var gameData = Helper.GetGameData(file.RootNode);

			if ((from entry in _turns
				 where entry.Turn == gameData.Turn
				 select entry).Any())
			{
				duplicates++;
				return false;
			}
			else
			{
				var factionArrayNode = Helper.GetFactionArrayNode(file.RootNode);

				var entry = new SeriesTurn() { Turn = gameData.Turn };
				_turns.Add(entry);

				var regions = Helper.GetRegions(Helper.GetRegionArrayNode(file.RootNode));

				foreach (ParentNode factionEntryNode in ((ParentNode)factionArrayNode).Children)
				{
					ParentNode factionNode = factionEntryNode.Children[0];
					var faction = Helper.GetFactionDetails(factionNode, regions);
					if (faction != null)
						entry.Factions.Add(faction);
				}
			}
			return true;
		}

		public string GetTSV()
		{
			StringBuilder sb = new StringBuilder();
			produceReport(sb);
			return sb.ToString();
		}

		private void produceReport(StringBuilder sb)
		{
			reportHeader(sb);

			var sorted = from turn in _turns
						 orderby turn.Turn ascending
						 select turn;
			foreach (var turn in sorted)
			{
				reportTurn(turn, sb);
			}
		}

		private void reportTurn(SeriesTurn turn, StringBuilder sb)
		{
			foreach (var faction in turn.Factions)
			{
				reportFaction(turn.Turn, faction, sb);
			}
		}

		private void reportFaction(uint turn, SeriesTurnFaction faction, StringBuilder sb)
		{
			sb.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\n",
				turn,
				faction.Faction, faction.Prestige, faction.Regions, faction.Strength,
				faction.Treasury, faction.TotalIncome-faction.TotalExpenses, faction.TradeIncome);
		}
		private void reportHeader(StringBuilder sb)
		{
			// header includes an extra cell describing Faction Strength computation
			sb.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\tFaction Strength = {8}\n",
				"Turn",
				"Faction ID", "Prestige", "Regions", "Strength",
				"Treasury", "Net Income", "Trade Income",
				SeriesTurnFaction.FactionStrengthDescription);
		}
	}
}
