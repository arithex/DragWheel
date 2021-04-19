using System;

namespace DragWheel
{
    internal class RawInputMouseHandler
    {
        int? _configuredMouseButton;
        byte? _xbutton1Scancode, _xbutton2Scancode;

        bool[] _mouseButtons;
        int _dx, _dy;

        Win32.RawInput.MouseButtonHandler _onMouseButton;
        Win32.RawInput.MouseMoveHandler _onMouseMove;
        Win32.RawInput.MouseWheelHandler _onMouseWheel;

        //--------------------------------------------------------------
        // Interface

        //----------------------------------------
        public volatile static bool DragActivated;

        //----------------------------------------
        public RawInputMouseHandler( IntPtr hWnd )
        {
            DragActivated = false;

            _configuredMouseButton = Config.MouseButton;
            _xbutton1Scancode = Config.ScancodeForMouseXButton1;
            _xbutton2Scancode = Config.ScancodeForMouseXButton2;

            _mouseButtons = new bool[5];
            _dx = _dy = 0;

            _onMouseButton = OnMouseButton;
            _onMouseMove = OnMouseMove;
            _onMouseWheel = OnMouseWheel;

            const ushort usagePage = 0x0001;//HID_USAGE_PAGE_GENERIC
            const ushort usageMouse = 0x0002;//HID_USAGE_GENERIC_MOUSE
            Win32.RawInput.RegisterWindowForRawInput(hWnd, usagePage, usageMouse);
        }

        //----------------------------------------
        public void ProcessRawInputMessage( IntPtr hRawInput, out int dx, out int dy )
        {
            dx = dy = 0;

            // Decode and track the mouse movement, in hardware-device coords.
            bool isMouse = Win32.RawInput.DecodeMouseEvent(hRawInput, _onMouseButton, _onMouseMove, _onMouseWheel);
            System.Diagnostics.Debug.Assert(isMouse);

            dx = _dx;
            dy = _dy;
            return;
        }

        //--------------------------------------------------------------
        // Callbacks

        //----------------------------------------
        void OnMouseButton( int buttonId, bool isPressed )
        {
            // Honor SM_SWAPBUTTON setting.
            if (Win32.RawInput.ButtonsSwapped)
            {
                if (buttonId == 0) buttonId = 1;
                else if (buttonId == 1) buttonId = 0;
            }

            Console.WriteLine("Mouse button {0} changed to {1}", buttonId, isPressed);

            // Update array of button states.
            if (0 <= buttonId && buttonId <= 4)
                _mouseButtons[buttonId] = isPressed;

            // Update master drag state.
            if (buttonId == _configuredMouseButton)
                DragActivated = isPressed;

            // On middle-click: throttle is reset to 50% 
            if (buttonId == 2)
                ThrottleAbsolutePosTracker.TrackMiddleClick(isPressed, Environment.TickCount);

            // Bonus feature: map mouse x-buttons to keyboard scancodes.
            if (_xbutton1Scancode.HasValue && buttonId == 3)
            {
                Console.WriteLine("XB1 key [{0:X}]: {1}", _xbutton1Scancode.Value, isPressed);
                Win32.SendInput.SendKeystroke(_xbutton1Scancode.Value, isPressed);
            }
            if (_xbutton2Scancode.HasValue && buttonId == 4)
            {
                Console.WriteLine("XB2 key [{0:X}]: {1}", _xbutton2Scancode.Value, isPressed);
                Win32.SendInput.SendKeystroke(_xbutton2Scancode.Value, isPressed);
            }

            return;
        }

        //----------------------------------------
        void OnMouseMove( int deltaX, int deltaY )
        {
            _dx = deltaX;
            _dy = deltaY;

            return;
        }

        //----------------------------------------
        void OnMouseWheel( int wheelDelta )
        {
		    // Divide by WHEEL_DELTA. NB: in Falcon BMS 4.35 throttle is directionally reverse of mousewheel.
            int throttleDelta = -1 * wheelDelta/120;
            ThrottleAbsolutePosTracker.TrackDelta(throttleDelta);

            return;
        }

    }
}
