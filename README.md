# Archipelago-Signalis
A randomizer mod for the game [SIGNALIS](https://store.steampowered.com/app/1262350/SIGNALIS/) by rose-engine, to use with the [Archipelago randomizer](https://archipelago.gg/).

Full info and set up on this Randomizer: [https://github.com/devoidlazarus/SIGNALISArchipelagoRandomizer](https://github.com/devoidlazarus/SIGNALISArchipelagoRandomizer)

### Mod installation steps
1. Launch SIGNALIS at least once after installing it in order to setup the necessary game files, then close it after it loads into the main menu
2. Download MelonLoader 0.5.7 and open `MelonLoader.Installer.exe`
3. When prompted for a game executable during MelonLoader installation, locate your `signalis.exe` file
   - Default location for a Steam installation of SIGNALIS is `C:\Program Files (x86)\Steam\steamapps\common\SIGNALIS`
4. After MelonLoader is finished installing, open SIGNALIS again to let MelonLoader setup the required mod support files. You can close the game again after it loads into the main menu
5. Copy the `ArchipelagoSignalis.dll` and `Archipelago.MultiClient.Net.dll` files you downloaded and paste them into the newly-created `Mods` folder located in your SIGNALIS installation directory
6. Launch SIGNALIS
7. Open the Settings menu and select Enter Archipelago Connection
8. Enter your Archipelago slot name, server, and port information
   - It is a known issue at the moment that you cannot see the cursor when entering your Archipelago connection settings. Please use the `Tab` key to select different fields of the form
9. Select Done
   - If you cannot locate your cursor to select Done, hold down `Alt` and press `Tab` to exit the game window momentarily, line up your visible cursor with where the Done button would be, press `Tab` until the SIGNALIS window is selected, and then release `Alt`
10. Exit the Settings menu
11. Select Continue
    - If you have no existing save files, skip to step 15
12. Select Begin Anew

Any time you launch SIGNALIS to play an existing randomized save file, repeat steps #9-15 to successfully load into your save and connect to Archipelago.
