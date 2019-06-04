using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.Rules;
using XRL.World.Encounters;
using Qud.API;
using System.Linq;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_MissilePreference : acegiak_RomancePreference
	{
        string wantedType = "Rifle";
        float amount = 0;
        acegiak_Romancable Romancable = null;

        string ExampleName = "Musket";
        List<string> tales = new List<string>();


        Dictionary<string, string> presentable = new Dictionary<string, string>()
        {
            { "Bow", "bow" },
            { "Pistol", "pistol" },
            { "HeavyWeapons", "heavy weapon" },
            { "Rifle", "rifle" } //also bows :(
        };

        public acegiak_MissilePreference(acegiak_Romancable romancable){
            GameObject sample = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("BaseMissileWeapon"));
            sample.MakeUnderstood();
            this.wantedType =  sample.GetPart<MissileWeapon>().Skill;
            this.ExampleName = sample.ShortDisplayName;
            Romancable = romancable;
            amount = (float)(Stat.Rnd2.NextDouble()*2-0.9);
            //IPart.AddPlayerMessage("They "+(amount>0?"like":"dislike")+" "+this.wantedType);

        }

        public string exampleObjectName(){
            GameObject sample = EncountersAPI.GetAnObject((GameObjectBlueprint b) => b.InheritsFrom("BaseMissileWeapon") && b.GetPartParameter("MissileWeapon","Skill") == this.wantedType);
            sample.MakeUnderstood();
            return sample.ShortDisplayName;
        }


        public acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
            float retamount = 0;
            string retexplain = "";
            if(gift.GetPart<MissileWeapon>() != null && gift.GetPart<MissileWeapon>().Skill == wantedType){
                return new acegiak_RomancePreferenceResult(amount,(amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.ShortDisplayName+"&Y.");
            }
            return null;
        }



        public acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            string bodytext = "whoah";

            float g = (float)Stat.Rnd2.NextDouble();
            bool haskey = false;
            foreach(var item in presentable){
                if(item.Key == wantedType){
                    haskey = true;
                    break;
                }
            }

            if(g<0.2 && haskey){
                bodytext = "Do you ever think about just shooting people?";
                node.AddChoice("yeahcleave","Oh yes, quite often.",amount>0?"Oh good. I thought I was the only one.":"Really? That is troubling.",amount>0?1:-1);
                node.AddChoice("nahcleave","No, that sounds bad.",amount>0?"Oh, I guess it is. Sorry.":"It does, doesn't it? How scary!",amount>0?-1:1);
            }else if(g<0.4 && haskey){
                bodytext = "How do you like to slay your enemies?";
                foreach(var item in presentable){
                    if(item.Key == wantedType){
                        node.AddChoice(item.Key,"I like shooting them with a "+presentable[item.Key]+".",amount>0?"Me too!":"That's quite violent, isn't it?",amount>0?1:-1);
                    }else{
                        node.AddChoice(item.Key,"I like shooting them with a "+presentable[item.Key]+".",amount>0?"That sounds unpleasant.":"That's quite violent, isn't it?",amount>0?1:-1);
                    }
                }
                node.AddChoice("notmelee","I to attack them up close in melee combat.",amount>0?"That sounds horrific.":"That sounds very brave.",amount>0?-1:1);
            }else if(g<0.6 && haskey){
                string sample = exampleObjectName();
                bodytext = "Have you ever seen a "+sample+"?";
                node.AddChoice("yesseen","Oh yes, I've seen a "+sample+". It was great.",amount>0?"Wow, how excellent!":"Oh, I don't think I would agree.",amount>0?1:-1);
                node.AddChoice("yesseendislike","I have but I wasn't impressed.",amount>0?"Oh, I guess we have different tastes.":"I agree, I saw one once and didn't like it.",amount>0?-1:1);
                node.AddChoice("notseen","No, I've not seen such a thing.",amount>0?"Oh, that's disappointing.":"That's probably for the best.",amount>0?-1:1);
            }else if(g<0.80){
                if(wantedType == "HeavyWeapons"){
                    bodytext = "I heard heavy ranged weapons often fire explosives!";}
                if(wantedType == "Rifle"){
                    bodytext = "I hear bows and rifles are very good for shooting folks at long range.";}
                if(wantedType == "Pistol"){
                    bodytext = "Did you know some folks weild a pistol in each hand?";}
               
                node.AddChoice("approve","Yes, it's amazing.",amount>0?"How fascinating!":"Oh, how scary.",amount>0?1:-1);
                node.AddChoice("disprove","That is, unfortunately, true.",amount>0?"Oh? I think it sounds very impressive.":"Yes it seems quite dangerous.",amount>0?-1:1);
                node.AddChoice("disagree","I'm not sure that is true.","Oh, isn't it? How odd.",-1);
            }else{
                bodytext = "Do you have any interesting weapons?";
                Inventory part2 = XRLCore.Core.Game.Player.Body.GetPart<Inventory>();
                int c = 0;
                int s = 0;
                foreach(GameObject GO in part2.GetObjects())
                {
                    MissileWeapon mw = null;
                    mw = GO.GetPart<MissileWeapon>();
                    if(GO.GetBlueprint().InheritsFrom("BaseMissileWeapon") && mw != null){
                        if(mw.Skill == wantedType){
                            node.AddChoice("weapon"+c.ToString(),"I have this "+GO.DisplayName+".",amount>0?"Wow, that's very interesting!":"Oh, is that all?",amount>0?1:-1);
                            s++;
                        }else{
                            node.AddChoice("weapon"+c.ToString(),"I have this "+GO.DisplayName+".",amount>0?"Oh, is that all?":"Hmm, that seems dangerous.",amount>0?0:-1);
                            s++;
                        }
                    }
                    if(s>5){
                        break;
                    }
                    MeleeWeapon rw = null;
                    rw = GO.GetPart<MeleeWeapon>();
                    if(GO.GetBlueprint().InheritsFrom("MeleeWeapon") &&rw != null && rw.Skill != null){
                        node.AddChoice("weapon"+c.ToString(),"I have this "+GO.DisplayName+"&Y.",amount>0?"Oh, is that all?":"Hmm, that seems dangerous.",amount>0?0:-1);
                        s++;
                    }
                    if(s>5){
                        break;
                    }
                    c++;
                    
                }
                node.AddChoice("noweapons","Not really, no.",amount>0?"That's a pity.":"That's sensible. Weapons are dangerous.",amount>0?-1:1);
            }

            if(Romancable != null){
                node.Text = node.Text+"\n\n"+Romancable.GetStory();
            }
            node.Text = node.Text+"\n\n"+bodytext;

            return node;
        }


        public string GetStory(){
                List<string> Stories = null;
                if(amount>0){
                    Stories = new List<string>(new string[] {
                        "Once, I had a dream about a ==sample== and then the next day I saw a rainbow.",
                        "I really love shooting my enemies.",
                        "I think I could probably make a ==sample==.",
                        "I think ==type==s are kind of neat.",
                        "You look like the kind of person that might carry a ==type==.",
                        "My friend used to carry a ==type==."
                    });
                }else{
                    Stories = new List<string>(new string[] {
                        "Once, I had a dream about a ==sample== and then the next day I got hit with a rock.",
                        "I worry about people shooting me with a ==type==.",
                        "A "+GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("Creature")).ShortDisplayName+" once shot me with a ==type==",
                        "I just can't feel safe around ==type==s.",
                        "You look like the kind of person that might carry a ==type==.",
                        "My greatest enemy used to carry a ==type==."
                    });
                }
                return Stories[Stat.Rnd2.Next(0,Stories.Count-1)].Replace("==type==",presentablec(wantedType)).Replace("==sample==",exampleObjectName());
            

        }

        string presentablec(string key){
            if(!presentable.ContainsKey(key)){
                return "?"+key;
            }else{
                return presentable[key];
            }
        }




    }
}