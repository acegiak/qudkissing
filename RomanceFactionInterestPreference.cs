using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.Rules;
using XRL.World;
using XRL.World.Encounters;
using Qud.API;
using System.Linq;
using HistoryKit;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_FactionInterestPreference : acegiak_RomancePreference
	{
        string interestedFaction = "Birds";
        float amount = 0;


        List<string> tales = new List<string>();



        public acegiak_FactionInterestPreference(acegiak_Romancable romancable){
            Romancable = romancable;
            amount = (float)(Stat.Rnd2.NextDouble()*2-0.9);
            if(romancable.ParentObject.Brain.FactionFeelings.Count > 0){
                this.interestedFaction = romancable.ParentObject.Brain.FactionFeelings.ElementAt(Stat.Rnd2.Next(0, romancable.ParentObject.Brain.FactionFeelings.Count)).Key;
                this.amount += romancable.ParentObject.Brain.FactionFeelings[this.interestedFaction];
            }else{
                this.interestedFaction = Factions.GetRandomFaction().Name;
            }
            //IPart.AddPlayerMessage("They "+(amount>0?"like":"dislike")+" "+this.interestedFaction);

        }

        string factionName(){
            if(Factions.get(this.interestedFaction)!= null){
                return Factions.get(this.interestedFaction).getFormattedName();
            }
            return "?"+this.interestedFaction;
        }


        public override acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
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
                b.GetPartParameter<string>("Preservable","Result","") == bp.Name);
                if(g != null){
                    bp = g.GetBlueprint();
                }
            }

            if(bp.InheritsFrom("Raw Meat")){
                GameObject g = EncountersAPI.GetAnObject((GameObjectBlueprint b) =>
                b.GetPartParameter<string>("Butcherable","OnSuccess","") == bp.Name);
                if(g != null){
                    bp = g.GetBlueprint();
                }
            }

            if(bp.InheritsFrom("Corpse")){
                GameObject g = EncountersAPI.GetAnObject((GameObjectBlueprint b) =>
                b.GetPartParameter<string>("Corpse","CorpseBlueprint","") == bp.Name);
                if(g != null){
                    if(this.interestedFaction == g.Brain.GetPrimaryFaction()){
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

        public GameObject exampleCreature() {
            GameObject GO = EncountersAPI.GetACreatureFromFaction(this.interestedFaction);
            return GO;
        }

        public override acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            var vars = new Dictionary<string, string>();
            vars["*type*"]   = factionName();

            return Build_QA_Node(node, "faction.qa.them", (amount > 0) ? "gen_good" : "gen_bad", vars);
        }


        public override acegiak_RomancePreferenceResult DateAssess(GameObject Date, GameObject DateObject){
            if(DateObject.GetPart<Pettable>() != null){
                return new acegiak_RomancePreferenceResult(0,Romancable.ParentObject.The+Romancable.ParentObject.ShortDisplayName+Romancable.ParentObject.GetVerb("pet")+DateObject.the+DateObject.ShortDisplayName+".");
            }
            return null;
        }

        

        public override string GetStory(acegiak_RomanceChatNode node, HistoricEntitySnapshot entity){
            var vars = new Dictionary<string, string>();
            vars["*type*"]   = factionName();
            string storyTag = ((amount > 0) ?
                "<spice.eros.opinion.faction.like.story.!random>" :
                "<spice.eros.opinion.faction.dislike.story.!random>");
            while(this.tales.Count < 5){
                SetSampleObject(vars, exampleCreature(), "member of " + factionName());
                this.tales.Add(//"  &K"+storyTag.Substring(1,storyTag.Count()-2)+"&y\n"+
                    acegiak_RomanceText.ExpandString(
                    storyTag, entity, vars));
            }
            return tales[Stat.Rnd2.Next(tales.Count)];

        }
        public override string getstoryoption(string key){
            GameObject GO = EncountersAPI.GetACreatureFromFaction(this.interestedFaction);
            if (GO == null) return null;

            var vars = new Dictionary<string, string>();
            vars["*type*"]   = factionName();
            SetSampleObject(vars, GO);
            return acegiak_RomanceText.ExpandString(
                "<spice.eros.opinion.faction." + ((amount > 0) ? "like." : "dislike.") + key + ".!random>",
                vars);
        }
        public override void Save(SerializationWriter Writer){
            base.Save(Writer);
            Writer.Write(interestedFaction);
            Writer.Write(amount);
            Writer.Write(tales.Count);
            foreach(string tale in tales){
                Writer.Write(tale);
            }
        }

        public override void Load(SerializationReader Reader){
            this.interestedFaction = Reader.ReadString();
            this.amount = Reader.ReadSingle();
            int countTales = Reader.ReadInt32();
            this.tales = new List<string>();
            for(int i = 0; i < countTales; i++){
                this.tales.Add(Reader.ReadString());
            }
        }
        

    }
}