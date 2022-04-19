using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.Rules;
using XRL.World.Encounters;
using Qud.API;
using System.Linq;
using HistoryKit;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_ArmorPreference : acegiak_RomancePreference
	{
        string wantedType = "Body";
        float amount = 0;


        string ExampleName = "Club";
        List<string> tales = new List<string>();


        public acegiak_ArmorPreference(acegiak_Romancable romancable){
            GameObject sample = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("Armor"));
            sample.MakeUnderstood();
            this.wantedType =  GetSkill(sample);
            this.ExampleName = sample.ShortDisplayName;
            Romancable = romancable;

            amount = (float)((Stat.Rnd2.NextDouble()*2f)-0.9f);
            //IPart.AddPlayerMessage("They "+(amount>0?"like":"dislike")+" "+this.wantedType);

        }

        


        public GameObject exampleObject(){
            GameObject sample = EncountersAPI.GetAnObject((GameObjectBlueprint b) =>
            b.InheritsFrom("Armor")
            && (b.GetPartParameter("Armor","WornOn") == this.wantedType ));

            sample.MakeUnderstood();
            return sample;
        }
        public string exampleObjectName(){
            var obj = exampleObject();
            return obj.a + obj.DisplayNameOnlyDirectAndStripped;
        }

        public override acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
            //IPart.AddPlayerMessage("checked armor part:"+gift.DisplayNameOnly+" "+GetSkill(gift)+"/"+wantedType+"("+amount.ToString()+")");
            if(GetSkill(gift) == wantedType){
                return new acegiak_RomancePreferenceResult(amount,(amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.pRender.DisplayName+"&Y.");
            }
            return null;
        }

        string GetSkill(GameObject GO){
            if(GO.GetPart<Armor>() != null){
                return GO.GetPart<Armor>().WornOn;
            }
            return null;
        }



        public override acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            float g = (float)Stat.Rnd2.NextDouble();

            var vars = new Dictionary<string, string>();
            vars["*type*"] = wantedType;
            
            if(g<0.50 ){
                SetSampleObject(vars, exampleObject());
                return Build_QA_Node(node, "armor.qa.seen", (amount > 0) ? "gen_good" : "gen_bad", vars);
            }else{
                return Build_QA_Node(node, "armor.qa.show_me", (amount > 0) ? "gen_good" : "gen_bad", vars);
            }
        }


        public override acegiak_RomancePreferenceResult DateAssess(GameObject Date, GameObject DateObject){
            List<BodyPart> equippedParts = XRLCore.Core.Game.Player.Body.GetPart<Body>().GetEquippedParts();
            foreach (BodyPart item in equippedParts)
            {
                if(item.Equipped != null){
                    if(item.Equipped.GetPart<Armor>() != null && item.Equipped.GetPart<Armor>().WornOn == this.wantedType){
                        return new acegiak_RomancePreferenceResult(amount,(amount > 0?Romancable.ParentObject.GetVerb("like"):Romancable.ParentObject.GetVerb("dislike"))+" your "+item.Equipped.DisplayNameOnly);
                    }
                }
            }
            return null;
        }



        public override string GetStory(acegiak_RomanceChatNode node, HistoricEntitySnapshot entity){
            var vars = new Dictionary<string, string>();
            vars["*type*"] = wantedType;
            string storyTag = ((amount > 0) ?
                    "<spice.eros.opinion.armor.like.story.!random>" :
                    "<spice.eros.opinion.armor.dislike.story.!random>");
            while(this.tales.Count < 5){
                SetSampleObject(vars, exampleObject());
                this.tales.Add(//"  &K"+storyTag.Substring(1,storyTag.Count()-2)+"&y\n"+
                    acegiak_RomanceText.ExpandString(
                    storyTag, entity, null, vars));
            }
            return tales[Stat.Rnd2.Next(tales.Count)];
        }

        public override string getstoryoption(string key){
            var vars = new Dictionary<string, string>();
            vars["*type*"]   = wantedType;
            SetSampleObject(vars, exampleObject());
            return acegiak_RomanceText.ExpandString(
                "<spice.eros.opinion.armor." + ((amount > 0) ? "like." : "dislike.") + key + ".!random>",
                null, null, vars);
        }

        public override void Save(SerializationWriter Writer){
            base.Save(Writer);
            Writer.Write(wantedType);
            Writer.Write(amount);
            Writer.Write(ExampleName);
            Writer.Write(tales.Count);
            foreach(string tale in tales){
                Writer.Write(tale);
            }
        }

        public override void Load(SerializationReader Reader){
            this.wantedType = Reader.ReadString();
            this.amount = Reader.ReadSingle();
            this.ExampleName = Reader.ReadString();
            int countTales = Reader.ReadInt32();
            this.tales = new List<string>();
            for(int i = 0; i < countTales; i++){
                this.tales.Add(Reader.ReadString());
            }
        }
    }
}