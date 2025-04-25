using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Policy;
using System.Xml.Linq;
using System;

namespace CampaignReportNs
{
	internal static class Program
	{
		/// <summary>
		///  Read a set of campaign savegames and produce a turn-by-turn report
		/// </summary>
		/// <remarks>
		/// 1. Choose savegame series
		///		- folder picker, default to save_games
		///		- process savefiles  down to list of faction names
		///			TDD 		"Northern Enedwaith 3015 TA Early March"
		///			vanilla		"Lakhmids 454 AD Spring"
		///			name-template "FACTION year IGNORED-STUFF"
		///		- show a list of campaigns with 2 or more entries, number of files in each, pick one
		/// 2. Process selected file series
		///		- progress bar with N of N, show filename
		///		- if multiple saves at same turn, use the latest read
		/// 3. Copy a csv to clipboard
		/// 4. Excel tables and charts
		///		- paste to excel
		///		- csv include headers?
		///		- can I format as pivot-table automatically?
		///		- graphs from table
		///		
		/// 
		/// Basic data
		///		difficulty
		///		player faction
		///		number of files processed (and duplicate turns ignored)
		///		first and last turn
		///		
		/// Turn
		///		optional year/month(half month?)
		///		
		/// per faction
		///		Name (identify which is player)
		///		Prestige
		///		Treasury, Net income, Trade?
		///		
		///		Regions
		///		Armies, Land Units
		///		Fleets, Naval Units
		///		
		/// future
		///		General ranks, Army/Fleet ranks
		///		
		/// far future
		///		graphs right in the app
		///		
		/// TDD specific reports
		///		turn that Rhurim is last easterling and Harad is last southron
		/// </remarks>
		[STAThread]
		static void Main()
		{
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			Application.Run(new CampaignReport());
		}
	}
}