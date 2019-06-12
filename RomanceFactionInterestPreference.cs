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
            amount = (float)(Stat.Rnd2.NextDouble()*2-0.9);
            if(romancable.ParentObject.pBrain.FactionFeelings.Count > 0){
                this.interestedFaction = romancable.ParentObject.pBrain.FactionFeelings.ElementAt(Stat.Rnd2.Next(0, romancable.ParentObject.pBrain.FactionFeelings.Count)).Key;
                this.amount += romancable.ParentObject.pBrain.FactionFeelings[this.interestedFaction];
            }else{
                this.interestedFaction = Factions.GetRandomFaction().Name;
            }
            IPart.AddPlayerMessage("They "+(amount>0?"like":"dislike")+" "+this.interestedFaction);

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
            if(gift.GetPart<ModFeathered>() != null && this.interestedFaction == "Birds"){
                retamount += this.amount;
                retexplain += Romancable.ParentObject.The+Romancable.ParentObject.ShortDisplayName+Romancable.ParentObject.GetVerb("think")+" the feathers on "+gift.the+gift.ShortDisplayName+(this.amount > 0?"are excellent":"are horrible")+".\n";
            }
            if(gift.GetPart<ModScaled>() != null && this.interestedFaction == "Unshelled Reptiles"){
                retamount += this.amount;
                retexplain += Romancable.ParentObject.The+Romancable.ParentObject.ShortDisplayName+Romancable.ParentObject.GetVerb("think")+" the scales on "+gift.the+gift.ShortDisplayName+(this.amount > 0?"are excellent":"are horrible")+".\n";
            }
            if(gift.GetPart<AddsRep>() != null && gift.GetPart<AddsRep>().Faction == this.interestedFaction){
                retamount += this.amount;
            }
            GameObjectBlueprint bp = gift.GetBlueprint();
            if(bp.InheritsFrom("Jerky")){
                GameObject g = EncountersAPI.GetAnObject((GameObjectBlueprint b) =>
                b.GetPartParameter("Preservable","Result") == bp.Name);
                if(g != null){
                    bp = g.GetBlueprint();
                }
            }

            if(bp.InheritsFrom("Raw Meat")){
                GameObject g = EncountersAPI.GetAnObject((GameObjectBlueprint b) =>
                b.GetPartParameter("Butcherable","OnSuccess") == bp.Name);
                if(g != null){
                    bp = g.GetBlueprint();
                }
            }

            if(bp.InheritsFrom("Corpse")){
                GameObject g = EncountersAPI.GetAnObject((GameObjectBlueprint b) =>
                b.GetPartParameter("Corpse","CorpseBlueprint") == bp.Name);
                if(g != null){
                    if(this.interestedFaction == g.pBrain.GetPrimaryFaction()){
                        retamount += amount*-1;
                        retexplain += Romancable.ParentObject.The+Romancable.ParentObject.ShortDisplayName+Romancable.ParentObject.GetVerb("is")+(this.amount > 0?"upset by the remains of":"pleased by the remains of")+g.a+g.DisplayNameOnly+".\n";
                    }
                }
            }
            if(retamount != 0 || retexplain != ""){
                return new acegiak_RomancePreferenceResult(retamount,retexplain);
            }
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
                node.AddChoice("dislikethem","[Loathsome creatures, one and all|They are wretched|I can't stand them].",amount>0?"That's very judgemental":"Aren't they horrible?",amount>0?-1:1);
            }

            if(Romancable != null){
                node.Text = node.Text+"\n\n"+Romancable.GetStory();
            }
            node.Text = node.Text+"\n\n"+bodytext;

            return node;
        }


        public acegiak_RomancePreferenceResult DateAssess(GameObject Date, GameObject DateObject){
            if(DateObject.GetPart<Pettable>() != null){
                return new acegiak_RomancePreferenceResult(0,Romancable.ParentObject.The+Romancable.ParentObject.ShortDisplayName+Romancable.ParentObject.GetVerb("pet")+DateObject.the+DateObject.ShortDisplayName+".");
            }
            return null;
        }

        

        public string GetStory(){
            while(this.tales.Count < 5){
                List<string> Stories = null;
                if(amount>0){
                    GameObject item = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("Item"));
                    item.MakeUnderstood();
                    Stories = new List<string>(new string[] {
                        "Once, I had a dream about a ==example==. When I woke "+Romancable.storyoptions("goodthinghappen","I saw a rainbow")+"!",
                        "Once, a ==example== [gave me|showed me|told me about] "+Romancable.storyoptions("goodobject",item.a+item.ShortDisplayName)+".",
                        "I think ==type== are neat."
                    });
                }else{
                    GameObject item = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("MeleeWeapon"));
                    item.MakeUnderstood();
                    Stories = new List<string>(new string[] {
                        "Once, I had a dream about a ==example==. When I woke up "+Romancable.storyoptions("goodthinghappen","I was drenched in sweat")+"!",
                        "Once, a ==example== [attacked|tried to kill me] me with "+Romancable.storyoptions("badweapon",item.a+item.ShortDisplayName)+".",
                        "I just [hate|can't stand] ==type==."
                    });
                }
                this.tales.Add(Stories[Stat.Rnd2.Next(0,Stories.Count-1)].Replace("==type==",factionName()).Replace("==example==",examplename()));
            }
            return tales[Stat.Rnd2.Next(tales.Count)];

        }
        public string getstoryoption(string key){
            GameObject GO = EncountersAPI.GetACreatureFromFaction(this.interestedFaction);
            if(GO != null){
                if(key == "goodperson" && this.amount > 0){
                    return GO.a+GO.ShortDisplayName;
                }
                if(key == "badperson" && this.amount < 0){
                    return GO.a+GO.ShortDisplayName;
                }
                if(key == "goodthinghappen" && this.amount > 0){
                    return "I met "+GO.a+GO.ShortDisplayName;
                }
                if(key == "badthinghappen" && this.amount < 0){
                    return "I met "+GO.a+GO.ShortDisplayName;
                }
            }
            return null;
        }
        

    }
}