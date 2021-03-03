using BepInEx.Logging;

namespace Valheim.DisplayBepInExInfo
{
    internal class ValheimConsoleListener : ILogListener
    {
        public void Dispose()
        {
        }

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            if ((eventArgs.Level & DisplayInfoPlugin.LogLevels.Value) == 0)
                return;
            if (Console.instance)
                Console.instance.Print(eventArgs.ToString());
        }
    }
}
