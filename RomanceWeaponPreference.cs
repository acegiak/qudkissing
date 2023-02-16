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

using XRL.World.Anatomy;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_WeaponPreference : acegiak_RomancePreference
	{
        string wantedType = "Cudgel";
        float amount = 0;
        

        string ExampleName = "Club";
        List<string> tales = new List<string>();


        static Dictionary<string, string> verbs = new Dictionary<string, string>()
        {
            { "Cudgel", "bashing" },
            { "ShortBlades", "stabbing" },
            { "LongBlades", "slashing" },
            { "Axe", "cleaving" },
            { "Whip", "whipping" },
            { "Chain", "chain-beating" },
            { "Pistol", "shooting" },
            { "HeavyWeapons", "exploding" },
            { "Rifle", "sniping" } //also bows :(
        };

        static Dictionary<string, string> presentable = new Dictionary<string, string>()
        {
            { "Cudgel", "cudgel" },
            { "ShortBlades", "short blade" },
            { "LongBlades", "long blade" },
            { "Axe", "axe" },
            { "Whip", "whip" },
            { "Chain", "chain" },
            { "Pistol", "pistol" },
            { "HeavyWeapons", "heavy weapon" },
            { "Rifle", "rifle" } //also bows :(
        };

        public acegiak_WeaponPreference(acegiak_Romancable romancable){
            GameObject sample = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("MeleeWeapon"));
            sample.MakeUnderstood();
            this.wantedType =  sample.GetPart<MeleeWeapon>().Skill;
            this.ExampleName = sample.ShortDisplayName;
            Romancable = romancable;

            amount = (float)((Stat.Rnd2.NextDouble()*2)-0.9);
            //IPart.AddPlayerMessage("They "+(amount>0?"like":"dislike")+" "+this.wantedType);

        }
        public GameObject exampleObject(){
            GameObject sample = EncountersAPI.GetAnObject((GameObjectBlueprint b) =>
            (b.InheritsFrom("MeleeWeapon") ||
            b.InheritsFrom("BaseMissileWeapon"))
            && (b.GetPartParameter("MeleeWeapon","Skill") == this.wantedType || b.GetPartParameter("MissileWeapon","Skill") == this.wantedType));

            sample.MakeUnderstood();
            return sample;
        }
        public string exampleObjectName(){
            var obj = exampleObject();
            return obj.a + obj.DisplayNameOnlyDirectAndStripped;
        }

        public override acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
            if(GetSkill(gift) == wantedType){
                return new acegiak_RomancePreferenceResult(amount,(amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.pRender.DisplayName+"&Y.");
            }
            return null;
        }

        string GetSkill(GameObject GO){
            if(GO.GetPart<MeleeWeapon>() != null && GO.GetBlueprint().InheritsFrom("MeleeWeapon")){
                return GO.GetPart<MeleeWeapon>().Skill;
            }
            if(GO.GetPart<MissileWeapon>() != null){
                return GO.GetPart<MissileWeapon>().Skill;
            }
            return null;
        }



        public override acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            string bodytext = "whoah";

            float g = (float)Stat.Rnd2.NextDouble();
            bool haskey = false;
            foreach(var item in verbs){
                if(item.Key == wantedType){
                    haskey = true;
                    break;
                }
            }

            var vars = new Dictionary<string, string>();
            vars["*type*"] = wantedType;

            if(g<0.2 && haskey){
                SetSampleObject(vars, exampleObject());
                return Build_QA_Node(node, "weapon.qa.postal", (amount > 0) ? "gen_good" : "gen_bad", vars);

                /*bodytext = "Do you ever <think about|fantasize about|ponder|dream about> just "+verbs[wantedType]+" people?";
                node.AddChoice("yeahcleave","Oh yes, quite often.",amount>0?"Oh good. I thought I was the only one.":"Really? That is troubling.",amount>0?1:-1);
                node.AddChoice("nahcleave","No, that sounds bad.",amount>0?"Oh, I guess it is. Sorry.":"It does, doesn't it? How scary!",amount>0?-1:1);*/
            }else if(g<0.4 && haskey){
                bodytext = "How do you like to <slay|attack|fight|combat> your enemies?";
                foreach(var item in verbs){
                    if((item.Key  == wantedType || Stat.Rnd2.NextDouble() < 0.5)){
                        GameObject GO = EncountersAPI.GetAnObject((GameObjectBlueprint b) => b.GetPartParameter("MeleeWeapon","Skill")==item.Key ||b.GetPartParameter("MissileWeapon","Skill")==item.Key );
                        if(item.Key == wantedType || (GO != null && Romancable.assessGift(GO,XRLCore.Core.Game.Player.Body).amount>0)){
                            node.AddChoice(item.Key,"&cI <like|prefer> "+item.Value+" them with a "+presentable[item.Key]+".",amount>0?"Me too!":"That's quite violent, isn't it?",amount>0?1:-1);
                        }else{
                            node.AddChoice(item.Key,"&cI <like|prefer> "+item.Value+" them with a "+presentable[item.Key]+".",amount>0?"That sounds unpleasant.":"That's quite violent, isn't it?",amount>0?1:-1);
                        }
                    }
                }
                node.AddChoice("notmelee","&rI prefer to avoid combat entirely.",amount>0?"That sounds cowardly.":"That sounds very wise.",amount>0?-1:1);
            }else if(g<0.60 && haskey){
                SetSampleObject(vars, exampleObject());
                return Build_QA_Node(node, "armor.qa.seen", (amount > 0) ? "gen_good" : "gen_bad", vars);

                /*string sample = exampleObjectName();
                bodytext = "Have you ever seen "+sample+"?";
                node.AddChoice("yesseen","Oh yes, I have seen "+sample+". It was great.",amount>0?"Wow, how excellent!":"Oh, I don't think I would agree.",amount>0?1:-1);
                node.AddChoice("yesseendislike","I have but I didn't like it.",amount>0?"Oh, I guess we have different tastes.":"I agree, I saw one once and didn't like it.",amount>0?-1:1);
                node.AddChoice("notseen","No, I've not seen such a thing.",amount>0?"Oh, that's disappointing.":"That's probably for the best.",amount>0?-1:1);*/
            }else if(g<0.80 ){
                return Build_QA_Node(node, "weapon.qa.factoid", (amount > 0) ? "gen_good" : "gen_bad", vars);

                /*if(wantedType == "Cudgel"){
                    bodytext = "Did you know that heavy weapons like maces and clubs can sometimes stun people when they hit?";}
                if(wantedType == "ShortBlades"){
                    bodytext = "Are you familiar with short blades like daggers and knives? I hear that they make wounds that bleed profusely";}
                if(wantedType == "LongBlades"){
                    bodytext = "I hear long blades are very versatile. Talented swordsmen can use them for defence as well offence.";}
                if(wantedType == "Axe"){
                    bodytext = "Axes are very dangerous. No only can they crack armour but they can also cleave off whole limbs!";}
                if(wantedType == "Whip"){
                    bodytext = "I hear whips are usually used to keep dogs in line?";}
                if(wantedType == "Chain"){
                    bodytext = "I hear some people just beat their enemies with chains?";}
                if(wantedType == "HeavyWeapons"){
                    bodytext = "I heard heavy ranged weapons often fire explosives!";}
                if(wantedType == "Rifle"){
                    bodytext = "I hear bows and rifles are very good for shooting folks at long range.";}
                if(wantedType == "Pistol"){
                    bodytext = "Did you know some folks weild a pistol in each hand?";}
                node.AddChoice("approve","I have seen as much. It is <glorious|wonderful|fantastic>.",amount>0?"So fascinating!":"Oh, how scary.",amount>0?1:-1);
                node.AddChoice("disprove","That is, unfortunately, true.",amount>0?"Oh? I think it sounds very impressive.":"Yes it seems quite dangerous.",amount>0?-1:1);
                node.AddChoice("disagree","I'm not sure that is true.","Oh, isn't it? How odd.",-1);*/
            }else{
                return Build_QA_Node(node, "weapon.qa.show_me", (amount > 0) ? "gen_good" : "gen_bad", vars);
                /*bodytext = "Do you have any interesting weapons?";
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
                    MeleeWeapon mw = GO.GetPart<MeleeWeapon>();
                    MissileWeapon rw = GO.GetPart<MissileWeapon>();
                    if((GO.GetBlueprint().InheritsFrom("BaseMissileWeapon") || GO.GetBlueprint().InheritsFrom("MeleeWeapon")) && (mw != null || rw != null) ){
                        if(Romancable.assessGift(GO,XRLCore.Core.Game.Player.Body).amount > 0){
                            node.AddChoice("weapon"+c.ToString(),"I have this "+GO.DisplayName+".",amount>0?"Wow, that's very interesting!":"Oh, is that all?",amount>0?2:-1);
                            s++;
                        }else{
                            node.AddChoice("weapon"+c.ToString(),"I have this "+GO.DisplayName+".",amount>0?"Oh, is that all?":"Hmm, that seems dangerous.",amount>0?0:-1);
                            s++;
                        }
                    }
                    // if(s>5){
                    //     break;
                    // }
                    
                }
                node.AddChoice("noweapons","Not really, no.",amount>0?"That's a pity.":"That's sensible. Weapons are dangerous.",amount>0?-1:1);*/
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
                    if(item.Equipped.GetPart<MeleeWeapon>() != null && item.Equipped.GetPart<MeleeWeapon>().Skill == this.wantedType){
                        return new acegiak_RomancePreferenceResult(amount,(amount > 0?Romancable.ParentObject.GetVerb("like"):Romancable.ParentObject.GetVerb("dislike"))+" your "+item.Equipped.DisplayNameOnly);
                    }
                    if(item.Equipped.GetPart<MissileWeapon>() != null && item.Equipped.GetPart<MissileWeapon>().Skill == this.wantedType){
                        return new acegiak_RomancePreferenceResult(amount,(amount > 0?Romancable.ParentObject.GetVerb("like"):Romancable.ParentObject.GetVerb("dislike"))+" your "+item.Equipped.DisplayNameOnly);
                    }
                }
            }
            return null;
        }



        public override string GetStory(acegiak_RomanceChatNode node, HistoricEntitySnapshot entity){
            var vars = new Dictionary<string, string>();
            vars["*type*"]   = wantedType;
            string storyTag = ((amount > 0) ?
                    "<spice.eros.opinion.weapon.like.story.!random>" :
                    "<spice.eros.opinion.weapon.dislike.story.!random>");
            while(this.tales.Count < 5){
                SetSampleObject(vars, exampleObject());
                this.tales.Add(//"  &K"+storyTag.Substring(1,storyTag.Count()-2)+"&y\n"+
                    acegiak_RomanceText.ExpandString(
                    storyTag, entity, vars));
            }
            return tales[Stat.Rnd2.Next(tales.Count)];
        }

        public override string getstoryoption(string key){
            var vars = new Dictionary<string, string>();
            vars["*type*"]   = wantedType;
            SetSampleObject(vars, exampleObject());
            return acegiak_RomanceText.ExpandString(
                "<spice.eros.opinion.weapon." + ((amount > 0) ? "like." : "dislike.") + key + ".!random>",
                vars);
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