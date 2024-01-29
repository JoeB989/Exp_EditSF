using System.Xml;
using System.Xml.Linq;

namespace TDD_TileGen
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private string InputFile = string.Empty;
		private string OutputFile = string.Empty;

		private void EnableGenerate()
		{
			GenerateButton.Enabled = (!string.IsNullOrWhiteSpace(InputFile)) && (!string.IsNullOrWhiteSpace(OutputFile));
		}

		private void SelectInputButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
			ofd.Title = "Select input file";
			ofd.CheckFileExists = true;
			if (ofd.ShowDialog(this) == DialogResult.OK)
			{
				InputFile = ofd.FileName;
				InputFileLabel.Text = InputFile;
				EnableGenerate();
			}
		}

		private void SelectOutputButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
			ofd.Title = "Select output file";
			ofd.CheckFileExists = false;
			if (ofd.ShowDialog(this) == DialogResult.OK)
			{
				OutputFile = ofd.FileName;
				OutputFileLabel.Text = OutputFile;
				EnableGenerate();
			}
		}

		private void GenerateButton_Click(object sender, EventArgs e)
		{
			List<Faction> factions;
			try
			{
				factions = ReadCities();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Input data error");
				return;
			}
			XmlDocument doc = GenerateTileUpgrades(factions);
			doc.Save(OutputFile);
			MessageBox.Show("Done!", "Done");
		}

		private List<Faction> ReadCities()
		{
			/* Sample xml input format:
			<DATA>	
				<FACTION name='rom_sc_dunedain' id='gondor' comment='Gondor'>
					<CITY level='minor'  variants='a,b,c' />
					<CITY level='small'  variants='a,b,c' />
					<CITY level='medium' variants='a,b,c' />
					<PORT level='small'  variants='a,b' />
					<PORT level='medium' variants='a,b' />
					<UNIQUE_CITY name='cair_andros'  levels='small,medium' />
					<UNIQUE_CITY name='osgiliath'	 levels='small,medium' />
					<UNIQUE_CITY name='ethring'		 levels='small,medium' />
					<UNIQUE_CITY name='minas_tirith' levels='small,medium' />
					<UNIQUE_PORT name='annulond'	 levels='small,medium' />
					<UNIQUE_PORT name='dol_amroth'	 levels='small,medium' />
					<UNIQUE_PORT name='linhir'		 levels='small,medium' />
					<UNIQUE_PORT name='lond_galen'	 levels='small,medium' />
					<UNIQUE_PORT name='pelargir'	 levels='small,medium' />
				</FACTION>
			</DATA>
			*/
			List<Faction> factions = new List<Faction>();
			XmlDocument doc = new XmlDocument();
			doc.Load(InputFile);

			foreach (var node in doc.ChildNodes)
			{
				XmlElement? element = node as XmlElement;	// skip XmlComments
				if (element != null)
				{
					foreach (XmlElement factionNode in element.ChildNodes)
					{
						if (string.Compare(factionNode.Name, "FACTION", true) != 0)
							throw new InvalidDataException("Expected FACTION node at:\n" + factionNode.OuterXml);
						Faction faction = new Faction();
						faction.Name = factionNode.GetAttribute("name");
						faction.Id = factionNode.GetAttribute("id");
						faction.Comment = factionNode.GetAttribute("comment");
						factions.Add(faction);

						foreach (XmlElement cityNode in factionNode.ChildNodes)
						{
							switch (cityNode.Name.ToUpper())
							{
								case "CITY":
									var city = new Generic(CityType.City);
									city.Level = cityNode.GetAttribute("level");
									city.Variants = cityNode.GetAttribute("variants").Split(',');
									faction.Generics.Add(city);
									break;
								case "PORT":
									var port = new Generic(CityType.Port);
									port.Level = cityNode.GetAttribute("level");
									port.Variants = cityNode.GetAttribute("variants").Split(',');
									faction.Generics.Add(port);
									break;
								case "UNIQUE_CITY":
									var ucity = new Unique(CityType.City);
									ucity.Name = cityNode.GetAttribute("name");
									ucity.Levels = cityNode.GetAttribute("levels").Split(',');
									faction.Uniques.Add(ucity);
									break;
								case "UNIQUE_PORT":
									var uport = new Unique(CityType.Port);
									uport.Name = cityNode.GetAttribute("name");
									uport.Levels = cityNode.GetAttribute("levels").Split(',');
									faction.Uniques.Add(uport);
									break;
								default:
									throw new InvalidDataException("Unexpected node at:\n" + cityNode.OuterXml);
							}
						}
					}
				}
			}

			return factions;
		}

		private XmlDocument GenerateTileUpgrades(List<Faction> factions)
		{
			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement("TILE_UPGRADES");
			doc.AppendChild(root);

			foreach (var faction in factions)
			{
				// <!---------------------- GONDOR ----------------------->	
				root.AppendChild(doc.CreateComment(string.Format(" ===================== {0} ===================== ", faction.Comment.ToUpper())));
				// < !-- Gondor Minor Settlements -->
				root.AppendChild(doc.CreateComment(string.Format(" {0} Minor Settlements ", faction.Comment)));
				// < UPGRADE_GROUP name = 'rom_sc_dunedain' >
				var upgradeGroup = doc.CreateElement("UPGRADE_GROUP");
				upgradeGroup.SetAttribute("name", faction.Name);
				root.AppendChild(upgradeGroup);

				//		<!-- Minor Cities -->
				// TODO: should minor/small/medium be read out of city level instead of hardcoded?
				upgradeGroup.AppendChild(doc.CreateComment(" Minor Cities "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "minor", "minor", CityType.City);

				//		<!-- Minor Ports -->
				upgradeGroup.AppendChild(doc.CreateComment(" Minor Ports "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "minor", "minor", CityType.Port);

				// < !--Gondor Major Settlements -->
				root.AppendChild(doc.CreateComment(string.Format(" {0} Major Settlements ", faction.Comment)));
				// < UPGRADE_GROUP name = 'level1_rom_sc_dunedain' >
				upgradeGroup = doc.CreateElement("UPGRADE_GROUP");
				upgradeGroup.SetAttribute("name", "level1_"+faction.Name);
				root.AppendChild(upgradeGroup);

				//		<!-- Small Cities -->
				upgradeGroup.AppendChild(doc.CreateComment(" Small Cities "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "small", "small", CityType.City);

				//		<!-- Small Ports -->
				upgradeGroup.AppendChild(doc.CreateComment(" Small Ports "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "small", "small", CityType.Port);

				//		<!-- Medium Cities -->
				upgradeGroup.AppendChild(doc.CreateComment(" Medium Cities "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "medium", "small", CityType.City);

				//		<!-- Medium Ports -->
				upgradeGroup.AppendChild(doc.CreateComment(" Medium Ports "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "medium", "small", CityType.Port);

				//		<!-- Uniques -->
				upgradeGroup.AppendChild(doc.CreateComment(" Uniques "));
				AddUniqueUpgrades(upgradeGroup, faction, factions, "small", CityType.City);
				AddUniqueUpgrades(upgradeGroup, faction, factions, "small", CityType.Port);


				// < UPGRADE_GROUP name = 'level2_rom_sc_dunedain' >
				upgradeGroup = doc.CreateElement("UPGRADE_GROUP");
				upgradeGroup.SetAttribute("name", "level2_" + faction.Name);
				root.AppendChild(upgradeGroup);

				//		<!-- Minor Cities -->
				upgradeGroup.AppendChild(doc.CreateComment(" Minor Cities "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "minor", "small", CityType.City);

				//		<!-- Minor Ports -->
				upgradeGroup.AppendChild(doc.CreateComment(" Minor Ports "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "minor", "small", CityType.Port);

				//		<!-- Small Cities -->
				upgradeGroup.AppendChild(doc.CreateComment(" Small Cities "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "small", "medium", CityType.City);

				//		<!-- Small Ports -->
				upgradeGroup.AppendChild(doc.CreateComment(" Small Ports "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "small", "medium", CityType.Port);

				//		<!-- Medium Cities -->
				upgradeGroup.AppendChild(doc.CreateComment(" Medium Cities "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "medium", "medium", CityType.City);

				//		<!-- Medium Ports -->
				upgradeGroup.AppendChild(doc.CreateComment(" Medium Ports "));
				AddGenericUpgrades(upgradeGroup, faction, factions, "medium", "medium", CityType.Port);

				//		<!-- Uniques -->
				upgradeGroup.AppendChild(doc.CreateComment(" Uniques "));
				AddUniqueUpgrades(upgradeGroup, faction, factions, "medium", CityType.City);
				AddUniqueUpgrades(upgradeGroup, faction, factions, "medium", CityType.Port);
			}
			return doc;
		}

		private const string CITY_PATH = @"terrain\tiles\battle\settlement_{0}_cities\{0}_city_{1}\{2}";
		private const string PORT_PATH = @"terrain\tiles\battle\settlement_{0}_ports\{0}_port_{1}\{2}";

		private string getGenericPath(string factionId,  CityType cityType, string variant, string level)
		{
			if (cityType == CityType.Port)
			{
				return string.Format(PORT_PATH, factionId, variant, level);
			}
			return string.Format(CITY_PATH, factionId, variant, level);
		}

		private string getUniquePath(string factionId, CityType cityType, string variant, string level)
		{
			if (cityType == CityType.Port)
			{
				return string.Format(PORT_PATH, factionId, variant, level);
			}
			return string.Format(CITY_PATH, factionId, variant, level);
		}

		private void AddGenericUpgrades(XmlElement upgradeGroup,
			Faction targetFaction, List<Faction> factions,
			string sourceLevel, string targetLevel, CityType type)
		{
			// match the src city_a\minor with the dst city_a\minor
			//				 city_b\minor			   city_b\minor
			//				 city_c\minor			   city_c\minor
			//
			// TODO: how to handle a mismatch?  where src has more or fewer variants than dst
			// Currently:
			//	- only look for variants in dst
			//	- if that variant is missing in src, omit it from output
			foreach (var faction in factions)
			{
				var examine = from dst in targetFaction.Generics
							  where (dst.Type == type) && (dst.Level == targetLevel)
							  select dst;
				foreach (var dst in examine)
				{
					foreach (var variant in dst.Variants)
					{
						Generic src = (from gen in faction.Generics
									   where (gen.Type == type) && (gen.Level == sourceLevel) && gen.Variants.Contains(variant)
									   select gen).FirstOrDefault();
						if (src != null)
						{
							XmlElement upgrade = upgradeGroup.OwnerDocument.CreateElement("UPGRADE");
							upgrade.SetAttribute("src", getGenericPath(faction.Id, src.Type, variant, src.Level));
							upgrade.SetAttribute("dst", getGenericPath(targetFaction.Id, dst.Type, variant, targetLevel));
							upgradeGroup.AppendChild(upgrade);
						}
					}
				}
			}
		}

		private void AddUniqueUpgrades(XmlElement upgradeGroup,
			Faction targetFaction, List<Faction> factions,
			string targetLevel, CityType type)
		{
			foreach (var faction in factions)
			{
				var examine = from dst in targetFaction.Uniques
							  where (dst.Type == type) && (dst.Levels.Contains(targetLevel))
							  select dst;
				foreach (var dst in examine)
				{
					Unique src = (from gen in faction.Uniques
								  where (gen.Name == dst.Name) && (gen.Type == type) && gen.Levels.Contains(targetLevel)
								  select gen).FirstOrDefault();
					if (src != null)
					{
						foreach (var level in src.Levels)
						{
							XmlElement upgrade = upgradeGroup.OwnerDocument.CreateElement("UPGRADE");
							upgrade.SetAttribute("src", getGenericPath(faction.Id, src.Type, src.Name, level));
							upgrade.SetAttribute("dst", getGenericPath(targetFaction.Id, dst.Type, dst.Name, targetLevel));
							upgradeGroup.AppendChild(upgrade);
						}
					}
				}
			}
		}
	}

	// <FACTION name='rom_sc_dunedain' id='gondor' comment='Gondor'>
	public class Faction
	{
		public string? Name;
		public string? Id;
		public string? Comment;
		public List<Generic> Generics = new List<Generic>();
		public List<Unique> Uniques = new List<Unique>();
	}

	// <CITY level='medium' variants='a,b,c' />
	// <PORT level = 'small'  variants='a,b' />
	public class Generic
	{
		public string? Level;
		public string[]? Variants;
		public CityType Type;
		public Generic(CityType type) { Type = type; }
		public bool IsCity () { return Type == CityType.City; }
		public bool IsPort() { return Type == CityType.Port; }
	}

	public enum CityType { City, Port /*, Castle*/ };

	// <UNIQUE_CITY name='minas_tirith' levels='small,medium' />
	// <UNIQUE_PORT name='annulond'     levels='small,medium' />
	public class Unique
	{
		public string Name;
		public string[] Levels;
		public CityType Type;
		public Unique(CityType type) { Type = type; }
		public bool IsCity() { return Type == CityType.City; }
		public bool IsPort() { return Type == CityType.Port; }
	}
}
