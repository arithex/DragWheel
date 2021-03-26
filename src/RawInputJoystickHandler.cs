using System;

namespace DragWheel
{
    internal class RawInputJoystickHandler
    {
        bool[] _stickButtons;
        int _configuredStickButton;

        Win32.RawInput.JoystickButtonHandler _onStickButton;

        //--------------------------------------------------------------
        // Interface

        //----------------------------------------
        public volatile static bool DragActivated;

        //----------------------------------------
        public RawInputJoystickHandler( IntPtr hWnd )
        {
            DragActivated = false;

            _stickButtons = new bool[32];
            _configuredStickButton = Config.JoystickButton.Value;

            _onStickButton = OnStickButton;

            const ushort usagePage = 0x0001;//HID_USAGE_PAGE_GENERIC
            const ushort usageJoystick= 0x0004;//HID_USAGE_GENERIC_JOYSTICK
            Win32.RawInput.RegisterWindowForRawInput(hWnd, usagePage, usageJoystick);
        }

        //----------------------------------------
        public void ProcessRawInputMessage( IntPtr hRawInput )
        {
            // Decode and track the hardware button-state report.
            bool isButton = Win32.RawInput.DecodeJoystickButtonEvent(hRawInput, _onStickButton);
            System.Diagnostics.Debug.Assert(isButton);

            return;
        }

        //--------------------------------------------------------------
        // Callbacks

        //----------------------------------------
        void OnStickButton( int buttonId, bool isPressed )
        {
            Console.WriteLine("Stick button {0} changed to {1}", buttonId, isPressed);

            // Update all button states.
            if (0 <= buttonId && buttonId <= 31)
                _stickButtons[buttonId] = isPressed;

            // Update master drag-state.
            if (buttonId == _configuredStickButton)
                DragActivated = isPressed;

            return;
        }

    }
}
