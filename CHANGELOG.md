# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [0.21.0] - 2025-01-23

- UT now works without a yaml
- Fix rocks trap breaking the final boss elevator
- Fix shop rando leaking into other saves
- Bram is now allowed uppies

## [0.20.0] - 2025-01-07

- UT Map tabs now auto switch! (UT v0.1.15+)
- Fix map location
- Fix logic

## [0.19.0] - 2025-01-02

- Add new options for fast blood chalice
- Fix playing casual saves with the mod active
- Fix save data bleeding between saves
- Fix dying to spikes when approaching Volantis from the left
- More logic fixes
- More map fixes
- Update Archipelago.MultiClient.Net to 6.5.0

## [0.18.2] - 2024-10-13

- Improve generation times
- Make blood chalice normal speed while in boss rooms
- More map fixes
- More logic fixes

## [0.18.1] - 2024-09-08

- Add more cutscene traps
- Add Map Progression item group
- More logic fixes

## [0.18.0] - 2024-09-08

- Added cutscene and rock traps
- Added option to configure how much filler is turned into traps
- More logic fixes

## [0.17.2] - 2024-09-07

- Fix typo in Hall of the Phantoms switch names

## [0.17.1] - 2024-09-07

- Fix trio option for randomize characters

## [0.17.0] - 2024-09-04

- Added candle randomizer
- More logic fixes
- Minor optimizations

## [0.16.0] - 2024-08-20

- Added support for Poptracker inside of Universal Tracker
- Fixed some inconsistencies with item and location names
- Slightly improved apworld caching

## [0.15.3] - 2024-08-04

- Fix generating with item links
- Fix progression+useful items from other worlds not displaying as progression
- Fix not being able to get past the tutorial rooms with character rando on but skip cutscenes off
- More logic fixes
- Update Archipelago.MultiClient.Net to 6.3.0

## [0.15.2] - 2024-07-07

- Update Archipelago.MultiClient.Net to 6.2.0

## [0.15.1] - 2024-06-22

- Adjust character starting stats

## [0.15.0] - 2024-06-22

- Add option to modify character starting attack and defence based on what sphere they're unlocked in
- Fix player alias display

## [0.14.2] - 2024-06-22

- Fix received deals not working when randomzing the shop
- Display player aliases if they have them set
- More logic fixes

## [0.14.1] - 2024-06-17

- Add `always_restore_candles` option
- Fix Bell and Bow not being immediately usable when received
- More logic fixes

## [0.14.0] - 2024-06-16

- Upgrade AP MultiClient to v6 for better 0.5.0 support
- Fix more campfire warp edge cases
- Add allow_block_warping option to allow Zeek to carry a block through a campfire warp
- More logic fixes

## [0.13.0] - 2024-06-15

- Refactor item and location id handling to resolve issues related to datapackage bugs
- Refactor scouting to get all scouts at the start, avoiding any issues when entering rooms
- Rename items to have a more readable format
- Automatically connect to the previously used server upon loading a game
  - If the port is changed, connect to the new server in the main menu before loading
- Skip cutscenes option now skips miniboss death animations and health/attack pickup animations
- Fix unlocking the elevator when apex elevator option is set to vanilla and elevators are not randomized
- More logic fixes

## [0.12.0] - 2024-05-12

- Add Eye Hunt as a new goal
  - You can configure how many additional gold eyes you need before you can use the elevator to the final boss
  - The new gold eyes currently appear in the world as green (for now)
  - Red, blue, and green eyes are still required and open the corresponding doors as normal
  - Your progress towards the goal can be seen in the F1 debug menu
- More logic fixes

## [0.11.0] - 2024-05-12

- Rework campfire warp to look nicer and be less buggy
  - Campfire warps are now unlocked by actually saving at a campfire
  - Warping to a campfire will autosave
  - Added the dev room campfire to the list
- Add options to start with ascendant key and athena's bell
- Update apex elevator option to let you disable it entirely
- Add option for controlling if key items are randomized
- Skip game intro screens and first cutscene on new game
- Logic fixes

## [0.10.5] - 2024-05-03

