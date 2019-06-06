using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.Rules;
using XRL.World;
using XRL.World.Encounters;
using Qud.API;
using System.Linq;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_FactionInterestPreference : acegiak_RomancePreference
	{
        string interestedFaction = "Birds";
        float amount = 0;
        acegiak_Romancable Romancable = null;

        List<string> tales = new List<string>();



        public acegiak_FactionInterestPreference(acegiak_Romancable romancable){
            Romancable = romancable;
            amount = (float)(Stat.Rnd2.NextDouble()*10-5);
            if(romancable.ParentObject.pBrain.FactionFeelings.Count > 0){
                this.interestedFaction = romancable.ParentObject.pBrain.FactionFeelings.ElementAt(Stat.Rnd2.Next(0, romancable.ParentObject.pBrain.FactionFeelings.Count)).Key;
                this.amount += romancable.ParentObject.pBrain.FactionFeelings[this.interestedFaction];
            }else{
                this.interestedFaction = Factions.GetRandomFaction().Name;
            }
            //IPart.AddPlayerMessage("They "+(amount>0?"like":"dislike")+" "+this.interestedFaction);

        }

        string factionName(){
            if(Factions.FactionList.ContainsKey(this.interestedFaction)){
                return Factions.FactionList[this.interestedFaction].getFormattedName();
            }
            return "?"+this.interestedFaction;
        }


        public acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
            float retamount = 0;
            string retexplain = "";
            // if(getType(gift) == wantedType){
            //     return new acegiak_RomancePreferenceResult(amount,(amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.ShortDisplayName+"&Y.");
            // }
            return null;
        }

        public string examplename(){
            GameObject GO = EncountersAPI.GetACreatureFromFaction(this.interestedFaction);
            if(GO == null){
                return "member of "+factionName();
            }
            return GO.ShortDisplayName;
        }

        public acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            string bodytext = "whoah";

            float g = (float)Stat.Rnd2.NextDouble();

            if(g<1){
                bodytext = "[What do you think of|How do you feel about|What is your opinion of] "+factionName()+"?";
                node.AddChoice("likethem","I am quite fond of them.",amount>0?"They are lovely, aren't they?":"Oh, You must keep awful company.",amount>0?1:-1);
                node.AddChoice("dislikethem","[Loathsome creatures, one and all|They're horrible|I can't stand them].",amount>0?"That's very judgemental":"Aren't they horrible?",amount>0?-1:1);
            }

            if(Romancable != null){
                node.Text = node.Text+"\n\n"+Romancable.GetStory();
            }
            node.Text = node.Text+"\n\n"+bodytext;

            return node;
        }


        public acegiak_RomancePreferenceResult DateAssess(GameObject Date, GameObject DateObject){
            return null;
        }

        void FireEvent(Event E){
            
        }

        public string GetStory(){

            
                List<string> Stories = null;
                if(amount>0){
                    GameObject item = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("Item"));
                    item.MakeUnderstood();
                    Stories = new List<string>(new string[] {
                        "Once, I had a dream about a ==example==. When I woke [my fungal infection was cured|I saw a rainbow|my greatest enemy was dead]!",
                        "Once, a ==example== [gave me|showed me|told me about] a "+item.ShortDisplayName+".",
                        "I think ==type== are neat."
                    });
                }else{
                    GameObject item = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("MeleeWeapon"));
                    item.MakeUnderstood();
                    Stories = new List<string>(new string[] {
                        "Once, I had a dream about a ==example==. When I woke up [I had a headache|I was drenched in sweat].",
                        "Once, a ==example== [attacked|tried to kill me] me with a "+item.ShortDisplayName+".",
                        "I just [hate|can't stand] ==type==."
                    });
                }
                return  Stories[Stat.Rnd2.Next(0,Stories.Count-1)].Replace("==type==",factionName()).Replace("==example==",examplename());

        }

        

    }
}