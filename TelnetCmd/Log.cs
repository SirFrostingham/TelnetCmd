using NLog;

namespace TelnetCmd
{
    public class Log
    {
        static Log()
        {
            Instance = LogManager.GetCurrentClassLogger();
        }

        public static Logger Instance { get; private set; }
    }
}