using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Logging
{
    public static class LoggingUtilities
    {
        public const string DEFAULT_EXCEPTION_MESSAGE_HEADER = 
            "An exception was thrown during the applications runtime!\n" + 
            "While this does not mean that the application will crash it can certainly be a concern and should be further investigated.\n" +
            "You can find a collection of different information about this problem below.\n" +
            "If you are a consumer of this app, please send this log with an additional report about your actions that lead to this error to the app developer so they can further investigate on this issue.\n" +
            "Application Information:\n{0}\n" +
            "Exception Information:\n"
        ;
        public static string DEFAULT_EXCEPTION_FORMAT_MESSAGE =
            "  Exception Type:    {0}\n" +
            "  Exception Message: {1}\n" +
            "  Stack Trace:\n" +
            "{2}\n"
        ;
        public const string DEFAULT_INNER_EXCEPTION_MESSAGE = 
            "This exception has been caused by the following inner exception:\n"
        ;

        public static string GetExceptionMessage(Exception ex, string headerMessage = DEFAULT_EXCEPTION_MESSAGE_HEADER)
        {
            string message = string.Format(headerMessage, AppInfo.StringOutput);
            message += GetExceptionMessage(ex);
            return message;
        }

        private static string GetExceptionMessage(Exception ex)
        {
            string message = string.Format(DEFAULT_EXCEPTION_FORMAT_MESSAGE, ex.GetType().FullName, ex.Message, ex.StackTrace);
            if (ex.InnerException != null) {
                message += DEFAULT_INNER_EXCEPTION_MESSAGE + GetExceptionMessage(ex.InnerException);
            }
            return message;
        }
    }
}
