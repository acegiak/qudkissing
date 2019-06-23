using System;
using XRL.Core;
using XRL.UI;
using XRL.World;

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
        public abstract string GetStory(acegiak_RomanceChatNode node);
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
    }
}