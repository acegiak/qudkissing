using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.Rules;
using XRL.World;
using XRL.World.Encounters;
using Qud.API;
using HistoryKit;
using System.Linq;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_PatriotPreference : acegiak_RomancePreference
	{
        HistoricEntitySnapshot village = null;
        FactionInfo faction = null;
        float amount = 0;
        acegiak_Romancable Romancable = null;

        List<string> tales = new List<string>();



        public acegiak_PatriotPreference(acegiak_Romancable romancable){
            Romancable = romancable;


            List<HistoricEntity> villages = HistoryAPI.GetVillages();
            // .Name.Contains("villagers")
            int high = 0;
            HistoricEntitySnapshot village = null;
            foreach (KeyValuePair<string, int> item in  romancable.ParentObject.pBrain.FactionMembership)
			{
                if(item.Key.Contains("villagers")){
                    if(item.Value > high && villages.Select(v=>v.GetCurrentSnapshot()).Where(v=>item.Key.Contains(v.Name)).Count()>0){
                        village = villages.Select(v=>v.GetCurrentSnapshot()).Where(v=>item.Key.Contains(v.Name)).FirstOrDefault();
                        high = item.Value;
                        faction = Factions.FactionList[item.Key];
                    }
                }
			}

			

            amount = (float)((Stat.Rnd2.NextDouble()*1.5)-0.5);
            
            if(village == null || faction == null){
                throw new Exception("Not a villager.");
            }
            //IPart.AddPlayerMessage("They "+(amount>0?"like":"dislike")+" "+this.interestedFaction);

        }

        string factionName(){
            return faction.getFormattedName();
        }
        string townName(){
            return village.Name;
        }

        public List<string> goods (){
            List<string> good = village.GetList("sacredThings");
            good.Add(village.GetProperty("defaultSacredThing"));
            good.Add(village.GetProperty("signatureItem"));
            good.Add(village.GetProperty("signatureHistoricObjectName"));
            good.RemoveAll(r=>r==null);
            return good;
        }

        public List<string> bads(){
            List<string> bad = village.GetList("profaneThings");
            bad.Add(village.GetProperty("defaultProfaneThing"));
            bad.RemoveAll(r=>r==null);
            return bad;
        }

        public string randomGood(){
            string element = goods().GetRandomElement();
            GameObject GO = GameObject.create(element);
            if(GO == null){
                return element;
            }
            return GO.a+GO.DisplayNameOnly;
        }
        public string randomBad(){
            string element = bads().GetRandomElement();
            GameObject GO = GameObject.create(element);
            if(GO == null){
                return element;
            }
            return GO.a+GO.DisplayNameOnly;
        }

        public acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
            float retamount = 0;
            string retexplain = "";


            if(goods().Contains(gift.GetBlueprint().Name)){
                retamount += this.amount;
                retexplain = (amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.DisplayNameOnly+"'s &Y sacredness.";
            }
            if(bads().Contains(gift.GetBlueprint().Name)){
                retamount += this.amount*-1f;

                retexplain = (amount < 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.ShortDisplayName+"'s&Y profanity.";
            }
            
            if(retamount != 0 || retexplain != ""){
                return new acegiak_RomancePreferenceResult(retamount,retexplain);
            }
            // if(getType(gift) == wantedType){
            //     return new acegiak_RomancePreferenceResult(amount,(amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.ShortDisplayName+"&Y.");
            // }
            return null;
        }



        public acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            string bodytext = "whoah";

            float g = (float)Stat.Rnd2.NextDouble();

            if(g<0.5){
                bodytext = "<What do you think of|How do you feel about|What is your opinion of> "+randomGood()+"?";
                node.AddChoice("likethem","I approve.",amount>0?"They are lovely, aren't they?":"Oh, You must keep awful company.",amount>0?1:-1);
                node.AddChoice("dislikethem","It is terrible.",amount>0?"That's very judgemental":"Aren't they horrible?",amount>0?-1:1);
            }else
            if(g<1){
                bodytext = "<What do you think of|How do you feel about|What is your opinion of> "+randomBad()+"?";
                node.AddChoice("likethem","I approve.",amount>0?"Such blasphemy!":"They aren't so bad, are they?.",amount>0?1:-1);
                node.AddChoice("dislikethem","It is terrible.",amount>0?"Isn't it horrid?":"Then you are a fool.",amount>0?-1:1);
            }

            if(Romancable != null){
                node.Text = node.Text+"\n\n"+Romancable.GetStory();
            }
            node.Text = node.Text+"\n\n"+bodytext;

            return node;
        }


        public acegiak_RomancePreferenceResult DateAssess(GameObject Date, GameObject DateObject){
            // if(DateObject.GetPart<Pettable>() != null){
            //     return new acegiak_RomancePreferenceResult(0,Romancable.ParentObject.The+Romancable.ParentObject.ShortDisplayName+Romancable.ParentObject.GetVerb("pet")+DateObject.the+DateObject.ShortDisplayName+".");
            // }
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
                        "Once, a ==example== <gave me|showed me|told me about> "+Romancable.storyoptions("goodobject",item.a+item.ShortDisplayName)+".",

                        "Once, I had a dream about a ==examplebad==. When I woke up "+Romancable.storyoptions("goodthinghappen","I was drenched in sweat")+"!",
                        "Once, a ==examplebad== <attacked|tried to kill me> me with "+Romancable.storyoptions("badweapon",item.a+item.ShortDisplayName)+"."
                    });
                }else{
                    GameObject item = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("MeleeWeapon"));
                    item.MakeUnderstood();
                    Stories = new List<string>(new string[] {
                        "Once, I had a dream about a ==example==. When I woke up "+Romancable.storyoptions("goodthinghappen","I was drenched in sweat")+"!",
                        "Once, a ==example== <attacked|tried to kill me> me with "+Romancable.storyoptions("badweapon",item.a+item.ShortDisplayName)+".",

                        "Once, I had a dream about a ==examplebad==. When I woke "+Romancable.storyoptions("goodthinghappen","I saw a rainbow")+"!",
                        "Once, a ==examplebad== <gave me|showed me|told me about> "+Romancable.storyoptions("goodobject",item.a+item.ShortDisplayName)+"."
                    });
                }
                this.tales.Add(Stories[Stat.Rnd2.Next(0,Stories.Count-1)].Replace("==example==",randomGood()).Replace("==examplebad==",randomBad()));
            }
            return tales[Stat.Rnd2.Next(tales.Count)];

        }
        public string getstoryoption(string key){
            // GameObject GO = EncountersAPI.GetACreatureFromFaction(this.interestedFaction);
            // if(GO != null){
                if(key == "goodobject" && this.amount > 0){
                    return randomGood();
                }
                if(key == "badobject" && this.amount > 0){
                    return randomBad();
                }
                if(key == "badobject" && this.amount < 0){
                    return randomGood();
                }
                if(key == "goodobject" && this.amount < 0){
                    return randomBad();
                }
                // if(key == "goodthinghappen" && this.amount > 0){
                //     return "I met "+GO.a+GO.ShortDisplayName;
                // }
                // if(key == "badthinghappen" && this.amount < 0){
                //     return "I met "+GO.a+GO.ShortDisplayName;
                // }
            // }
            return null;
        }
        

    }
}