## 1.0.6

- Fixed NREs from situations where the body is deleted prematurely (e.g. void explosion deaths)
- Damage events are no longer fired if HP is less than or equal to 0, to prevent them being prioritised over death events

## 1.0.5

- Added option to not apply specific transformations in the CSS

## 1.0.4

- A single instance of an EventSub is no longer limited to a single SkinDef and can now take an array of SkinDefs

## 1.0.3

- Fixed transformed limbs from drifting away when frozen
- Transformed limbs now retain their custom size on death

## 1.0.2
- Added checks to not apply transformations if in a vehicle state
- Added option to not apply any transformations in the CSS

## 1.0.1

- Fixed an issue when switching between skins on the same survivor using the transform controller

## 1.0.0

- Initial release
