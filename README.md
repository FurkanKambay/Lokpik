# Lokpik

Lockpicking game mechanic based on real life without using physics. Currently only supports single pin picking with pin tumbler locks.

## Controls

- `A`/`D`: Select the Pin to manipulate.
- `Mouse Up/Down` or `W`/`S`: Lift the current Pin using the lockpick tool.
- `Space`: Hold tension (and release)

> Controls are a work-in-progress. I'm playing around with it to get the feel for the tensioning and the pin manipulation just right. Currently, lifting pins with the mouse feels good, but I haven't perfected holding the tension.

## Features

- Simple 2D visual representation of a plug, key & driver pins, and applied tension/torque (no visual for the lockpick yet)
- Gradually applied tension/torque affects plug rotation
- `TumblerLockConfig` class with options:
  - Pin/chamber count
  - Driver and Key Pin lengths
  - Shear line height (constrained above every key pin)
  - Shear line tolerance to set pins
  - Binding angles (effectively the inverse of pin width; large values will exaggerate the plug rotation after picking a pin)
- Support for comb pick vulnerability (all pin stacks raised above the shear line)

## Plans

- Counter-rotation
- Audio feedback
- Random lock generation
