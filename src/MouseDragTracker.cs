
/*
 * While left button is down (or optional joystick button held) track the net drag distance, and 
 * high/low water marks from start of the drag.  When drag distance exceeds the threshold in any 
 * direction (ie. distance above low-water mark, or below high-water mark) generate a mousewheel 
 * command, then update the high/low water marks accordingly.
 */
using System;
using System.Diagnostics;

namespace DragWheel
{
    internal static class MouseDragTracker
    {
        // Virtualized coords, measured from start-of-drag position.
        static int _xPos, _yPos;
        static int _xHi, _yHi;
        static int _xLo, _yLo;

        //--------------------------------------------------------------
        // Interface

        //----------------------------------------
        static MouseDragTracker( )
        {
            ResetDragDeltas();
        }

        //----------------------------------------
        public static void ResetDragDeltas( )
        {
            _xPos = _yPos = 0;
            _xHi = _yHi = 0;
            _xLo = _yLo = 0;
            return;
        }

        //----------------------------------------
        public static int TrackDragDeltas( int deltaX, int deltaY )
        {
            // Accum drag-distance, and ratchet up or down the water marks.
            _xPos += deltaX;
            _yPos += deltaY;

            _xHi = Math.Max(_xPos, _xHi);
            _yHi = Math.Max(_yPos, _yHi);

            _xLo = Math.Min(_xPos, _xLo);
            _yLo = Math.Min(_yPos, _yLo);

            // If drag exceeds threshold distance from baseline, send mousewheel command(s) and update baseline.
            // The total "throw" of the throttle in BMS is 125 mousewheel-clicks.  Mapping that to 2.50 inches of 
            // mouse travel, is 50 wheel-clicks/inch of travel.  For a typical 1200dpi mouse, that works out 
            // to 24 mickies per wheel-click.
            int distanceThreshold = Config.MouseResolution / 50; // @1200dpi => 24 mickies (~0.02in)

            if (_yPos >= (_yLo + distanceThreshold))
                return CalcWheelnotchDeltaAndUpdateBaseline(_yPos - _yLo);

            if (_yPos <= (_yHi - distanceThreshold))
                return CalcWheelnotchDeltaAndUpdateBaseline(_yPos - _yHi);

            return 0;

            int CalcWheelnotchDeltaAndUpdateBaseline( int draggedDistance )
            {
                int quantizedDistance = (int)(draggedDistance / distanceThreshold);

                if (quantizedDistance < 0)
                    _yHi += (int)(quantizedDistance * distanceThreshold);

                if (quantizedDistance > 0)
                    _yLo += (int)(quantizedDistance * distanceThreshold);

                return quantizedDistance;
            }
        }
    }

    //------------------------------------------------------------------
    internal static partial class Tests
    {
        //----------------------------------------
        internal static void MouseTracker_Up( )
        {
            MouseDragTracker.ResetDragDeltas();

            int dy = Config.MouseResolution / 50 + 1;
            Debug.Assert(+1 == MouseDragTracker.TrackDragDeltas(0, +dy));
            Debug.Assert(+1 == MouseDragTracker.TrackDragDeltas(0, +dy));
        }

        //----------------------------------------
        internal static void MouseTracker_Down( )
        {
            MouseDragTracker.ResetDragDeltas();
            MouseTracker_Up();

            int dy = Config.MouseResolution / 50 + 1;
            Debug.Assert(-1 == MouseDragTracker.TrackDragDeltas(0, -dy));
            Debug.Assert(-1 == MouseDragTracker.TrackDragDeltas(0, -dy));
        }
    }
}
