# EXP_EditSF
An experimental hacked-up version of EditSF for prototyping.
Developed with focus on The Dawnless Days (TDD) https://discord.com/channels/328911806372511744/811928236752896020 (invite: https://discord.gg/tdd)

Originally copied from EditSF 1.2.7 on sourceforge https://sourceforge.net/p/packfilemanager/code/HEAD/tree/trunk/

## EditSF 1.3.0
Extends the public EditSF 1.2.7 with some reporting features to investigate save game state.

Note: All reports are currently dumped to a message box you can OK (or Enter) to copy to clipboard.
This clunky UI will be replaced at some point.

### Features
New Reports main menu - this is where all reporting features will move to
- Player Faction Report - gives a complete faction report on the player's faction.  Identical to Faction Report on FACTION_ARRAY - 0

New menu commands on the Faction Array node (FACTION_ARRAY)
- All Factions Economcs - gives an economics report of all factions
- All Factions Characters - give a report on all characters of all factions (family, nobles, candidates, agents)

New menu commands on any Faction node (FACTION_ARRAY - nn)
- Faction Report - gives a detailed report on a specific faction: economics, character, and army details
 - faction 0 is always the player faction
 - you can open any faction array node, click on the FACTION child node, and look at the second value row to see which faction it is

### Planned enhancements
- army should list general and agent
 - include general details in the army list
- a Reports modeless dialog that
 - lets you checkbox factions
 - lets you checkbox report content
 - remembers the report settings over loading new save file
 - outputs results into dialog instead of message box (with Copy button)
- slave population econ data

### Other possible enhancements
- CAI details once I figure out that stuff
- Possible PFM integration where reading db tables can help parse or display save file content (localized string for example)

## CompareSF
Total War Attila save file comparator (with some hard-coded knowledge of TDD).
Compares 2 or more save game files and displays their differences.
The output is fairly crude at this point.

Optionally supply filenames on the command line to pre-populate the file list
	CompareSF  [file1.save  file2.save  ...]

So far only tested with TW Attila save files

TODO:
1. Show parallel trees with
	a. only differences
	b. full files with differences highlighted, navigate differences buttons
2. Save a project with multiple file paths, so can re-examine as the game progresses
## TDD_Rotate
A work in progress utility to rotate unique settlement models on the campaign.
Because the Attila engine randomly rotates city models on campaign creation, and TDD has a number of unique city models that want a preset rotation.

## To build
Currently I'm not distributing executables.  Maybe I'll eventually do so.

All you need is the Visual Studio 2022 Community Edition (free) to build all these projects.
