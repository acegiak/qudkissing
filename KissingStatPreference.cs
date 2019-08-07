using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_StatPreference : acegiak_KissingPreference
	{
        string Stat = "Ego";
        float Amount = 0.1f;
        int Needs = 0;

        public acegiak_StatPreference(GameObject GO){
            if(GO == null){
                return;
            }
            List<string> Stats = new List<string>(new string[] { "Strength", "Agility", "Toughness", "Ego", "Willpower","Intelligence" });

            Random random = new Random();
            this.Stat = Stats[random.Next(Stats.Count-1)];
            this.Amount =  (float)((random.NextDouble()*2)-1);
            this.Needs =  random.Next(-3,5);
        }

        public override acegiak_KissingPreferenceResult attractionAmount(GameObject kissee, GameObject GO){
            //             IPart.AddPlayerMessage("They "+(Amount>0?"like ":"dislike ")+this.Stat+" over "+this.Needs.ToString());

            // IPart.AddPlayerMessage("Your "+Stat+(GO.StatMod(Stat)>=Needs?" meets ":" does not meet ")+this.Needs.ToString());

            int statHi = (GO.StatMod(Stat)>=Needs?1:-1);
            float result = Amount * statHi;
            string reactPath = "stat." + Stat + ((statHi>0) ? ".hi" : ".lo");
            string explain = ((result>0)?"is attracted to":"is &rnot attracted to")+" your "+((GO.StatMod(Stat)>=Needs)?"high ":"low ")+Stat;

            return new acegiak_KissingPreferenceResult(result,explain,reactPath);
        }

        public override void Save(SerializationWriter Writer){
            base.Save(Writer);
            Writer.Write(Stat);
            Writer.Write(Amount);
            Writer.Write(Needs);
        }

        public override void Load(SerializationReader Reader){
            base.Load(Reader);
            this.Stat = Reader.ReadString();
            this.Amount = Reader.ReadSingle();
            this.Needs = Reader.ReadInt32();
        }
    }
}