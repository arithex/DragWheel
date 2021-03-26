using System;

namespace DragWheel
{
    internal class RawInputMouseHandler
    {
        bool[] _mouseButtons;
        int _configuredMouseButton;

        int _dx, _dy;

        Win32.RawInput.MouseButtonHandler _onMouseButton;
        Win32.RawInput.MouseMoveHandler _onMouseMove;

        //--------------------------------------------------------------
        // Interface

        //----------------------------------------
        public volatile static bool DragActivated;

        //----------------------------------------
        public RawInputMouseHandler( IntPtr hWnd )
        {
            DragActivated = false;

            _mouseButtons = new bool[5];
            _configuredMouseButton = Config.MouseButton.HasValue ? Config.MouseButton.Value : -1;

            _dx = _dy = 0;

            _onMouseButton = OnMouseButton;
            _onMouseMove = OnMouseMove;

            const ushort usagePage = 0x0001;//HID_USAGE_PAGE_GENERIC
            const ushort usageMouse = 0x0002;//HID_USAGE_GENERIC_MOUSE
            Win32.RawInput.RegisterWindowForRawInput(hWnd, usagePage, usageMouse);
        }

        //----------------------------------------
        public void ProcessRawInputMessage( IntPtr hRawInput, out int dx, out int dy )
        {
            dx = dy = 0;

            // Decode and track the mouse movement, in hardware-device coords.
            bool isMouse = Win32.RawInput.DecodeMouseEvent(hRawInput, _onMouseButton, _onMouseMove);
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

            // Update all button states.
            if (0 <= buttonId && buttonId <= 4)
                _mouseButtons[buttonId] = isPressed;

            // Update master drag-state.
            if (buttonId == _configuredMouseButton)
                DragActivated = isPressed;

            //TEMP++
            //if (buttonId == 4 && isPressed)
            //{
            //    Console.WriteLine("Pressing key [L]");
            //    Win32.SendInput.SendKeystroke(0x0026, true);
            //}
            //if (buttonId == 4 && !isPressed)
            //{
            //    Console.WriteLine("Releasing key [L]");
            //    Win32.SendInput.SendKeystroke(0x0026, false);
            //}
            //if (buttonId == 3 && isPressed)
            //{
            //    Console.WriteLine("Pressing key sequence [shift+NumEnter]");
            //    Win32.SendInput.SendKeystroke(0x0036, true);//right-shift
            //    Win32.SendInput.SendKeystroke(0x009C, true, true);//keypad-enter
            //}
            //if (buttonId == 3 && !isPressed)
            //{
            //    Console.WriteLine("Releasing key sequence [shift+NumEnter]");
            //    Win32.SendInput.SendKeystroke(0x009C, false, true);//keypad-enter
            //    Win32.SendInput.SendKeystroke(0x0036, false);//right-shift
            //}
            //--TEMP
            return;
        }

        //----------------------------------------
        void OnMouseMove( int deltaX, int deltaY )
        {
            _dx = deltaX;
            _dy = deltaY;

            return;
        }

    }
}
