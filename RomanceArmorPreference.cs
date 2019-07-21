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


        Dictionary<string, string> presentable = new Dictionary<string, string>()
        {
            { "Body", "vestment" },
            { "Head", "hat" },
            { "Back", "cloak" },
            { "Floating Nearby", "floating artifact" },
            { "Tread", "tread decoration" },
            { "Hands", "glove" },
            { "Feet", "boot" },
            { "Arm", "armlet" },
            { "Face", "mask" } //also bows :(
        };


        public acegiak_ArmorPreference(acegiak_Romancable romancable){
            GameObject sample = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("Armor"));
            sample.MakeUnderstood();
            this.wantedType =  sample.GetPart<Armor>().WornOn;
            this.ExampleName = sample.ShortDisplayName;
            Romancable = romancable;

            amount = (float)((Stat.Rnd2.NextDouble()*2)-0.9);
            //IPart.AddPlayerMessage("They "+(amount>0?"like":"dislike")+" "+this.wantedType);

        }

        


        public string exampleObjectName(){
            GameObject sample = EncountersAPI.GetAnObject((GameObjectBlueprint b) =>
            b.InheritsFrom("Armor")
            && (b.GetPartParameter("Armor","WornOn") == this.wantedType ));

            sample.MakeUnderstood();
            return sample.a+sample.DisplayNameOnly;
        }

        public override acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
            float retamount = 0;
            string retexplain = "";
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
            string bodytext = "whoah";

            float g = (float)Stat.Rnd2.NextDouble();


            
            if(g<0.50 ){
                string sample = exampleObjectName();
                bodytext = "Have you ever seen a "+sample+"?";
                node.AddChoice("yesseen","Oh yes, I have seen a "+sample+". It was great.",amount>0?"Wow, how excellent!":"Oh, I don't think I would agree.",amount>0?1:-1);
                node.AddChoice("yesseendislike","I have but I didn't like it.",amount>0?"Oh, I guess we have different tastes.":"I agree, I saw one once and didn't like it.",amount>0?-1:1);
                node.AddChoice("notseen","No, I've not seen such a thing.",amount>0?"Oh, that's disappointing.":"That's probably for the best.",amount>0?-1:1);
            
            }else{
                bodytext = "Do you have any interesting <clothing|armor|vestments>?";
                List<GameObject> part2 = XRLCore.Core.Game.Player.Body.GetPart<Inventory>().GetObjects();

                List<BodyPart> equippedParts = XRLCore.Core.Game.Player.Body.GetPart<Body>().GetEquippedParts();
                foreach (BodyPart item in equippedParts)
                {
                    if(item.Equipped != null){
                        part2.Add(item.Equipped);
                    }
                }

                int c = 0;
                int s = 0;
                foreach(GameObject GO in part2)
                {
                    Armor mw = GO.GetPart<Armor>();
                    if(GO.GetBlueprint().InheritsFrom("Armor") && mw != null  ){
                        if(Romancable.assessGift(GO,XRLCore.Core.Game.Player.Body).amount > 0){
                            node.AddChoice("armor"+c.ToString(),"I have this "+GO.ShortDisplayName+".",amount>0?"Wow, that's very interesting!":"Oh, is that all?",amount>0?2:-1);
                            s++;
                        }else{
                            node.AddChoice("armor"+c.ToString(),"I have this "+GO.ShortDisplayName+".",amount>0?"Oh, is that all?":"Oh, that's quite ugly.",amount>0?0:-1);
                            s++;
                        }
                    }
                    // if(s>5){
                    //     break;
                    // }
                    
                }
                node.AddChoice("noweapons","Not really, no.",amount>0?"That's a pity.":"That's sensible. You probably don't need any.",amount>0?-1:1);
            }

            if(Romancable != null){
                node.Text = node.Text+"\n\n"+Romancable.GetStory(node);
            }
            node.Text = node.Text+"\n\n"+bodytext;

            return node;
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
            vars["*type*"]   = presentablec(wantedType);
            string storyTag = ((amount > 0) ?
                    "<spice.eros.opinion.armor.like.story.!random>" :
                    "<spice.eros.opinion.armor.dislike.story.!random>");
            while(this.tales.Count < 5){
                vars["*sample*"] = exampleObjectName();
                this.tales.Add(//"  &K"+storyTag.Substring(1,storyTag.Count()-2)+"&y\n"+
                    HistoricStringExpander.ExpandString(
                    storyTag, entity, null, vars));
            }
            return tales[Stat.Rnd2.Next(tales.Count)];
        }


        string presentablec(string key){
            if(!presentable.ContainsKey(key)){
                return "?"+key;
            }else{
                return presentable[key];
            }
        }

        public override string getstoryoption(string key){
            var vars = new Dictionary<string, string>();
            vars["*type*"]   = presentablec(wantedType);
            vars["*sample*"] = exampleObjectName();
            return HistoricStringExpander.ExpandString(
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