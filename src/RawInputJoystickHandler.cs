/*
 * Application logic for responding to joystick button-presses.
 */
using System;

namespace DragWheel
{
    internal class RawInputJoystickHandler
    {
        Win32.RawInput.JoystickButtonHandler _onStickButton;

        //--------------------------------------------------------------
        // Interface

        //----------------------------------------
        public volatile static bool DragActivated;

        //----------------------------------------
        public RawInputJoystickHandler( IntPtr hWnd )
        {
            DragActivated = false;

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
        void OnStickButton( ushort vendorId, ushort productId, ushort buttonId, bool isPressed )
        {
            Console.WriteLine("Device pidvid {1}{0} button {2} changed to {3}", vendorId.ToString("X4"), productId.ToString("X4"), buttonId, isPressed);

            if (Config.JoystickButton == null)
                return;

            string pidvidId = String.Format(@"{1:X4},{0:X4},{2}", vendorId, productId, buttonId);

            // Update master drag-state.
            if (0 == String.CompareOrdinal(pidvidId, Config.JoystickButton))
                DragActivated = isPressed;

            return;
        }

    }
}
