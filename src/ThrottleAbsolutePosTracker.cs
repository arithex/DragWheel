/*
 * Keep track of estimated absolute throttle position, starting from 50% midpoint.
 * Play audio feedback when hitting min/max extent, or passing through AB detent.
 */
using System;

namespace DragWheel
{
    internal static class ThrottleAbsolutePosTracker
    {
        static int s_throwRange = Config.ThrottleThrow;
        static int s_maxMilPower = Config.ThrottleMaxMIL;

        static int s_currentPosition;

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
            s_currentPosition = s_throwRange / 2 + 1;
            s_middleClickTickcount = 0;
            return;
        }

        //----------------------------------------
        public static int TrackDelta( int deltaThrottle )
        {
            int newPosition = s_currentPosition + deltaThrottle;
            newPosition = Math.Min(Math.Max(0, newPosition), s_throwRange);

            if (s_currentPosition > 0 && newPosition == 0)
                Sounds.PlayIdleStop();

            if (s_currentPosition < s_throwRange && newPosition == s_throwRange)
                Sounds.PlayMaxBurner();

            if (s_currentPosition <= s_maxMilPower && newPosition > s_maxMilPower)
                Sounds.PlayBurnerDetentUp();

            if (s_currentPosition > s_maxMilPower && newPosition <= s_maxMilPower)
                Sounds.PlayBurnerDetentDown();

            s_currentPosition = newPosition;
            return s_currentPosition;
        }

        //----------------------------------------
        public static void TrackMiddleClick( bool isPressed, int tickCount )
        {
            if (isPressed)
                s_middleClickTickcount = tickCount;
            else if ((tickCount - s_middleClickTickcount) < 300) //TODO: tune this to match reality?
                ResetToMidpoint();
            return;
        }

    }
}
