using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace LocaTracker2.Views
{
    public enum StatusIndicatorState
    {
        Off,            // None
        White,          // White
        Ok,             // Green
        Info,           // Green Blinking
        WarnInfo,       // Yellow Blinking
        Warning,        // Yellow
        Error,          // Red
        CritError       // Red Blinking
    }

    public static class StatusIndicatorStateExtesions
    {
        #region Brushes
        static Brush
            defaultBrush = null,
            controlBrush = new SolidColorBrush(Color.FromArgb(255, 0, 160, 0)),
            warningBrush = new SolidColorBrush(Color.FromArgb(255, 255, 125, 0)),
            errorBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            disabledBrush = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)),
            whiteBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
        ;
        #endregion Brushes

        public static Brush GetBrush(this StatusIndicatorState state)
        {
            switch (state)
            {
                case StatusIndicatorState.White:
                    return whiteBrush;

                case StatusIndicatorState.Ok:
                case StatusIndicatorState.Info:
                    return controlBrush;

                case StatusIndicatorState.WarnInfo:
                case StatusIndicatorState.Warning:
                    return warningBrush;

                case StatusIndicatorState.Error:
                case StatusIndicatorState.CritError:
                    return errorBrush;

                case StatusIndicatorState.Off:
                default:
                    return defaultBrush;
            }
        }

        public static bool IsBlinking(this StatusIndicatorState state)
        {
            switch (state)
            {
                case StatusIndicatorState.Info:
                case StatusIndicatorState.WarnInfo:
                case StatusIndicatorState.CritError:
                    return true;
                default:
                    return false;
            }
        }

        public static Brush DefaultBrush { get { return defaultBrush; } }
        public static Brush WhiteBrush { get { return whiteBrush; } }
    }
}
