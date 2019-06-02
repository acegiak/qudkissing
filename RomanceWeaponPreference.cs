using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.Encounters;
using Qud.API;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_WeaponPreference : acegiak_RomancePreference
	{
        string wantedType = "Cudgel";
        float amount = 0;

        Dictionary<string, string> verbs = new Dictionary<string, string>()
        {
            { "Cudgel", "bashing" },
            { "ShortBlades", "stabbing" },
            { "LongBlades", "slashing" },
            { "Axe", "slashing" }
        };


        public acegiak_WeaponPreference(){
            GameObject sample = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("MeleeWeapon"));
            this.wantedType =  sample.GetPart<MeleeWeapon>().Skill;
        }

        public acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
            float retamount = 0;
            string retexplain = "";
            if(gift.GetPart<MeleeWeapon>() != null && gift.GetPart<MeleeWeapon>().Skill == wantedType){
                return new acegiak_RomancePreferenceResult(amount,(amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.pRender.DisplayName+"&Y.");
            }
            return null;
        }


        public acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            string bodytext = "whoah";


			Random r = new Random();
            float g = (float)r.NextDouble();

            if(g<0.5 && verbs.ContainsKey(wantedType)){
                bodytext = "Do you ever think about just "+verbs[wantedType]+" people?";
                node.AddChoice("yeahcleave","Oh yes, quite often.",amount>0?"Oh good. I thought I was the only one.":"Really? That's very troubling.",amount>0?1:-1);
                node.AddChoice("nahcleave","No, that sounds bad.",amount>0?"Oh, I guess it is. Sorry.":"It does, doesn't it? How scary!",amount>0?-1:1);
            }else{
                bodytext = "How do you like to slay your enemies?";
                node.AddChoice(wantedType,"I like killing with a "+wantedType+".",amount>0?"Me too!":"That's quite violent, isn't it?",amount>0?1:-1);
                node.AddChoice("notmelee","I prefer to keep them at a distance.",amount>0?"That sounds cowardly.":"That sounds very wise.",amount>0?-1:1);
            }


            node.Text = node.Text+"\n\n"+bodytext;

            return node;
        }


    }
}