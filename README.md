# Archipelago-Astalon

Randomizer mod for Astalon: Tears of the Earth that integrates with [Archipelago](https://archipelago.gg)

## Installation

Follow the [Setup Guide](https://github.com/drtchops/Archipelago/blob/astalon/worlds/astalon/docs/setup_en.md)

## Development

1. Install BepInEx and run your game once as per the setup guide
2. Copy `LocalOverrides.targets.example`, rename to `LocalOverrides.targets`, and edit it to point to the `BepInEx` folder of your local Astalon install
3. Debug build with `dotnet build`
4. Release build with `dotnet build -c release`
