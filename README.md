# DragWheel #

### *A mouse-as-throttle (HOJAM) experiment for [Falcon BMS](https://www.benchmarksims.org/)* ###

Mouselook Gang!

Are you tired of reaching back and forth, from your mouse to the dodgy 8-bit slider on your cheapo-joystick?  Me too.

I wondered, what if we could control the throttle by simply *dragging* the mouse forward and back?  There isn't any other interaction in the 3D cockpit controlled by drag/drop which should conflict with that.

So I built this simple prototype as a proof-of-concept, to try it out -- it works by hooking mouse input in the background, watching for left-button-drag movement, and generating mousewheel-rotation input events in response.  If you assign throttle-axis to the mousewheel in BMS, voila -- your mouse is now a HOTAS throttle!  (With far fewer knobs and buttons.. but hey you can use it to look around.)

Currently this is just a crude prototype -- but I'd like to get feedback and hear ideas before developing it further.  (Is this useful for anyone but me?)  Here are the details for installation and tips for configuration:

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

#### *Pre-flight:*

- Simply launch `DragWheel.exe` before launching BMS .. it will open in a console window.  Left-click and drag the window up and down, by its titlebar, and observe the stream of console output to verify it's working.
- Relocate your mouse to the left side of your keyboard .. wherever you'd place your hotas throttle if you had one.

#### *In BMS:*

- Setup / Controllers => reassign the throttle to the mousewheel.
- (recommended) Set the mousewheel sensitivity slider all the way to minimum, for finely-tuned throttle control.
- (consider) Whether you want to fly with clickable-cockpit on or off (mouselook mode) by default.  (See protip below.)
- (optional) Assign your throttle slider to something else if you want, eg. FOV or toe-brakes.

#### *In the jet:*

- Use right-button-drag to look around, as you normally do.
- ***Use left-button-drag (up and down) to control the throttle (forward and back, respectively).***
- (protip) If you prefer to fly with mouselook engaged, you can press and hold both left+right buttons simultaneously to move the throttle without panning the view up or down.  (Reminder: [alt+3] is the default key binding to toggle between mouselook and clickable-cockpit modes.)

#### *Debrief:*

- Close DragWheel.exe by pressing [ctrl+C] or simply close its console window.

## Benefits

- HOTAS flying sensation, on a shoestring budget -- experience the ability to look around while keeping hands on stick and "throttle"!

- Vastly increased throttle sensitivity, compared to most cheap 8-bit consumer joystick sliders (eg. Thrustmaster T.16000M, or Logitech Extreme 3D Pro) .. makes easier landing, taxiing, holding formation, etc.


## Caveats

- This is just a prototype / proof of concept.  The most obvious downside to this approach is, you lose ability to control FOV with the mousewheel.
  - Setting `g_fNarrowFOV 40` in bms.cfg can help mitigate that, if you assign `FOVToggle` "Look Closer" to a stick button you can make regular use of that while keeping hotas.
  - If your stick has a throttle-slider, consider assigning that to control FOV.
- Mapping mousewheel-to-throttle isn't well supported or documented by BMS.  There are many quirks.. eg. if you click the middle-button it will still reset fov, like normal, but also it will snap your throttle to the 50% point.  And beware taxiway-starts -- be ready on the wheelbrakes -- you will spawn in with throttle immediately set to 50%!
  - Sometimes the throttle in BMS will stop responding to mousewheel entirely .. when that happens, clicking the wheel (middle) button usually re-engages it.. usually.

## Future

If this idea generates enough interest, I plan to pursue a different approach based on vJoy (or similar framework) to drive a virtual-axis for the throttle instead of relying on mousewheel.

That should be more stable, supportable, and allow the mousewheel to return to its rightful role controlling FOV.

With full control of a virtual-axis, we can also add the possibility of configurable detents (eg. stop the throttle at MIL power; require a second drag to go into AB).  Or perhaps add a 2nd axis to control two engines independently -- perhaps by dragging with buttons 4 & 5 held (for 5-button mice) or dragging with a modifier key held.  Or .. whatever other crazy ideas we come up with.

