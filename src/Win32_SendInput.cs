using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Win32
{
    internal class SendInput
    {
        //--------------------------------------------------------------
        // Managed wrappers

        //----------------------------------------
        public static void SendKeystroke( ushort scanCode, bool keyDown, bool isExtended=false )
        {
            _Interop_User32.InputStruct[] inputs = new _Interop_User32.InputStruct[] {
                _PrepKeybdScanCode(scanCode, keyDown, isExtended)
            };

            int status = _Interop_User32.SendInput(inputs.Length, inputs, _Interop_User32.InputStruct.MarshalSize);
            if (status != inputs.Length)
                throw new Win32Exception();

            return;
        }

        //----------------------------------------
        public static void MoveMouseWheel( int wheelDelta )
        {
            _Interop_User32.InputStruct[] inputs = new _Interop_User32.InputStruct[] {
                _PrepMouseWheelDelta(wheelDelta)
            };

            int status = _Interop_User32.SendInput(inputs.Length, inputs, _Interop_User32.InputStruct.MarshalSize);
            if (status != inputs.Length) 
                throw new Win32Exception();

            return;
        }

        //--------------------------------------------------------------
        // Managed helpers

        static _Interop_User32.InputStruct _PrepMouseWheelDelta( int wheelDelta )
        {
            _Interop_User32.InputStruct inputStruct = new _Interop_User32.InputStruct();
            inputStruct.type = _Interop_User32.INPUT_MOUSE;
            inputStruct._union.mouseInput.flags = _Interop_User32.MOUSEEVENTF_WHEEL;
            inputStruct._union.mouseInput.mouseData = wheelDelta;

            return inputStruct;
        }

        static _Interop_User32.InputStruct _PrepKeybdScanCode( ushort scanCode, bool keyDown, bool isExtended=false )
        {
            uint flags = _Interop_User32.KEYEVENTF_SCANCODE;
            flags |= (keyDown ? 0u : _Interop_User32.KEYEVENTF_KEYUP);
            flags |= (isExtended ? _Interop_User32.KEYEVENTF_EXTENDEDKEY : 0u);

            _Interop_User32.InputStruct inputStruct = new _Interop_User32.InputStruct();
            inputStruct.type = _Interop_User32.INPUT_KEYBOARD;
            inputStruct._union.keybdInput.scanCode = scanCode;
            inputStruct._union.keybdInput.flags = flags;

            return inputStruct;
        }

        //--------------------------------------------------------------
        // Interop declarations

        //----------------------------------------
        // User32 SendInput API

        static class _Interop_User32
        {
            internal const uint INPUT_MOUSE = 0;
            internal const uint INPUT_KEYBOARD = 1;

            internal const ushort MOUSEEVENTF_WHEEL = 0x0800;

            internal const ushort KEYEVENTF_SCANCODE = 0x0008;
            internal const ushort KEYEVENTF_KEYUP = 0x0002;
            internal const ushort KEYEVENTF_EXTENDEDKEY = 0x0001;

            [DllImport("User32.dll", SetLastError = true)]
            internal static extern int SendInput(
                int nInputs,
                [MarshalAs(UnmanagedType.LPArray)][In] InputStruct[] pInputs,
                int cbSize
            );

            [StructLayout(LayoutKind.Sequential)]
            internal struct InputStruct
            {
                internal static readonly int MarshalSize = Marshal.SizeOf<InputStruct>();

                internal uint type;
                internal InputStruct_union _union;
            }
            [StructLayout(LayoutKind.Explicit)]
            internal struct InputStruct_union
            {
                [FieldOffset(0)]
                internal MouseInputStruct mouseInput;
                [FieldOffset(0)]
                internal KeybdInputStruct keybdInput;

                //NB: This option seems unimplemented? And smaller size than others, so irrelevant.
                //[FieldOffset(0)]
                //internal HARDWAREINPUT hi;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct MouseInputStruct
            {
                internal int dx;
                internal int dy;
                internal int mouseData;
                internal uint flags;
                internal uint time;
                internal IntPtr extraInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct KeybdInputStruct
            {
                internal ushort virtualKeyCode;
                internal ushort scanCode;
                internal uint flags;
                internal int time;
                internal IntPtr extraInfo;
            }
        }

    }
}