# DragWheel #

### *Hands-on-joystick-and-mouse (HOJAM) experiment for [Falcon BMS](https://www.benchmarksims.org/)* ###
### *(Use your mouse as a throttle!)* ###

Are you tired of reaching back and forth, from your mouse to the dodgy 8-bit slider on your cheapo-joystick?  Me too.

What if you could control the throttle by simply *dragging* the mouse forward and back?  There isn't any other interaction in the 3D cockpit controlled by drag/drop which should conflict with that.

This simple prototype is a proof-of-concept, to try that out -- it works by hooking mouse input in the background, watching for (by default) left-button-drag movements, and generating mousewheel-rotation input events in response.  If you assign throttle-axis to the mousewheel in BMS, voila -- your mouse is now a HOTAS throttle!  (With far fewer knobs and buttons.. but hey, you can use it to look around.)

This is still just a crude prototype -- but I'd like to get feedback and hear ideas before developing it further.  (Is this useful for anyone but me?)

## Change Log ##

#### v1.1.2021.522:

- Allow sound effects to be overridden with custom wav files.
- Also allow mouse wheelbutton to mapped to a keyboard scancode -- 
  - only suitable for press-and-hold keys (because a short-tap will always set throttle to 50% and reset fov).
  - but works great for the `SimHotasShift` callback, to take advantage of secondary stick buttons without sacrificing one of them.
  - eg: `<add key="ScancodeForMouseWheelButton" value="0x39"/><!-- [spacebar] -->`

#### v1.1:

- Implemented rudimentary tracking of throttle-lever position, and audible feedback when your throttle hits the minimum/maximum extent (and push-through afterburner detent).
  - NOTE: for this to work, you must have mousewheel-sensitivity (in Falcon settings) set all the way to minimum.
- Bonus feature: for those with a 5-button mouse.. you can map the two x-buttons to keyboard scancodes.  Some fun ideas: 
  - PTT for UHF/VHS comms
  - Look-ahead / Check-six
  - Look-closer / Reset-FOV
  - Speedbrake in/out
  - Chaff/Flare programs (eg. slap switch / pgm #6)

## Instructions ##

#### *Download and Install:*

- https://github.com/arithex/DragWheel/releases 
- Simply download and unzip, anywhere you like.  No installer, no dependencies, and no special permissions required.
  - Should run ok on Windows 10 version 1903 and later (requires .NET Framework 4.8).

#### *Review settings in `DragWheel.exe.config` file:*

- (optional) Adjust the `MouseResolution` value in `DragWheel.exe.config` if your mouse is more or less sensitive than mine.  For a 1200 dpi mouse the total "throw" of travel for the throttle is about 2.5 inches of mouse movement.

- (protip) You can also change the `MouseButton` used to manipulate the throttle .. button #0 (left-button) is the default.  But if you have a 5-button mouse I recommend using button #3 or #4 (aka zero-based index of button-4 or -5) so there's no possibility of an errant click, when beginning to drag.
- (protip) I've also added the ability to use a `JoystickButton` instead of a mouse button.  If your stick has a pinky/dx-shift configured, I've found that can work well for engaging the throttle.

#### *Review related settings in `falcon bms.cfg` file:*

- (recommended) Disable wheel-knobs to avoid grabbing and spinning random knobs by accident: 
  `set g_bMouseWheelKnobs 0`
- (recommended) Disable clickable-hotspot anchoring, to clear the way for smooth uniform, mouse dragging: 
  `set g_b3DClickableCursorAnchored 0`
- (recommended) Disable 4th-button clickable-pit toggle, if you want to utilize the new x-button remapping: 
  `set g_bMouseButton4TogglesClickablePit 0`

#### *Pre-flight:*

- Simply launch `DragWheel.exe` before launching BMS .. it will open in a console window.  Left-click and drag the window up and down, by its titlebar, and observe the stream of console output to verify it's working.
  - NOTE: to avoid messing up interaction in the 2D environment, wait to launch DragWheel.exe until after you enter 3D.
- (optional) Relocate your mouse to the left side of your keyboard .. wherever you'd place your hotas throttle if you had one.

#### *In BMS:*

- Setup / Controllers => reassign the throttle to the mousewheel.
- (recommended) Set the mousewheel sensitivity slider all the way to minimum, for finely-tuned throttle control.
- (consider) Whether you want to fly with clickable-cockpit on or off (mouselook mode) by default.  (See protip below.)
- (optional) Assign your throttle slider to something else if you want, eg. FOV or toe-brakes.

#### *In the jet:*

- Use right-button-drag to look around, as you normally do.
- ***Use left-button-drag (up and down) to control the throttle (forward and back, respectively).***
- (protip) If you prefer to fly with mouselook engaged, you can press and hold both left+right buttons simultaneously to move the throttle without panning the view up or down.  (Reminder: [alt+3] is the default key binding to toggle between mouselook and clickable-cockpit modes.)
- (protip) If you have a dx-shift button defined, and it's convenient to press-and-hold on the stick, you can configure that as the button to engage the throttle.

#### *Debrief:*

- Close DragWheel.exe by pressing [ctrl+C] or simply close its console window.

## Benefits

- HOTAS flying sensation, on a shoestring budget -- experience the ability to look around while keeping hands on stick and "throttle"!

- Vastly increased throttle sensitivity, compared to most cheap 8-bit consumer joystick sliders (eg. Thrustmaster T.16000M, or Logitech Extreme 3D Pro) .. makes easier landing, taxiing, holding formation, etc.


## Caveats

- This is just a prototype / proof of concept.  The most obvious downside to this approach is, you lose ability to control FOV with the mousewheel.
  - Setting `g_fNarrowFOV 40` in bms.cfg can help mitigate that, if you assign `FOVToggle` "Look Closer" to a stick button you can make regular use of that while keeping hotas.
  - If your stick has a throttle-slider, consider assigning that to control FOV.
- Mapping mousewheel-to-throttle isn't well supported or documented by BMS.  There are many quirks.. eg. if you click the middle-button it will still reset fov, like normal, but also it will snap your throttle to the 50% point.  And beware taxiway-starts -- be ready on the wheelbrakes! -- you will spawn in with throttle immediately set to 50% (ie. about 75% MIL-power).
  - Sometimes the throttle in BMS will stop responding to mousewheel entirely .. when that happens, clicking the wheel (middle) button usually re-engages it.. usually.

## Future

To avoid all the quirks and conflicts with hardcoded mousewheel behaviors, and to provide more reliable behavior based on knowing the absolute throttle-axis position -- this app needs to be rebuilt as a vJoy "feeder" application.

That should be more stable, supportable, and allow the mousewheel to return to its rightful role controlling FOV.

Other changes in progress:

- use BMS shared memory interface to only hook the mouse while in 3D pit
- optional overlay to display throttle position, rpm, ftit, brake, etc