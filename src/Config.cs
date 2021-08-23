using System;
using System.Configuration;
using System.Diagnostics;

namespace DragWheel
{
    internal static class Config
    {
        private static System.Collections.Generic.Dictionary<string, object> s_configDefaults =
            new System.Collections.Generic.Dictionary<string, object> {
                { "MouseResolution", 1200 }, //typical mouse DPI
                { "ThrottleThrow", 125 }, //tested on Falcon BMS 4.35, F-16 and F-18, with mousewheel-sensitivity set to minimum
                { "ThrottleMaxMIL", 83 }, //TODO: test other (non-afterburner) planes
            };

        private static System.Collections.Generic.Dictionary<string, object> s_cachedConfig =
            new System.Collections.Generic.Dictionary<string, object>();

        //--------------------------------------------------------------
        // Settings

        //----------------------------------------
        public static int? MouseResolution
        {
            get
            { return (int?)_GetCachedValue("MouseResolution", typeof(int)); }
        }

        //----------------------------------------
        public static ushort? MouseButton
        {
            get
            { return (ushort?)_GetCachedValue("MouseButton", typeof(ushort)); }
        }

        //----------------------------------------
        public static string JoystickButton
        {
            get
            { return (string)_GetCachedValue("JoystickButton"); }
        }

        //----------------------------------------
        public static uint? ThrottleThrow
        {
            get
            { return (uint?)_GetCachedValue("ThrottleThrow", typeof(uint)); }
        }

        //----------------------------------------
        public static uint? ThrottleMaxMIL
        {
            get
            { return (uint?)_GetCachedValue("ThrottleMaxMIL", typeof(uint)); }
        }

        //----------------------------------------
        public static string IdleStopSound
        {
            get
            { return (string)_GetCachedValue("IdleStopSound"); }
        }

        //----------------------------------------
        public static string BurnerDetentUpSound
        {
            get
            { return (string)_GetCachedValue("BurnerDetentUpSound"); }
        }

        //----------------------------------------
        public static string BurnerDetentDownSound
        {
            get
            { return (string)_GetCachedValue("BurnerDetentDownSound"); }
        }

        //----------------------------------------
        public static string MaxBurnerSound
        {
            get
            { return (string)_GetCachedValue("MaxBurnerSound"); }
        }

        //----------------------------------------
        public static byte? ScancodeForMouseWheelButton
        {
            get
            { return (byte?)_GetCachedValue("ScancodeForMouseWheelButton", typeof(byte)); }
        }

        //----------------------------------------
        public static byte? ScancodeForMouseXButton1
        {
            get
            { return (byte?)_GetCachedValue("ScancodeForMouseXButton1", typeof(byte)); }
        }

        //----------------------------------------
        public static byte? ScancodeForMouseXButton2
        {
            get
            { return (byte?)_GetCachedValue("ScancodeForMouseXButton2", typeof(byte)); }
        }

        //--------------------------------------------------------------
        // Helpers

        //----------------------------------------
        static object _GetCachedValue( string keyname, Type type=null )
        {
            if (s_cachedConfig.ContainsKey(keyname))
                return s_cachedConfig[keyname];

            object value = _GetValueFromConfigOrDefault(keyname, type);
            s_cachedConfig[keyname] = value;

            if (value == null) return null;
            return value;
        }

        //----------------------------------------
        static object _GetValueFromConfigOrDefault( string keyname, Type type=null )
        {
            string s = ConfigurationManager.AppSettings[keyname];

            if (String.IsNullOrEmpty(s) && s_configDefaults.ContainsKey(keyname))
                return s_configDefaults[keyname];

            if (String.IsNullOrEmpty(s))
                return null;

            if (type == null || type == typeof(string))
                return s;

            // Crude parsing for parsing valuetypes.
            object value = s;
            if (type == typeof(byte)) value = _ParseByte(s);
            if (type == typeof(ushort)) value = _ParseUShort(s);
            if (type == typeof(uint)) value = _ParseUInt32(s);
            if (type == typeof(int)) value = _ParseInt32(s);

            return value;
        }

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

        //----------------------------------------
        static ushort _ParseUShort( string s )
        {
            uint value = _ParseUInt32(s);
            if (value > 0xFFFF) throw new ArgumentOutOfRangeException();
            return (ushort)value;
        }

        //----------------------------------------
        static byte _ParseByte( string s )
        {
            uint value = _ParseUInt32(s);
            if (value > 0xFF) throw new ArgumentOutOfRangeException();
            return (byte)value;
        }

        //----------------------------------------
        static int _ParseInt32( string s )
        {
            int value = Int32.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            return value;
        }

    }

    //--------------------------------------------------------------
    // Testing

    internal static partial class Tests
    {
        internal static void Config_NullOrEmpty( )
        {
            string configValue = ConfigurationManager.AppSettings["__NonExistent__"];
            Debug.Assert(String.IsNullOrEmpty(configValue));
        }

        internal static void Nullable_Casting( )
        {
            object foo = null;

            int? nullable = (int?)foo;
            Debug.Assert(!nullable.HasValue);

            foo = (object)4242;
            nullable = (int?)foo;
            Debug.Assert(nullable.HasValue && nullable.Value == 4242);
        }
    }

}
