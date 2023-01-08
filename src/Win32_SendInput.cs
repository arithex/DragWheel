/*
 * Interop wrapper for programmatically generating keystrokes, and manipulating mouse.
 * 
 * TODO: test with multi-monitor.. coords relative to "primary" desktop? virtual desktop-space?
 */
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
        public static void MoveMouseAbsolute( int x, int y )
        {
            //?: multiple monitors? virtual desktop?
            _Interop_User32.InputStruct[] inputs = new _Interop_User32.InputStruct[] {
                _PrepMousePosition(x, y, virtualCoords:true ) //?
            };

            int status = _Interop_User32.SendInput(inputs.Length, inputs, _Interop_User32.InputStruct.MarshalSize);
            if (status != inputs.Length)
                throw new Win32Exception();

            return;
        }

        //----------------------------------------
        public static void MoveMouseRelative( int dx, int dy )
        {
            _Interop_User32.InputStruct[] inputs = new _Interop_User32.InputStruct[] {
                _PrepMouseMovement(dx, dy, virtualCoords:true ) //?
            };

            int status = _Interop_User32.SendInput(inputs.Length, inputs, _Interop_User32.InputStruct.MarshalSize);
            if (status != inputs.Length)
                throw new Win32Exception();

            return;
        }

        //----------------------------------------
        public static void SendMouseButton( int mouseButton, bool buttonDown )
        {
            //?: swap mouse buttons?
            _Interop_User32.InputStruct[] inputs = new _Interop_User32.InputStruct[] {
                _PrepMouseButton(mouseButton, buttonDown)
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

        static _Interop_User32.InputStruct _PrepMousePosition( int x, int y, bool virtualCoords )
        {
            _Interop_User32.InputStruct inputStruct = new _Interop_User32.InputStruct();
            inputStruct.type = _Interop_User32.INPUT_MOUSE;
            inputStruct._union.mouseInput.dx = x;
            inputStruct._union.mouseInput.dy = y;
            inputStruct._union.mouseInput.flags = (_Interop_User32.MOUSEEVENTF_MOVE | _Interop_User32.MOUSEEVENTF_ABSOLUTE);

            if (virtualCoords)
                inputStruct._union.mouseInput.flags |= _Interop_User32.MOUSEEVENTF_VIRTUALDESK;

            return inputStruct;
        }

        static _Interop_User32.InputStruct _PrepMouseMovement( int dx, int dy, bool virtualCoords )
        {
            _Interop_User32.InputStruct inputStruct = new _Interop_User32.InputStruct();
            inputStruct.type = _Interop_User32.INPUT_MOUSE;
            inputStruct._union.mouseInput.dx = dx;
            inputStruct._union.mouseInput.dy = dy;
            inputStruct._union.mouseInput.flags = (_Interop_User32.MOUSEEVENTF_MOVE);

            if (virtualCoords)
                inputStruct._union.mouseInput.flags |= _Interop_User32.MOUSEEVENTF_VIRTUALDESK;

            return inputStruct;
        }

        static _Interop_User32.InputStruct _PrepMouseButton( int mouseButton, bool buttonDown )
        {
            _Interop_User32.InputStruct inputStruct = new _Interop_User32.InputStruct();
            inputStruct.type = _Interop_User32.INPUT_MOUSE;
            switch (mouseButton)
            {
                case 0: //left
                    inputStruct._union.mouseInput.flags = buttonDown ? _Interop_User32.MOUSEEVENTF_LEFTDOWN : _Interop_User32.MOUSEEVENTF_LEFTUP;
                    break;
                case 1: //right
                    inputStruct._union.mouseInput.flags = buttonDown ? _Interop_User32.MOUSEEVENTF_RIGHTDOWN : _Interop_User32.MOUSEEVENTF_RIGHTUP;
                    break;
                case 2: //middle
                    inputStruct._union.mouseInput.flags = buttonDown ? _Interop_User32.MOUSEEVENTF_MIDDLEDOWN : _Interop_User32.MOUSEEVENTF_MIDDLEUP;
                    break;
                default:
                    throw new ArgumentException();
            }

            return inputStruct;
        }

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

            internal const ushort MOUSEEVENTF_MOVE = 0x0001;
            internal const ushort MOUSEEVENTF_LEFTDOWN = 0x0002;
            internal const ushort MOUSEEVENTF_LEFTUP = 0x0004;
            internal const ushort MOUSEEVENTF_RIGHTDOWN = 0x0008;
            internal const ushort MOUSEEVENTF_RIGHTUP = 0x0010;
            internal const ushort MOUSEEVENTF_MIDDLEDOWN = 0x0020;
            internal const ushort MOUSEEVENTF_MIDDLEUP = 0x0040;
            internal const ushort MOUSEEVENTF_WHEEL = 0x0800;

            internal const ushort MOUSEEVENTF_VIRTUALDESK = 0x4000;
            internal const ushort MOUSEEVENTF_ABSOLUTE = 0x8000;

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

                //NB: This option seems unimplemented? And smaller size than others, so irrelevant as part of the union.
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