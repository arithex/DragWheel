/*
 * DragWheel.exe - Main app entrypoint and callbacks
 * 
 * Listen for raw mouse left-drag inputs; synthesize mousewheel movements in response.
 * 
 * Useful for controlling FOV/zoom or throttle, or any other game function that can be mapped to 
 * mousewheel.
 * 
 * Adjust sensitivity (mouse DPI) or change the button used, in DragWheel.exe.config
 * 
 */
using System;

namespace DragWheel
{
    class Program
    {
        static IntPtr s_msgWndForMouse = IntPtr.Zero;
        static IntPtr s_msgWndForStick = IntPtr.Zero;

        static RawInputMouseHandler s_mouseInputHandler = null;
        static RawInputJoystickHandler s_stickInputHandler = null;

        //--------------------------------------------------------------
        // Main entrypoint

        //----------------------------------------
        [STAThread]
        static void Main( string[] args )
        {
            try
            {
#if DEBUG
                TestImpl();
#endif
                MainImpl(args);
            }
            catch ( Exception ex )
            {
                Console.Error.WriteLine(ex.ToString());
                System.Diagnostics.Debug.Print(ex.ToString());
            }
            return;
        }

        //----------------------------------------
        static void MainImpl( string[] args )
        {
            if (Config.MouseButton == null && Config.JoystickButton == null)
                throw new ApplicationException("Nothing to do -- must configure one of MouseButton or JoystickButton");

            // Create hidden HWNDs to subscribe to Raw Input (WM_INPUT) events. We create two
            // separate windows, one for mouse and another for joystick.  It is far easier 
            // and more efficient than de-multiplexing the events later.
            Win32.MessageWindow.InitWindowClass(_WndProc);

            s_msgWndForMouse = Win32.MessageWindow.CreateMessageWindow();
            s_mouseInputHandler = new RawInputMouseHandler(s_msgWndForMouse);

            if (Config.JoystickButton != null)
            {
                s_msgWndForStick = Win32.MessageWindow.CreateMessageWindow();
                s_stickInputHandler = new RawInputJoystickHandler(s_msgWndForStick);
            }

            // Pump messages.
            Win32.MessageWindow.PumpMessages();
            return;
        }

        //--------------------------------------------------------------
        // Raw Input window message handler

        //----------------------------------------
        static int _WndProc( IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam )
        {
            try
            {
                return _WndProcImpl(hWnd, msg, wParam, lParam);
            }
            catch ( Exception ex )
            {
                Console.Error.WriteLine(ex.ToString());
                System.Diagnostics.Debug.Print(ex.ToString());
            }
            return 0;
        }

        //----------------------------------------
        static int _WndProcImpl( IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam )
        {
            switch (msg)
            {
                case 0x0001://WM_CREATE
                    break;

                case 0x00FF://WM_INPUT
                    IntPtr hInput = lParam;

                    if (hWnd == s_msgWndForStick)
                    {
                        s_stickInputHandler.ProcessRawInputMessage(hInput);
                    }
                    else if (hWnd == s_msgWndForMouse)
                    {
                        int dx, dy;
                        s_mouseInputHandler.ProcessRawInputMessage(hInput, out dx, out dy);

                        if (RawInputMouseHandler.DragActivated || RawInputJoystickHandler.DragActivated)
                        {
                            int wheelDelta = MouseDragTracker.TrackDragDeltas(dx, dy);
                            if (wheelDelta != 0)
                            {
                                Console.WriteLine("Sending deltaMouseWheel={0}", wheelDelta);
                                Win32.SendInput.MoveMouseWheel(wheelDelta);
                            }
                        }
                        else
                        {
                            MouseDragTracker.ResetDragDeltas();
                        }
                    }

                    // Per docs, don't call DefWindowProc for background-sink messages.
                    bool backgroundFlag = (wParam != IntPtr.Zero);
                    if (backgroundFlag) return 0;

                    break;

                case 0x0002://WM_DESTROY
                    Win32.MessageWindow.PostQuitMessage(0);
                    break;
            }

            return Win32.MessageWindow.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        //--------------------------------------------------------------
        // Crude unit-testing

        //----------------------------------------
        static void TestImpl( )
        {
            Tests.Config_NullOrEmpty();
            Tests.MouseTracker_Up();
            Tests.MouseTracker_Down();
            return;
        }

    }
}
