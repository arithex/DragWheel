/*
 * Application logic for responding to mouse-move, mouse-wheel, and mouse button clicks.
 */
using System;

namespace DragWheel
{
    internal class RawInputMouseHandler
    {
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
            // Honor SM_SWAPBUTTON setting (tested: BMS 4.35 also honors this).
            if (Win32.RawInput.ButtonsSwapped)
            {
                if (buttonId == 0) buttonId = 1;
                else if (buttonId == 1) buttonId = 0;
            }

            Console.WriteLine("Mouse button {0} changed to {1}", buttonId, isPressed);

            // Update master drag state.
            if (Config.MouseButton.HasValue && (buttonId == Config.MouseButton.Value))
                DragActivated = isPressed;

            // On middle-click: throttle is reset to 50% 
            if (buttonId == 2)
                ThrottleAbsolutePosTracker.TrackMiddleClick(isPressed, Environment.TickCount);

            // Bonus feature: map mouse x-buttons to keyboard scancodes.
            if (Config.ScancodeForMouseWheelButton.HasValue && buttonId == 2)
            {
                Console.WriteLine("WB key [{0:X}]: {1}", Config.ScancodeForMouseWheelButton.Value, isPressed);
                Win32.SendInput.SendKeystroke(Config.ScancodeForMouseWheelButton.Value, isPressed);
            }
            if (Config.ScancodeForMouseXButton1.HasValue && buttonId == 3)
            {
                Console.WriteLine("XB1 key [{0:X}]: {1}", Config.ScancodeForMouseXButton1.Value, isPressed);
                Win32.SendInput.SendKeystroke(Config.ScancodeForMouseXButton1.Value, isPressed);
            }
            if (Config.ScancodeForMouseXButton2.HasValue && buttonId == 4)
            {
                Console.WriteLine("XB2 key [{0:X}]: {1}", Config.ScancodeForMouseXButton2.Value, isPressed);
                Win32.SendInput.SendKeystroke(Config.ScancodeForMouseXButton2.Value, isPressed);
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
