// minimalistic telnet implementation
// conceived by Tom Janssens on 2007/06/06  for codeproject
//
// http://www.corebvba.be

using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace TelnetCmd
{
    public class Program
    {
        // NLog not working?? Read this: https://brutaldev.com/post/logging-setup-in-5-minutes-with-nlog
        // Set NLog.config property to: COPY ALWAYS

        const string ExitCmd = "exit";
        private static void Main(string[] args)
        {
            try
            {

                //debug
                if (Debugger.IsAttached)
                {
                    Log.Instance.Debug("Appsetting hostname: {0}", ConfigurationManager.AppSettings.Get("hostname"));
                    Log.Instance.Debug("Appsetting hostport: {0}", ConfigurationManager.AppSettings.Get("hostport"));
                    Log.Instance.Debug("Appsetting username: {0}", ConfigurationManager.AppSettings.Get("username"));
                    Log.Instance.Debug("Appsetting password: {0}", ConfigurationManager.AppSettings.Get("password"));
                    Log.Instance.Debug("Appsetting timeout: {0}", ConfigurationManager.AppSettings.Get("timeout"));
                    Log.Instance.Debug("Appsetting debugCommandToSend: {0}", ConfigurationManager.AppSettings.Get("debugCommandToSend"));
                }

                //if running VS, add arg...
                if (Debugger.IsAttached)
                    args = new[] { ConfigurationManager.AppSettings.Get("debugCommandToSend") };

                var cmdArgs = string.Empty;

                foreach (var cmdArg in args)
                {
                    if (cmdArg != args.Last())
                        cmdArgs += cmdArg + " ";
                    else
                        cmdArgs += cmdArg;
                }

                Log.Instance.Debug("Command to exec: {0}", cmdArgs);

                if (string.IsNullOrEmpty(cmdArgs))
                    Console.WriteLine("Usage: TelnetCmd [command]");

                //create a new telnet connection to hostname and port
                var tc = new TelnetConnection(ConfigurationManager.AppSettings.Get("hostname"),
                    Convert.ToInt32(ConfigurationManager.AppSettings.Get("hostport")));

                //login with user "root",password "rootpassword", using a timeout of 100ms, and show server output
                string s = tc.Login(ConfigurationManager.AppSettings.Get("username"),
                    ConfigurationManager.AppSettings.Get("password"),
                    Convert.ToInt32(ConfigurationManager.AppSettings.Get("timeout")));
                Console.Write(s);

                // server output should end with "$" or ">", otherwise the connection failed
                string prompt = s.TrimEnd();
                prompt = s.Substring(prompt.Length - 1, 1);
                if (prompt != "$" && prompt != ">" && prompt != "=" && prompt != ":")
                    throw new Exception("Connection failed");

                // while connected
                while (tc.IsConnected && prompt.Trim() != "exit")
                {
                    try
                    {
                        // display server output
                        Console.Write(tc.Read());

                        // send client input to server
                        //prompt = Console.ReadLine();
                        prompt = cmdArgs;
                        tc.WriteLine(prompt);
                        Log.Instance.Debug("Wrote to telnet cmd line: {0}", prompt);

                        // display server output
                        Console.Write(tc.Read());

                        //exit
                        prompt = ExitCmd;
                        tc.WriteLine(prompt);
                        Log.Instance.Debug("Wrote to telnet cmd line: {0}", prompt);

                        // display server output
                        Console.Write(tc.Read());
                    }
                    catch (Exception)
                    {
                        //NO OP
                        //throw;
                    }
                }
                Console.WriteLine("***DISCONNECTED");

                if (Debugger.IsAttached)
                    Console.ReadLine();
            }
            catch (Exception)
            {
                //NO OP
                //throw;
            }
        }
    }
}