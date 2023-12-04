# EXP_EditSF
An experimental hacked-up version of EditSF for prototyping.
Developed with focus on [The Dawnless Days (TDD)](https://discord.com/channels/328911806372511744/811928236752896020) ([invite link](https://discord.gg/tdd)).
Originally copied from [EditSF 1.2.7](https://sourceforge.net/p/packfilemanager/code/HEAD/tree/trunk/) on sourceforge.

### Contents
- [EditSF 1.3.0](#EditSF-1.3.0)
- [CompareSF](#CompareSF)
- [TDD_Rotate](#TDD_Rotate)
- [How to build](#How-to-build)

## EditSF 1.3.0
Extends the public EditSF 1.2.7 with some reporting features to investigate save game state.

Note: All reports are currently dumped to a message box you can OK (or Enter) to copy to clipboard.
This clunky UI will be replaced at some point.

### Features
New Reports main menu - this is where all reporting features will move to
- Player Faction Report - gives a complete faction report on the player's faction.  Identical to Faction Report on FACTION_ARRAY - 0
- All Factions Economcs - gives an economics report of all factions
- All Factions Characters - give a report on all characters of all factions (family, nobles, candidates, agents)
- Verify Settlements - Performs some simple verifications in a save file.  The intent is to offer a quick acceptance test "sanity check" that can be run against release candidate builds to verify no regression:
  - no empty building slots
  - all settlement buildings have a garrison
  - wounded garrisons

New menu commands on the Faction Array node (FACTION_ARRAY)
- All Factions Economcs - gives an economics report of all factions
- All Factions Characters - give a report on all characters of all factions (family, nobles, candidates, agents)

New menu commands on any Faction node (FACTION_ARRAY - nn)
- Faction Report - gives a detailed report on a specific faction: economics, character, and army details
 - faction 0 is always the player faction
 - you can open any faction array node, click on the FACTION child node, and look at the second value row to see which faction it is

### Planned enhancements
- income from raiding
- slave population econ data
- army should list general and agent
	- include general details in the army list
- a Reports modeless dialog that
	- lets you checkbox factions
	- lets you checkbox report content
	- remembers the report settings over loading new save file
	- outputs results into dialog instead of message box (with Copy button)

### Other possible enhancements
- Campaign AI (CAI) details once I figure out that stuff
- Possible RPFM integration where reading db tables can help parse or display save file content (localized strings for example)

## CompareSF
Total War Attila save file comparator (with some hard-coded knowledge of TDD).
Compares 2 or more save game files and displays their differences.
The output is fairly crude at this point.

Optionally supply filenames on the command line to pre-populate the file list

>    CompareSF  [file1.save  file2.save  ...]

So far only tested with TW Attila save files

TODO:
1. Show parallel trees with
	- only differences
	- full files with differences highlighted, navigate differences buttons
2. Save a project with multiple file paths, so can re-examine as the game progresses

## TDD_Rotate
A work in progress utility to rotate unique settlement models on the campaign.
Because the Attila engine randomly rotates city models on campaign creation, and TDD has a number of unique city models that want a preset rotation.

Usage:

>	TDD_Rotate.exe inputFile.save  outputFile.save  "fix-list of buildings and hex directions"

The fix-list has the format:

>	id,slot,direction;id,slot,direction;...

- **id** is the region id
- **slot** is the building slot number.  Currently we only anticipate adjusting the settlement building in slot 0, but this allows for future expansion
- **direction** is 0-5

Known TDD region settlement rotations:
- 43, rom_reg_anorien_minas_tirith, 5
- 93, rom_reg_westfold_hornburg, 0 or 1 (pick 1 for now)
- 155, rom_reg_gap_of_rohan_isengard, 5

which resolves to a fix list: 43,0,5;93,0,1;155,0,5

### Open issues and Concerns
The intended direction is to embed the fix-list in a script or db entry and launch TDD_rotate "at an appropriate time" from a LUA script.

Warning: the command line from LUA is probably limited in length.
Can LUA set the working directory of the command?
Or should the app figure out Attila save directory and not need a path on the file names?

I've been thinking about all the challenges to solve, and I'm not feeling good about this approach to fix save files.
I'm feeling pretty hesitent about this approach to fix save files.
Specifically, how to solve all these issues in a light, unobtrusive way
- The 'app' is actually a .net exe that also requires 2 more dlls and a config file, so 4 files total, and 7zip.dll and EsfLibrary.dll are fairly large.  So 4 payloads have to be written as files, and the total size is much larger (about 106kb, compared to 12kb for TDD_12_9_slots)
- Then, we're also relying on a certain minimum .net installed on the user's PC.  With an installer-based install (like the launcher) this is easily managed (dependency checks, installer warnings), but in a UI-less background process, how will we detect errors, report them to user, and recover from them?  (Say .net version dependency is not present.)
- How will LUA script know when the app is done?  When outputFile.save exists?  Or is available for read access?
- What if outputFile never appears?  (app crashes etc.)  The script will need a timeout
- When will fix-game script run?  Scripts aren't loaded until faction game is entered, so how will the script know on entry if it's a new campaign or savegame loaded from main menu?  Would we always fix the game even in the latter case?

The workflow looks like:
1. main menu - new campaign, Accept
2. wait while game loads
3. after game loads, the script would
	- save game temp.save
	- os.execute TDD_Rotate temp.save temp_fixed.save fixlist
	- prevent further game UI interaction while processing
	- wait until temp_fixed.save exists and is not write locked, or timeout is reached (how long is a safe timeout?)
	- timed out?
      - no: load temp_fixed.save
      - yes: warn user that the file isn't fixed (let him retry again manually later?)
	- delete temp.save and temp_fixed.save (step could be disabled by a config setting if needed for debugging)
4. give UI control back to user

Alternatively:
1. main menu - load game, Accept
2. wait while game loads
3. after game loads, script determines it's a load not a new, does nothing
	- but don't yet know how to detect new vs. load

And I haven't even mentioned the time it will take to do all this, even if everything works perfectly.

## How to build
Currently I'm not distributing executables.  Maybe I'll eventually do so.

All you need is the Visual Studio 2022 Community Edition (free) to build all these projects.
