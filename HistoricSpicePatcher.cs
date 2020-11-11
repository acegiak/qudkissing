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
            
            HistoricSpice.CheckInit();

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


                JSONClass patch = (JSON.Parse(Data) as JSONClass)["spice"] as JSONClass;

                foreach (KeyValuePair<string, JSONNode> child in patch.ChildNodes)
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
                        //UnityEngine.Debug.Log("   ...Ignoring already-existing root '" + child.Key
                        //    + "'... JSON merge is not supported.");
                        Logger.gameLog.Info("| ...Merge root '" + child.Key + "'.");
                        int errors = MergeJSON(child.Key, HistoricSpice.roots[child.Key], child.Value);
                        if (errors != 0)
                            Logger.gameLog.Info("| ...Merged, but with " + errors.ToString() + " errors.");
                    }
                    else
                    {
                        // Simply add the new root
                        Logger.gameLog.Info("| ...Add root '" + child.Key + "'.");
                        HistoricSpice.roots.Add(child.Key, child.Value);
                        HistoricSpice.root.Add(child.Key, child.Value);

                        foreach (KeyValuePair<string, JSONNode> grandchild in
                            (HistoricSpice.roots[child.Key] as JSONClass).ChildNodes)
                        {
                            Logger.gameLog.Info("| | ...Grandchild: " + grandchild.Key);
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

        // Merge patch JSON class into target class.
        private static int MergeJSON(string name, JSONNode target, JSONNode patch)
        {
            var targetObject = target as JSONClass;
            var patchObject = patch as JSONClass;
            if (targetObject == null)
            {
                Logger.gameLog.Info("| | ...Can't merge into non-object " + name);
                return 1;
            }
            if (patchObject == null)
            {
                Logger.gameLog.Info("| | ...Can't merge non-object into object " + name);
                return 1;
            }

            int errorCount = 0;
            foreach (JSONNode child in patch.Childs)
            {
                var targetChild = target[child.Key];
                if (targetChild == null)
                {
                    // Copy child into target
                    target.Add(child.Key, child);
                    //targetChild = child.Value;
                    //Logger.gameLog.Info("| | Insert value " + name + "." + child.Key);
                }
                else
                {
                    // Merge child object into target member object
                    Logger.gameLog.Info("| | Merge sub-object " + name + "." + child.Key);
                    errorCount += MergeJSON(name + "." + child.Key, targetChild, patch[child.Key]);
                }
            }
            //Logger.gameLog.Info("| | Children of " + name + " are now:");
            //foreach (JSONNode child in target.Childs)
            //    Logger.gameLog.Info("| | | " + child.Key + " ... " + target[child.Key].Key);
            return errorCount;
        }

        // Verbatim copy of the method from HistoricSpice as of July 2019
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
