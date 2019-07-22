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
    static class acegiak_HistoricSpicePatcher
    {
        private static Dictionary<string, bool> loadedPatches = new Dictionary<string, bool>();

        public static bool Patch(string filename)
        {
            if (loadedPatches.ContainsKey(filename)) return true;
            
            HistoricSpice.Init();

            Logger.gameLog.Info("Loading spice patch: " + filename);

            string Data = "";
            using (StreamReader sr = XRL.DataManager.GetStreamingAssetsStreamReader(filename))
            {
                Data = sr.ReadToEnd();
            }

            if (Data != "")
            {
                // Strip comments
                Data = Regex.Replace(Data, "//.*$", "", RegexOptions.Multiline);


                JSONClass spice = (JSON.Parse(Data) as JSONClass)["spice"] as JSONClass;

                foreach (KeyValuePair<string, JSONNode> child in spice.ChildNodes)
                {
                    // Resolve relative links in the loaded ruleset
                    List<string> parents = new List<string>();
                    parents.Add("spice");
                    parents.Add(child.Key);
                    ResolveRelativeLinks(parents, child.Value);
                    parents.RemoveAt(parents.Count - 1);

                    // Inject the ruleset into HistoricSpice.
                    if (HistoricSpice.roots.ContainsKey(child.Key))
                    {
                        // Merge with an existing root..?
                        UnityEngine.Debug.Log("   ...Ignoring already-existing root '" + child.Key
                            + "'... JSON merge is not supported.");
                    }
                    else
                    {
                        // Simply add the new root
                        UnityEngine.Debug.Log("   ...Add root '" + child.Key + "'.");
                        HistoricSpice.roots.Add(child.Key, child.Value);
                        HistoricSpice.root.Add(child.Key, child.Value);

                        foreach (KeyValuePair<string, JSONNode> grandchild in
                            (HistoricSpice.roots[child.Key] as JSONClass).ChildNodes)
                        {
                            UnityEngine.Debug.Log("      ...Grandchild: " + grandchild.Key);
                        }
                    }
                }

                Logger.gameLog.Info("Applied spice patch: " + filename);
                loadedPatches.Add(filename, true);
                return true;
            }
            else
            {
                Logger.gameLog.Info("Failed to load spice patch: " + filename);
                loadedPatches.Add(filename, false);
                return false;
            }
        }

        // Verbatim copy of the method from HistoricSpice
        private static void ResolveRelativeLinks(List<string> parents, JSONNode current)
        {
            foreach (JSONNode node in current.Childs)
            {
                if (node.Value.Contains("^."))
                {
                    string input = node.Value;

                    Match HeaderMatches = Regex.Match(input, @"<.*?>");
                    while (HeaderMatches != null && !string.IsNullOrEmpty(HeaderMatches.Value))
                    {
                        if (HeaderMatches.Value.Contains("^."))
                        {
                            string[] parts = HeaderMatches.Groups[0].Value.Replace("<", "").Replace(">", "").Split('.');
                            string prefix = parts[0];
                            string postfix = HeaderMatches.Groups[0].Value.Substring(prefix.Length + 2).Replace(">", "");

                            int n = prefix.Length;
                            prefix = "";
                            for (int x = 0; x < parents.Count - n; x++)
                            {
                                prefix += parents[x];
                                prefix += ".";
                            }
                            string newValue = "<" + prefix + postfix + ">";

                            node.Value = node.Value.Replace(HeaderMatches.Groups[0].Value, newValue);
                        }
                        HeaderMatches = HeaderMatches.NextMatch();
                    }
                }
            }

            JSONClass cl = current as JSONClass;
            if (cl != null)
            {
                foreach (KeyValuePair<string, JSONNode> node in cl.ChildNodes)
                {
                    parents.Add(node.Key);
                    ResolveRelativeLinks(parents, node.Value);
                    parents.RemoveAt(parents.Count - 1);
                }
            }
        }
    }
}
