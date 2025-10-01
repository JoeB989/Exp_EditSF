using EsfHelper;
using EsfLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace CampaignReportNs
{
	public class Report
	{
		private List<SeriesTurn> _turns = new List<SeriesTurn>();
		private Helper.GameData _gameData;

		public bool AddSaveGame(FileInfo inputFile, ref int duplicates, bool getMapImage)
		{
			EsfFile file = EsfCodecUtil.LoadEsfFile(inputFile.FullName);

			// get turn number, discard if duplicate
			var gameData = Helper.GetGameData(file.RootNode, getMapImage);

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

				var entry = new SeriesTurn()
				{
					Turn = gameData.Turn,
					Map = gameData.Map
				};
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

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		public void GetMapAnim(GifBitmapEncoder gifEnc)
		{
			var font = new System.Drawing.Font("Calibri", 16);
			int textWidth = 100;
			int textHeight = 20;
			var sf = new StringFormat(StringFormat.GenericDefault);

			var sorted = from turn in _turns
						 orderby turn.Turn ascending
						 select turn;
			foreach (var turn in sorted)
			{
				// write the turn number onto the bitmap
				using (Graphics gr = Graphics.FromImage(turn.Map))
				{
					var rect = new RectangleF(turn.Map.Width - textWidth, 20, textWidth, textHeight);
					string text = string.Format("Turn {0}", turn.Turn);
					gr.DrawString(text, font, Brushes.Black, rect, sf);
				}

				IntPtr bmp = turn.Map.GetHbitmap();
				BitmapSource src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					bmp,
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());

				gifEnc.Frames.Add(BitmapFrame.Create(src));

				// poor man's replay delay: add a few extra copies of the last frame
				if (turn == sorted.Last())
				{
					for (int i = 0; i < 6; i++)
					{
						gifEnc.Frames.Add(BitmapFrame.Create(src));
					}
				}

				DeleteObject(bmp);
			}
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
