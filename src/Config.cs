using System;
using System.Configuration;
using System.Diagnostics;

namespace DragWheel
{
    internal static class Config
    {
        //----------------------------------------
        public static int MouseResolution
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["MouseResolution"];

                if (String.IsNullOrEmpty(configValue)) 
                    return defaultMouseResolution;

                return Int32.Parse(configValue, System.Globalization.CultureInfo.InvariantCulture);
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
                    return null;

                int value = Int32.Parse(configValue, System.Globalization.CultureInfo.InvariantCulture);
                if (value < 0 || value > 4)
                    throw new ConfigurationErrorsException("MouseButton must be in range [0-4]");

                return value;
            }
        }

        //----------------------------------------
        public static int? JoystickButton
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["JoystickButton"];

                if (String.IsNullOrEmpty(configValue))
                    return null;

                int value = Int32.Parse(configValue, System.Globalization.CultureInfo.InvariantCulture);
                if (value < 0 || value > 31)
                    throw new ConfigurationErrorsException("JoystickButton must be in range [0-31]");

                return value;
            }
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
