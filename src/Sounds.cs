using System;
using System.IO;
using System.Media;

namespace DragWheel
{
    internal static class Sounds
    {
        //--------------------------------------------------------------
        static SoundPlayer s_playerIdleStop = null;
        static SoundPlayer s_playerBurnerDetentUp = null;
        static SoundPlayer s_playerBurnerDetentDown = null;
        static SoundPlayer s_playerMaxBurner = null;

        static Sounds( )
        {
            ResourceLoader resloader = new ResourceLoader();

            string soundIdleStop = Config.IdleStopSound;
            if (!String.IsNullOrEmpty(soundIdleStop))
            {
                Stream wavStream = resloader.TryLoadResourceBinary(soundIdleStop);
                 s_playerIdleStop = new SoundPlayer(wavStream);
            }

            string soundBurnerDetentUp = Config.BurnerDetentUpSound;
            if (!String.IsNullOrEmpty(soundBurnerDetentUp))
            {
                Stream wavStream = resloader.TryLoadResourceBinary(soundBurnerDetentUp);
                s_playerBurnerDetentUp = new SoundPlayer(wavStream);
            }

            string soundBurnerDetentDown = Config.BurnerDetentDownSound;
            if (!String.IsNullOrEmpty(soundBurnerDetentDown))
            {
                Stream wavStream = resloader.TryLoadResourceBinary(soundBurnerDetentDown);
                s_playerBurnerDetentDown = new SoundPlayer(wavStream);
            }

            string soundMaxBurner = Config.MaxBurnerSound;
            if (!String.IsNullOrEmpty(soundMaxBurner))
            {
                Stream wavStream = resloader.TryLoadResourceBinary(soundMaxBurner);
                s_playerMaxBurner = new SoundPlayer(wavStream);
            }
        }

        //----------------------------------------
        public static void PlayIdleStop( )
        {
            if (s_playerIdleStop != null)
                s_playerIdleStop.Play();
        }

        //----------------------------------------
        public static void PlayBurnerDetentUp( )
        {
            if (s_playerBurnerDetentUp != null)
                s_playerBurnerDetentUp.Play();
        }

        //----------------------------------------
        public static void PlayBurnerDetentDown( )
        {
            if (s_playerBurnerDetentDown != null)
                s_playerBurnerDetentDown.Play();
        }

        //----------------------------------------
        public static void PlayMaxBurner( )
        {
            if (s_playerMaxBurner != null)
                s_playerMaxBurner.Play();
        }

    }

    //------------------------------------------------------------------
    internal static partial class Tests
    {
        internal static void PlaySound( )
        {
            Sounds.PlayIdleStop();
            Sounds.PlayBurnerDetentUp();
            Sounds.PlayBurnerDetentDown();
            Sounds.PlayMaxBurner();
        }
    }
}
