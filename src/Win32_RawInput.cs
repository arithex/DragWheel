using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Win32
{
    internal class RawInput
    {
        //--------------------------------------------------------------
        // Managed wrappers

        //----------------------------------------
        public static void RegisterWindowForRawInput( IntPtr hWnd, ushort usagePage, ushort usageId )
        {
            _Interop_User32.RawInputDevice[] rids = { new _Interop_User32.RawInputDevice() };
            rids[0].usagePage = usagePage;
            rids[0].usageId = usageId;
            rids[0].flags = _Interop_User32.RIDEV_INPUTSINK; //collect input in background
            rids[0].hwndTarget = hWnd;

            if (!_Interop_User32.RegisterRawInputDevices(rids, rids.Length, _Interop_User32.RawInputDevice.MarshalSize))
                throw new Win32Exception();

            return;
        }

        //----------------------------------------
        public static bool ButtonsSwapped
        {
            get
            {
                return (0 != _Interop_User32.GetSystemMetrics(_Interop_User32.SM_SWAPBUTTON));
            }
        }

        //----------------------------------------
        public delegate void MouseButtonHandler( int buttonId, bool isPressed );
        public delegate void MouseMoveHandler( int deltaX, int deltaY );

        //----------------------------------------
        public static bool DecodeMouseEvent( IntPtr hRawInput, MouseButtonHandler onMouseButton, MouseMoveHandler onMouseMove )
        {
            // Crack raw-input to retrieve device handle and data-buffer; ensure expected HID type.
            _Interop_User32.RawInputHeaderAndRawMouse riHeaderAndMouse;
            _GetRawInputData_FixedHeaderAndMouseData(hRawInput, out riHeaderAndMouse);

            uint typeEnum = riHeaderAndMouse.header.dwType;
            if (typeEnum != _Interop_User32.RIM_TYPEMOUSE)
                return false;

            // Unmarshal the raw data for the mouse event.
            ushort mouseFlags = riHeaderAndMouse.mouse.usFlags;
            ushort mouseButtonFlags = riHeaderAndMouse.mouse.usButtonFlags;
            //ushort mouseButtonData = riHeaderAndMouse.mouse.usButtonData;//wheelDelta

            // Process mouse button state-changes (for 5 standardized buttons).  NOTE these are physical hardware
            // button positions -- the SM_SWAPBUTTON user preference must be honored later, if desired.
            if (0 != (mouseButtonFlags & _Interop_User32.RI_MOUSE_LEFT_BUTTON_DOWN))
                onMouseButton(0, true);
            else if (0 != (mouseButtonFlags & _Interop_User32.RI_MOUSE_LEFT_BUTTON_UP))
                onMouseButton(0, false);

            if (0 != (mouseButtonFlags & _Interop_User32.RI_MOUSE_RIGHT_BUTTON_DOWN))
                onMouseButton(1, true);
            else if (0 != (mouseButtonFlags & _Interop_User32.RI_MOUSE_RIGHT_BUTTON_UP))
                onMouseButton(1, false);

            if (0 != (mouseButtonFlags & _Interop_User32.RI_MOUSE_MIDDLE_BUTTON_DOWN))
                onMouseButton(2, true);
            else if (0 != (mouseButtonFlags & _Interop_User32.RI_MOUSE_MIDDLE_BUTTON_UP))
                onMouseButton(2, false);

            if (0 != (mouseButtonFlags & _Interop_User32.RI_MOUSE_BUTTON_4_DOWN))
                onMouseButton(3, true);
            else if (0 != (mouseButtonFlags & _Interop_User32.RI_MOUSE_BUTTON_4_UP))
                onMouseButton(3, false);

            if (0 != (mouseButtonFlags & _Interop_User32.RI_MOUSE_BUTTON_5_DOWN))
                onMouseButton(4, true);
            else if (0 != (mouseButtonFlags & _Interop_User32.RI_MOUSE_BUTTON_5_UP))
                onMouseButton(4, false);

            // Process directional mouse movement..
            int mouseLastX = riHeaderAndMouse.mouse.lLastX;
            int mouseLastY = riHeaderAndMouse.mouse.lLastY;

            // ..but ignore any absolute-positioning events.
            if ((mouseFlags & _Interop_User32.MOUSE_MOVE_ABSOLUTE) != _Interop_User32.MOUSE_MOVE_ABSOLUTE)
                if (mouseLastX != 0 || mouseLastY != 0)
                    onMouseMove(mouseLastX, mouseLastY);

            return true;
        }

        //----------------------------------------
        public delegate void JoystickButtonHandler( int buttonId, bool isPressed );

        //----------------------------------------
        public static bool DecodeJoystickButtonEvent( IntPtr hRawInput, JoystickButtonHandler onJoystickButton )
        {
            // Crack raw-input to retrieve device handle and data-buffer; ensure expected HID type.
            _Interop_User32.RawInputHeaderAndRawHid riHeaderAndHid;
            IntPtr refRawInputHeaderBlock = _GetRawInputData_CachedHeaderAndBuffer(hRawInput, out riHeaderAndHid);

            uint typeEnum = riHeaderAndHid.header.dwType;
            if (typeEnum != _Interop_User32.RIM_TYPEHID)
                return false;

            // Obtain the "preparse" data for use with subsequent HID functions.
            IntPtr hDevice = riHeaderAndHid.header.hDevice;
            IntPtr refPreparsedHidBlock = _GetRawInputDeviceInfo_CachedPreparsedData(hDevice);

            // Get the max number of buttons, and allocate buffer accordingly. NOTE the confusing change 
            // of context.. we asked to receive events for "page=Desktop,usage=Joystick".. but when we 
            // ask for info on buttons we must switch to ask "page=Buttons" and the "usage" is then the
            // 1-based index of the button, on the device in question.
            uint numButtons = _Interop_Hid.HidP_MaxUsageListLength(
                _Interop_Hid.HidP_Input,
                _Interop_Hid.HID_USAGE_PAGE_BUTTON,
                refPreparsedHidBlock
            );
            if (numButtons == 0)
                throw new Win32Exception("HidP_MaxUsageListLength return 0");

            ushort[] usageList = new ushort[numButtons];

            // To fetch the actual data for the axis/buttons, we must use ugly untyped pointer-arithmetic 
            // on the buffer we got from GetRawInputData.  The offset to the start of the HID report data
            // begins at the tail end of the RAWHID structure.
            IntPtr refRawDataBuffer = refRawInputHeaderBlock + Marshal.SizeOf<_Interop_User32.RawInputHeaderAndRawHid>();
            {
                uint itemCount = riHeaderAndHid.hid.dwCount;
                uint itemSize = riHeaderAndHid.hid.dwSizeHid;

                int status = _Interop_Hid.HidP_GetUsages(
                    _Interop_Hid.HidP_Input,
                    _Interop_Hid.HID_USAGE_PAGE_BUTTON,
                    0,
                    usageList, ref numButtons, 
                    refPreparsedHidBlock,
                    refRawDataBuffer, (itemSize*itemCount)
                );
                if (status != _Interop_Hid.HIDP_STATUS_SUCCESS)
                    throw new Win32Exception("HidP_GetUsages returned 0x"+status.ToString("X8"));
            }

            // Track changes in button state; adjust from 1-based to 0-based indexing, and issue callbacks.
            _IssueCallbacksForChangedButtons(usageList, numButtons, onJoystickButton);
            return true;
        }

        //--------------------------------------------------------------
        // Managed helpers

        //----------------------------------------
        static void _GetRawInputData_FixedHeaderAndMouseData( IntPtr hRawInput, 
            out _Interop_User32.RawInputHeaderAndRawMouse riHeaderAndMouse )
        {
            riHeaderAndMouse = new _Interop_User32.RawInputHeaderAndRawMouse();
            riHeaderAndMouse.header.dwType = UInt32.MaxValue;

            // The buffer-size for mouse event data is known at compile-time.
            int fixedBufferSize = Marshal.SizeOf<_Interop_User32.RawInputHeaderAndRawMouse>();

            if (s_cacheMouseBuffer == IntPtr.Zero)
                s_cacheMouseBuffer = Marshal.AllocHGlobal(fixedBufferSize);

            int status = _Interop_User32.GetRawInputData(hRawInput, 
                _Interop_User32.RID_INPUT,//fetch header-and-data
                s_cacheMouseBuffer, ref fixedBufferSize, 
                _Interop_User32.RawInputHeader.MarshalSize
            );
            if (status == -1)
                throw new Win32Exception();

            // Unpack the header and data struct from the buffer.. 
            riHeaderAndMouse = Marshal.PtrToStructure<_Interop_User32.RawInputHeaderAndRawMouse>(s_cacheMouseBuffer);
            return;
        }
        static IntPtr s_cacheMouseBuffer = IntPtr.Zero;

        //----------------------------------------
        static IntPtr _GetRawInputData_CachedHeaderAndBuffer( IntPtr hRawInput, out _Interop_User32.RawInputHeaderAndRawHid riHeaderAndHid )
        {
            riHeaderAndHid = new _Interop_User32.RawInputHeaderAndRawHid();
            riHeaderAndHid.header.dwType = UInt32.MaxValue;

            // Alloc the necessary buffer if first time through. This buffer appears to be invariant size, but unlike
            // mouse and keybd, is not knowable at compile-time (it depends on the details of the HID device).
            if (s_cacheHidHeaderAndReportBuffer == IntPtr.Zero)
            {
                s_sizeofHidHeaderAndReportBuffer = 0;

                // Query the buffer size.
                int status = _Interop_User32.GetRawInputData(hRawInput,
                    _Interop_User32.RID_INPUT,//fetch header-and-data
                    IntPtr.Zero, ref s_sizeofHidHeaderAndReportBuffer,
                    _Interop_User32.RawInputHeader.MarshalSize
                );
                if (status != 0)
                    throw new Win32Exception();

                // Allocate and cache the buffer.
                s_cacheHidHeaderAndReportBuffer = Marshal.AllocHGlobal(s_sizeofHidHeaderAndReportBuffer);
            }

            // Fetch the data; verify size and failfast if not as expected.
            System.Diagnostics.Debug.Assert(s_sizeofHidHeaderAndReportBuffer != 0);
            if (true)
            {
                int bufferSize = s_sizeofHidHeaderAndReportBuffer;

                int status = _Interop_User32.GetRawInputData(hRawInput, 
                    _Interop_User32.RID_INPUT,//fetch header-and-data
                    s_cacheHidHeaderAndReportBuffer, ref bufferSize, 
                    _Interop_User32.RawInputHeader.MarshalSize
                );
                if (status == -1) throw new Win32Exception();
                if (bufferSize != s_sizeofHidHeaderAndReportBuffer) 
                    throw new Win32Exception("Buffer size for GetRawInputData changed unexpectedly.");
            }

            // Unpack the header struct from the buffer.
            riHeaderAndHid = Marshal.PtrToStructure<_Interop_User32.RawInputHeaderAndRawHid>(s_cacheHidHeaderAndReportBuffer);
            return s_cacheHidHeaderAndReportBuffer;
        }
        static IntPtr s_cacheHidHeaderAndReportBuffer = IntPtr.Zero;
        static int s_sizeofHidHeaderAndReportBuffer = 0;

        //----------------------------------------
        static IntPtr _GetRawInputDeviceInfo_CachedPreparsedData( IntPtr hDevice )
        {
            // Alloc the necessary buffer if first time through. This buffer appears to be invariant size, but is
            // not knowable at compile-time (it depends on the capabilities of the HID device).
            if (s_cacheHidPreparsedBuffer == IntPtr.Zero)
            {
                s_sizeofHidPreparsedBuffer = 0;

                // Query the buffer size.
                int status = _Interop_User32.GetRawInputDeviceInfo(hDevice, _Interop_User32.RIDI_PREPARSEDDATA,
                    IntPtr.Zero, ref s_sizeofHidPreparsedBuffer
                );
                if (status != 0)
                    throw new Win32Exception();

                // Allocate and cache the buffer.
                s_cacheHidPreparsedBuffer = Marshal.AllocHGlobal(s_sizeofHidPreparsedBuffer);
            }

            // Fetch the data; verify size and failfast if not as expected.
            System.Diagnostics.Debug.Assert(s_sizeofHidHeaderAndReportBuffer != 0);
            if (true)
            {
                int bufferSize = s_sizeofHidPreparsedBuffer;

                int status = _Interop_User32.GetRawInputDeviceInfo(hDevice, _Interop_User32.RIDI_PREPARSEDDATA, 
                    s_cacheHidPreparsedBuffer, ref bufferSize
                );
                if (status == -1) throw new Win32Exception();
                if (bufferSize != s_sizeofHidPreparsedBuffer)
                    throw new Win32Exception("Buffer size for GetRawInputDeviceInfo changed unexpectedly.");
            }
            return s_cacheHidPreparsedBuffer;
        }
        static IntPtr s_cacheHidPreparsedBuffer = IntPtr.Zero;
        static int s_sizeofHidPreparsedBuffer = 0;

        //----------------------------------------
        static void _IssueCallbacksForChangedButtons( ushort[] usageList, uint numButtons, 
            JoystickButtonHandler onJoystickButton )
        {
            uint buttonBitfield = 0u;
            for (int i = 0; i < numButtons; ++i)
            {
                int id = (usageList[i] - 1);//HID button usages are 1-based indices, we want 0-based
                buttonBitfield |= (1u << id);
            }
            uint buttonBitfieldXorDeltas = (buttonBitfield ^ s_buttonBitfieldPrevious);
            for (ushort i = 0; i < 32; ++i)
            {
                bool bChanged = ((buttonBitfieldXorDeltas & (1u << i)) != 0u);
                bool bPressed = ((buttonBitfield & (1u << i)) != 0u);
                if (bChanged)
                    onJoystickButton((int)i, bPressed);
            }
            s_buttonBitfieldPrevious = buttonBitfield;
            return;
        }
        static uint s_buttonBitfieldPrevious = 0u;


        //--------------------------------------------------------------
        // Interop declarations

        //----------------------------------------
        // User32 Raw Input API

        static class _Interop_User32
        {
            internal const int SM_SWAPBUTTON = 23;

            internal const uint RIDEV_INPUTSINK = 0x00000100;

            internal const uint RID_INPUT = 0x10000003;
            internal const uint RIM_TYPEMOUSE = 0;
            internal const uint RIM_TYPEHID = 2;

            internal const uint RIDI_PREPARSEDDATA = 0x20000005;

            internal const ushort MOUSE_MOVE_ABSOLUTE = 0x0001;

            internal const ushort RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001;
            internal const ushort RI_MOUSE_LEFT_BUTTON_UP = 0x0002;
            internal const ushort RI_MOUSE_RIGHT_BUTTON_DOWN = 0x0004;
            internal const ushort RI_MOUSE_RIGHT_BUTTON_UP = 0x0008;
            internal const ushort RI_MOUSE_MIDDLE_BUTTON_DOWN = 0x0010;
            internal const ushort RI_MOUSE_MIDDLE_BUTTON_UP = 0x0020;
            internal const ushort RI_MOUSE_BUTTON_4_DOWN = 0x0040;
            internal const ushort RI_MOUSE_BUTTON_4_UP = 0x0080;
            internal const ushort RI_MOUSE_BUTTON_5_DOWN = 0x0100;
            internal const ushort RI_MOUSE_BUTTON_5_UP = 0x0200;

            [DllImport("User32.dll", SetLastError = true)]
            internal static extern int GetSystemMetrics( int nIndex );

            [DllImport("User32.dll", SetLastError = true)]
            internal static extern bool RegisterRawInputDevices(
                [In, MarshalAs(UnmanagedType.LPArray)] RawInputDevice[] pRawInputDevices,
                int numDevices,
                int cbSize
            );

            [DllImport("User32.dll", SetLastError = true)]
            internal static extern int GetRawInputData(
                IntPtr hRawInput,
                uint uiCommand,
                IntPtr pData,//nb: variable-length array for HID report data -- will require ugly pointer-arithmetic :(
                [In, Out] ref int cbSize,
                int cbSizeHeader
            );

            [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "GetRawInputDeviceInfo")]
            internal static extern int GetRawInputDeviceInfo(
                IntPtr hDevice,
                uint uiCommand,
                IntPtr pData,
                [In, Out] ref int pcbSize
            );

            [StructLayout(LayoutKind.Sequential)]
            internal struct RawInputDevice
            {
                internal ushort usagePage;
                internal ushort usageId;
                internal uint flags;
                internal IntPtr hwndTarget;

                internal static int MarshalSize = Marshal.SizeOf<RawInputDevice>();
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct RawInputHeader
            {
                internal uint dwType;//mouse, keybd, or hid/other
                internal uint dwSize;
                internal IntPtr hDevice;
                internal IntPtr wParam;//nonzero indicates background-sinked message

                internal static int MarshalSize = Marshal.SizeOf<RawInputHeader>();
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct RawInputHeaderAndRawMouse
            {
                internal RawInputHeader header; //dwType==RIM_TYPEMOUSE
                internal RawMouse mouse;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct RawMouse
            {
                internal ushort usFlags;
                private ushort _reserved_padding;
                internal ushort usButtonFlags;
                internal ushort usButtonData;
                private uint ulRawButtons;//unused, always zero for most hardware
                internal int lLastX;
                internal int lLastY;
                internal uint ulExtraInformation;
            }
            //TBD: struct RawInputHeaderAndRawKeybd
            //TBD: struct RawKeyboard

            [StructLayout(LayoutKind.Sequential)]
            internal struct RawInputHeaderAndRawHid
            {
                internal RawInputHeader header; //dwType==RIM_TYPEHID
                internal RawHid hid;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct RawHid
            {
                internal uint dwSizeHid;
                internal uint dwCount;

                //[MarshalAs(UnmanagedType.ByValArray, SizeParamIndex=???)]
                //byte[] bRawData;
                //NB: Because of the difficulties marshalling the variable-length embedded array, 
                // which is opaque (untyped) binary data anyway, the bRawData must be referenced
                // via unmanaged pointer-arithmetic, eg: 
                //   pRawInput + sizeof(RawInputHeader) + sizeof(RawHid)
            }
        }

        //----------------------------------------
        // HID "preparsed" data API

        static class _Interop_Hid
        {
            internal const int HIDP_STATUS_SUCCESS = 0x00110000;

            internal const int HidP_Input = 0;
            internal const ushort HID_USAGE_PAGE_BUTTON = 9;

            [DllImport("Hid.dll", SetLastError = false)]
            internal static extern uint HidP_MaxUsageListLength(//aka "GetMaxCountOfButtons"
                uint reportType, ushort usagePage,
                IntPtr pPreparsedData
            );

            [DllImport("Hid.dll", SetLastError = false)]
            internal static extern int HidP_GetUsages(//aka "GetButtonsCurrentlyPressedInThisHidReport"
                uint reportType, ushort usagePage, ushort linkCollection, 
                [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] ushort[] usageList,
                [In, Out] ref uint usageLength,
                IntPtr pPreparsedData,
                IntPtr refHidReportBuffer,
                ulong reportLength
            );
         }

    }
}