- Fix checking elevator locations in elevator rando
- Fix some items not being received correctly
- Add F1 debug option to continue running the game when it's in the background instead of pausing
  - This may help if you've had issues with occasional crashing
- More logic fixes

## [0.10.4] - 2024-04-25

- Fix getting elevator key before any destinations
- Make cheap shining ray only require 50 orbs
- Allow starting with a specific character
- More logic fixes
- Update docs

## [0.10.3] - 2024-04-21

- Various fixes

## [0.10.2] - 2024-04-21

- Various fixes

## [0.10.1] - 2024-04-20

- Various fixes

## [0.10.0] - 2024-04-20

- Add character randomizer
  - If you don't start with Algus, Arias, or Kyuli, each one has a new sphere 1 location check added. You can complete them by just entering each character's tutorial room at the start of the game
- Add elevator randomizer
- Add switch randomizer
- Cyclops Idol and Prince's Crown are now randomized
- Add cheap Kyuli Shining Ray option

## [0.9.2] - 2024-04-04

- Fix more logic
- Fix shop upgrades not visually changing to AP items in some circumstances

## [0.9.1] - 2024-04-02

- Fix receiving items when quitting to the main menu and reloading a save
- Fix 1 logic check

## [0.9.0] - 2024-04-01

- Added shop randomizer
- Rewrote save file handling and detection, the mod will no longer mess with existing casual saves

## [0.8.1] - 2024-03-30

- Adds "open early doors" option to make sphere 1 bigger when using white and blue key randos
- Adds a sfx to campfire warps
- Animate doors opening when you receive them while in the same room
- Prevents campfire warp spam so you can't skip into rooms (sorry Sent)
- Add infinite jump debug cheat

## [0.8.0] - 2024-03-26

- Add Campfire Warp option that lets you warp between unlocked checkpoints with the debug menu (F1)
- Add Fast Blood Chalice option to make blood chalice heal 5x faster
- More logic fixes

## [0.7.0] - 2024-03-25

- Full support for white and blue key rando
- Disable game input when filling out the AP connection information so you don't interact with menus
- Ensure all in-game items have their appearance changed
- Show progression AP items as blue orbs, everything else as grey orbs
- Skip dialogue when fighting bosses the first time

## [0.6.1] - 2024-03-18

- Items now appear in-world based on what AP item is actually there
  - Astalon items (both yours and other's) will appear as the actual item
  - Items from other games will appear as a blue orb
- Fix random crash caused by console

## [0.5.1] - 2024-03-15

- Remember what was checked after getting disconnected and resend those locations when reconnected
- Fix double collection of some items
- Disable input when using the AP console
- Add support for white and blue keys (but not yet enabled since there's no logic)

## [0.5.0] - 2024-03-12

- Connection settings, debug options, and AP messages are all now available in an in-game UI. You no longer need to edit config files or use the text client.

## [0.4.1] - 2024-03-10

- Fix issues with item display and collecting duplicate items
- Fix stutter when collecting an item
- Support cost multiplier
- Automatically unlock elevators when you get ascendant key
  - You need to find a 2nd elevator (or have free Apex elevator enabled) before Gorgon Tomb 1 will be usable

## [0.4.0] - 2024-03-10

- Fix issues with item display and collecting duplicate items
- Fix stutter when collecting an item
- Support cost multiplier
- Automatically unlock elevators when you get ascendant key
  - You need to find a 2nd elevator (or have free Apex elevator enabled) before Gorgon Tomb 1 will be usable

## [0.3.1] - 2024-03-08

- Fix death link double sending and display death link message when receiving death

## [0.3.0] - 2024-03-08

- Fix most item display issues
- Fix death link, can be toggled by pressing Left Ctrl + Left Shift + L
- Add red key randomizer
- Allow disabling getting the apex elevator for free

## [0.2.0] - 2024-03-05

- Fix transition bug
- Fix goal completion
- Fix location logic issues
- Support starting with Zeek or Bram
- Support starting with some QoL upgrades
- Support skipping some cutscenes
- Support death link
- You can now force yourself to die by pressing Ctrl+Shift+K

## [0.1.1] - 2024-03-02

- First testing release, supports most key items, max health pickups, and attack pickups
