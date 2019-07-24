using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using HistoryKit;
using SimpleJSON;
using XRL;

namespace HistoryKit
{
    static class acegiak_RomanceText
    {
		private static bool initialized = false;
		private static bool debug = true;
		private static StreamWriter debugLogger = null;

        public static void Init()
        {
			if (initialized) return;

            ModManager.ForEachFileIn("acegiak_romance", (string filePath, ModInfo modInfo) =>
            {
                if (filePath.ToLower().Contains(".json"))
                    acegiak_HistoricSpicePatcher.Patch(filePath);
            });

			if (debug)
			{
				string logPath = Application.persistentDataPath + "/log_romance.txt";

				debugLogger = new StreamWriter(logPath);
                if (debugLogger != null)
                {
                    debugLogger.WriteLine("Opened RomanceSpice log...");
                    debugLogger.Flush();
                    Logger.gameLog.Info("Opened RomanceSpice log at " + logPath);
                }
                else Logger.gameLog.Info("Failed to open RomanceSpice log at " + logPath);
			}

			initialized = true;
        }

		public static string ExpandString(string input, Dictionary<string, string> vars = null)
		{
			return ExpandString(input, null, null, vars);
		}
		public static string ExpandString(string input, HistoricEntitySnapshot entity, Dictionary<string, string> vars = null)
		{
			return ExpandString(input, entity, null, vars);
		}

		public static string ExpandQuery(string input, HistoricEntitySnapshot entity, History history, Dictionary<string, string> vars = null)
		{
			if (!initialized) Init();

			string output = HistoricStringExpander.ExpandQuery(input, entity, history, vars, null);

			LogQuery(input, output);

			return output;
		}
		public static string ExpandString(string input, HistoricEntitySnapshot entity, History history, Dictionary<string, string> vars = null)
		{
			if (!initialized) Init();

			string output = HistoricStringExpander.ExpandString(input, entity, history, vars);

			LogQuery(input, output);

			return output;
		}

		public static void Log(string line)
		{
			if (debugLogger != null)
				debugLogger.WriteLine(line);
		}

		private static void LogQuery(string input, string output)
		{
			if (debugLogger != null && input != output)
			{
				if (input.First() == '<' && input.Last() == '>')
				{
					// Condense tags.
					int first = 1, last = input.Count()-1;
					if (input.StartsWith("<spice.eros.")) first += 11;
					else if (input.StartsWith("<spice.")) first += 6;
					if (input.EndsWith(".!random>")) last -= 8;
					input = input.Substring(first, last-first);

					debugLogger.Write("~");
					debugLogger.Write(input);
				}
				else
				{
					debugLogger.Write("[[");
					debugLogger.Write(input);
					debugLogger.Write("]]");
				}
				if (output.Count() > 0)
				{
					debugLogger.WriteLine();
					debugLogger.Write('\t');
					debugLogger.WriteLine(output);
				}
				else
				{
					debugLogger.WriteLine("  <empty>");
				}
                debugLogger.Flush();
			}
		}
    }
}
