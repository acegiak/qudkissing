using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World;
using HistoryKit;
using XRL.Language;

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
            string sample = ((obj != null) ?
                obj.DisplayNameOnlyDirectAndStripped :
                defaultName);
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
    }
}