using System;
using System.Linq;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World;
using HistoryKit;
using XRL.Language;
using SimpleJSON;

namespace XRL.World.Parts
{
    public class acegiak_RomancePreferenceResult{
        public string explanation;
        public float amount;
        public acegiak_RomancePreferenceResult(float amount,string explanation){
            this.amount = amount;
            this.explanation = explanation;
        }
    }

	[Serializable]
	public abstract class acegiak_RomancePreference
	{
        [NonSerialized]
        public acegiak_Romancable Romancable = null;
        public abstract acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift);
        public abstract acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node);

        public abstract string GetStory(acegiak_RomanceChatNode node, HistoricEntitySnapshot entity);
        public abstract acegiak_RomancePreferenceResult DateAssess(GameObject Date, GameObject DateObject);

        public abstract string getstoryoption(string key);
        public void setRomancable(acegiak_Romancable romancable){
            this.Romancable = romancable;
        }
        public virtual void Save(SerializationWriter Writer){
            Writer.Write(this.GetType().FullName);
        }
        public virtual void Load(SerializationReader Reader){
        }
        
        public static void Read(SerializationReader Reader, acegiak_Romancable romancable){
            string classname = Reader.ReadString();
            Type type = Type.GetType(classname);
            acegiak_RomancePreference preference = (acegiak_RomancePreference)Activator.CreateInstance(type,romancable);
            preference.setRomancable(romancable);
            preference.Load(Reader);
            romancable.preferences.Add(preference);             
        }

        // Utility
        public static void SetSampleObject(Dictionary<string, string> vars, GameObject obj, string defaultName = "thing")
        {
            if (obj != null)
            {
                string sample = obj.DisplayNameOnlyDirectAndStripped;
                string samples = Grammar.Pluralize(sample);
                vars["*sample*"]   = sample;
                vars["*samples*"]  = samples;
                vars["*a_sample*"] = obj.a + sample;
                vars["*A_sample*"] = obj.A + sample;
                vars["*the_sample*"] = obj.the + sample;
                vars["*The_sample*"] = obj.The + sample;
                vars["*the_samples*"] = obj.the + samples;
                vars["*The_samples*"] = obj.The + samples;
            }
            else
            {
                string sample = defaultName;
                string samples = Grammar.Pluralize(sample);
                vars["*sample*"]   = sample;
                vars["*samples*"]  = samples;
                vars["*a_sample*"] = "a " + sample;
                vars["*A_sample*"] = "A " + sample;
                vars["*the_sample*"] = "the " + sample;
                vars["*The_sample*"] = "The " + sample;
                vars["*the_samples*"] = "the " + samples;
                vars["*The_samples*"] = "The " + samples;
            }
        }

        public acegiak_RomanceChatNode Build_QA_Node(
            acegiak_RomanceChatNode node,
            string qa_path,
            string gen_type,
            Dictionary<string, string> vars)
        {
            string qaBase = "<spice.eros.opinion." + qa_path + ".";
            string qaGen = qaBase + gen_type + ".!random>";
            acegiak_RomanceText.Log("Q&A Rule: " + qaGen);

            string code = acegiak_RomanceText.ExpandString(qaGen, vars);

            acegiak_RomanceText.Log("Q&A Script: " + code);

            string[] parts = code.Split('$');
            if (parts.Count() < 1) acegiak_RomanceText.Log("ERROR: Q&A blank: " + code);

            string bodytext;

            if (parts[0] == "Q")
            {
                if (parts.Count() < 3)
                    acegiak_RomanceText.Log("ERROR: Q&A too few params in quiz: " + code);
                
                // Generated question-and-answer.
                bodytext = parts[1];

                for (int i = 2; i < parts.Count(); ++i)
                {
                    string[] subparts = parts[i].Split(';');
                    if (subparts.Count() != 3)
                        acegiak_RomanceText.Log("ERROR: Q&A choice should have two semicolons: " + parts[i]);

                    string
                        choice = acegiak_RomanceText.ExpandString(qaBase + subparts[0] + ".a.!random>", vars),
                        answer = acegiak_RomanceText.ExpandString(qaBase + subparts[0] + "." + subparts[1] + ".!random>", vars);
                    
                    int result = 0;
                    if (!int.TryParse(subparts[2], out result))
                        acegiak_RomanceText.Log("ERROR: Q&A non-integer choice result: " + code);

                    node.AddChoice(subparts[0], choice, answer, result);
                }
            }
            else
            {
                bodytext = "&RUnknown Q&A Node!&y";
                node.AddChoice("ok", "How unfortunate.", "A terrible shame indeed!", 0);
            }

            // Insert story
            if(Romancable != null){
                node.Text = node.Text+"\n\n"+Romancable.GetStory(node);
            }
            node.Text = node.Text+"\n\n"+bodytext;

            return node;
        }
    }
}