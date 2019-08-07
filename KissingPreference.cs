using System;
using XRL.Core;
using XRL.UI;
using System.Collections.Generic;

namespace XRL.World.Parts
{
    public class acegiak_KissingPreferenceResult{
        public string explanation;
        public float amount;
        public string reactPath;
        public Dictionary<string, string> reactVars;

        public acegiak_KissingPreferenceResult(float amount,string explanation,
            string                     reactPath,
            Dictionary<string, string> reactVars = null){
            this.amount = amount;
            this.explanation = explanation;
            this.reactPath = reactPath;
            this.reactVars = reactVars;
        }
    }

	public abstract class acegiak_KissingPreference
	{
        public abstract acegiak_KissingPreferenceResult attractionAmount(GameObject kissee, GameObject kisser);
        
        
        public virtual void Save(SerializationWriter Writer){
            Writer.Write(this.GetType().FullName);
        }
        public virtual void Load(SerializationReader Reader){
        }
        
        public static void Read(SerializationReader Reader,acegiak_Kissable kissable){
            string classname = Reader.ReadString();
            Type type = Type.GetType(classname);
            acegiak_KissingPreference preference = (acegiak_KissingPreference)Activator.CreateInstance(type,kissable.ParentObject);
            //preference.setRomancable(romancable);
            preference.Load(Reader);
            kissable.preferences.Add(preference);             
        }
    }

}