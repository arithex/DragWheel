/*
 * Keep track of estimated absolute throttle position, starting from 50% midpoint.
 * Play audio feedback when hitting min/max extent, or passing through AB detent.
 */
using System;

namespace DragWheel
{
    internal static class ThrottleAbsolutePosTracker
    {
        static uint s_currentPosition;
        static int s_middleClickTickcount;

        //--------------------------------------------------------------
        // Interface

        //----------------------------------------
        static ThrottleAbsolutePosTracker( )
        {
            ResetToMidpoint();
        }

        //----------------------------------------
        public static void ResetToMidpoint( )
        {
            s_currentPosition = 0;
            s_middleClickTickcount = 0;

            if (!Config.ThrottleThrow.HasValue)
                return;

            s_currentPosition = Config.ThrottleThrow.Value / 2 + 1;
            return;
        }

        //----------------------------------------
        public static void TrackDelta( int deltaThrottle )
        {
            if (!Config.ThrottleThrow.HasValue)
                return;

            uint maxthrow = Config.ThrottleThrow.Value;

            long newPosition = (s_currentPosition + deltaThrottle);//NB: use signed arith to avoid bug comparing unsigned >= 0
            newPosition = Math.Min(Math.Max(0, newPosition), maxthrow);

            if (s_currentPosition > 0 && newPosition == 0)
                Sounds.PlayIdleStop();

            if (s_currentPosition < maxthrow && newPosition >= maxthrow)
                Sounds.PlayMaxBurner();

            if (Config.ThrottleMaxMIL.HasValue)
            {
                uint maxmil = Config.ThrottleMaxMIL.Value;

                if (s_currentPosition <= maxmil && newPosition > maxmil)
                    Sounds.PlayBurnerDetentUp();

                if (s_currentPosition > maxmil && newPosition <= maxmil)
                    Sounds.PlayBurnerDetentDown();
            }

            s_currentPosition = (uint)newPosition;
            return;
        }

        //----------------------------------------
        public static void TrackMiddleClick( bool isPressed, int tickCount )
        {
            if (isPressed)
                s_middleClickTickcount = tickCount;
            else if ((tickCount - s_middleClickTickcount) < 250) //TODO: tune this to match reality?
                ResetToMidpoint();
            return;
        }

    }
}
