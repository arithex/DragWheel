using System;
using System.Configuration;
using System.Diagnostics;

namespace DragWheel
{
    internal static class Config
    {
        //--------------------------------------------------------------
        // Settings

        //----------------------------------------
        public static int MouseResolution
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["MouseResolution"];

                if (String.IsNullOrEmpty(configValue)) 
                    return defaultMouseResolution;

                return (int)_ParseUInt32(configValue);
            }
        }
        static int defaultMouseResolution = 1200;//dpi

        //----------------------------------------
        public static int? MouseButton
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["MouseButton"];

                if (String.IsNullOrEmpty(configValue))
                    return defaultMouseButton;

                uint value = _ParseUInt32(configValue);
                if (value > 4)
                    throw new ConfigurationErrorsException("MouseButton must be in range [0-4]");

                return (int)value;
            }
        }
        static int? defaultMouseButton = null;//not mouse-activated

        //----------------------------------------
        public static int? JoystickButton
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["JoystickButton"];

                if (String.IsNullOrEmpty(configValue))
                    return defaultJoystickButton;

                uint value = _ParseUInt32(configValue);
                if (value > 31)
                    throw new ConfigurationErrorsException("JoystickButton must be in range [0-31]");

                return (int)value;
            }
        }
        static int? defaultJoystickButton = null;//not stick-activated

        //----------------------------------------
        public static int ThrottleThrow
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["ThrottleThrow"];

                if (String.IsNullOrEmpty(configValue))
                    return defaultThrottleThrow;

                uint value = _ParseUInt32(configValue);
                if (value > 1000)
                    throw new ConfigurationErrorsException("ThrottleThrow expected range: [0-1000]");

                return (int)value;
            }
        }
        static int defaultThrottleThrow = 125;//tested on Falcon BMS 4.35, with mousewheel-sensitivity set to minimum
		// tested on F-16 and F-18 .. TODO: test other planes?

        //----------------------------------------
        public static int ThrottleMaxMIL
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["ThrottleMaxMIL"];

                if (String.IsNullOrEmpty(configValue))
                    return defaultThrottleMaxMIL;

                uint value = _ParseUInt32(configValue);
                if (value > 1000)
                    throw new ConfigurationErrorsException("ThrottleMaxMIL expected range: [0-1000]");

                return (int)value;
            }
        }
        static int defaultThrottleMaxMIL = 83;//tested on Falcon BMS 4.35, with mousewheel-sensitivity set to minimum
		// tested on F-16 and F-18 .. TODO: test other planes?

        //----------------------------------------
        public static string IdleStopSound
        {
            get
            {
                return ConfigurationManager.AppSettings["IdleStopSound"];
            }
        }

        //----------------------------------------
        public static string BurnerDetentUpSound
        {
            get
            {
                return ConfigurationManager.AppSettings["BurnerDetentUpSound"];
            }
        }

        //----------------------------------------
        public static string BurnerDetentDownSound
        {
            get
            {
                return ConfigurationManager.AppSettings["BurnerDetentDownSound"];
            }
        }

        //----------------------------------------
        public static string MaxBurnerSound
        {
            get
            {
                return ConfigurationManager.AppSettings["MaxBurnerSound"];
            }
        }

        //----------------------------------------
        public static byte? ScancodeForMouseWheelButton
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["ScancodeForMouseWheelButton"];

                if (String.IsNullOrEmpty(configValue))
                    return null;

                uint value = _ParseUInt32(configValue);
                if (value > 255)
                    throw new ConfigurationErrorsException("Scancode must be in range [0x00-0xFF]");

                return (byte)value;
            }
        }

        //----------------------------------------
        public static byte? ScancodeForMouseXButton1
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["ScancodeForMouseXButton1"];

                if (String.IsNullOrEmpty(configValue))
                    return null;

                uint value = _ParseUInt32(configValue);
                if (value > 255)
                    throw new ConfigurationErrorsException("Scancode must be in range [0x00-0xFF]");

                return (byte)value;
            }
        }

        //----------------------------------------
        public static byte? ScancodeForMouseXButton2
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["ScancodeForMouseXButton2"];

                if (String.IsNullOrEmpty(configValue))
                    return null;

                uint value = _ParseUInt32(configValue);
                if (value > 255)
                    throw new ConfigurationErrorsException("Scancode must be in range [0x00-0xFF]");

                return (byte)value;
            }
        }

        //--------------------------------------------------------------
        // Helpers

        //----------------------------------------
        static uint _ParseUInt32( string s )
        {
            System.Globalization.NumberStyles style = System.Globalization.NumberStyles.None;
            if (s.StartsWith("0x"))
            {
                style = System.Globalization.NumberStyles.HexNumber;
                s = s.Substring(2);
            }

            uint value = UInt32.Parse(s, style, System.Globalization.CultureInfo.InvariantCulture);
            return value;
        }

    }

    //------------------------------------------------------------------
    internal static partial class Tests
    {
        internal static void Config_NullOrEmpty( )
        {
            string configValue = ConfigurationManager.AppSettings["__NonExistent__"];
            Debug.Assert(String.IsNullOrEmpty(configValue));
        }
    }
}
