using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.Encounters;
using Qud.API;
using System.Linq;

namespace XRL.World.Parts
{
	[Serializable]
	public class FoodPreference : acegiak_RomancePreference
	{
        string wantedType = "meat";
        float amount = 0;
        acegiak_Romancable Romancable = null;

        string ExampleName = "corpse";
        List<string> tales = new List<string>();


        Dictionary<string, string> presentable = new Dictionary<string, string>()
        {
            { "Bow", "bow" },
            { "Pistol", "pistol" },
            { "HeavyWeapons", "heavy weapon" },
            { "Rifle", "rifle" } //also bows :(
        };

        string normalisename(string name){
            return name.Replace("elder","").Replace("LowTier","").Replace("HighTier","");
        }

        string getType(GameObject sample){
            if(sample.GetPart<PreparedCookingIngredient>() != null){
                return normalisename(sample.GetPart<PreparedCookingIngredient>().type);
            }else if(sample.GetPart<PreservableItem>() != null){
                GameObject newsample = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf(sample.GetPart<PreservableItem>().Result));
                if(newsample != null && sample.GetPart<PreparedCookingIngredient>() != null){
                    return normalisename(newsample.GetPart<PreparedCookingIngredient>().type);
                }
            }
            return "meat";
        }


        public FoodPreference(acegiak_Romancable romancable){
            GameObject sample = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("Food"));
            this.wantedType = getType(sample);

            this.ExampleName = sample.ShortDisplayName;
            Romancable = romancable;
            Random r = new Random();
            amount = (float)(r.NextDouble()*2-0.9);
        }

        public acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
            float retamount = 0;
            string retexplain = "";
            if(getType(gift) == wantedType){
                return new acegiak_RomancePreferenceResult(amount,(amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.ShortDisplayName+"&Y.");
            }
            return null;
        }



        public acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            string bodytext = "whoah";

			Random r = new Random();
            float g = (float)r.NextDouble();
            bool haskey = false;
            foreach(var item in presentable){
                if(item.Key == wantedType){
                    haskey = true;
                    break;
                }
            }

            if(g<0.3 && haskey){
                bodytext = "Do you ever think about just shooting people?";
                node.AddChoice("yeahcleave","Oh yes, quite often.",amount>0?"Oh good. I thought I was the only one.":"Really? That is troubling.",amount>0?1:-1);
                node.AddChoice("nahcleave","No, that sounds bad.",amount>0?"Oh, I guess it is. Sorry.":"It does, doesn't it? How scary!",amount>0?-1:1);
            }else if(g<0.6 && haskey){
                bodytext = "How do you like to slay your enemies?";
                foreach(var item in presentable){
                    if(item.Key == wantedType){
                        node.AddChoice(item.Key,"I like shooting them with a "+presentable[item.Key]+".",amount>0?"Me too!":"That's quite violent, isn't it?",amount>0?1:-1);
                    }else{
                        node.AddChoice(item.Key,"I like shooting them with a "+presentable[item.Key]+".",amount>0?"That sounds unpleasant.":"That's quite violent, isn't it?",amount>0?1:-1);
                    }
                }
                node.AddChoice("notmelee","I to attack them up close in melee combat.",amount>0?"That sounds horrific.":"That sounds very brave.",amount>0?-1:1);
            }else{
                bodytext = "Do you have any interesting food?";
                Inventory part2 = XRLCore.Core.Game.Player.Body.GetPart<Inventory>();
                int c = 0;
                int s = 0;
                foreach(GameObject GO in part2.GetObjects())
                {
                    PreparedCookingIngredient mw = null;
                    mw = GO.GetPart<PreparedCookingIngredient>();
                    PreservableItem rw = null;
                    rw = GO.GetPart<PreservableItem>();
                    if(rw != null || mw != null){
                        if(getType(GO) == wantedType){
                            node.AddChoice("food"+c.ToString(),"I have this "+GO.DisplayName+".",amount>0?"Wow, that looks delicious!":"Oh, that's disgusting!",amount>0?1:-1);
                            s++;
                        }else{
                            node.AddChoice("food"+c.ToString(),"I have this "+GO.DisplayName+".",amount>0?"Oh, is that all?":"Oh, I guess that IS edible.",amount>0?0:0);
                            s++;
                        }
                    }
                    if(s>5){
                        break;
                    }
                    
                }
                node.AddChoice("nofoods","Not really, no.",amount>0?"That's a pity.":"I'm sure you find enough to get by.",amount>0?-1:0);
            }

            if(Romancable != null){
                node.Text = node.Text+"\n\n"+Romancable.GetStory();
            }
            node.Text = node.Text+"\n\n"+bodytext;

            return node;
        }


        public string GetStory(){
            Random r = new Random();
            if(tales.Count < 3){
                List<string> Stories = null;
                if(amount>0){
                    Stories = new List<string>(new string[] {
                        "Once, I had a dream about a eating a ==sample==. It was delicious."
                    });
                }else{
                    Stories = new List<string>(new string[] {
                        "Once, I had a dream about a eating a ==sample==. It was disgusting."
                    });
                }
                tales.Add(Stories[r.Next(0,Stories.Count-1)].Replace("==type==",presentable[wantedType]).Replace("==sample==",ExampleName));
            }
              


            return tales[r.Next(0,tales.Count-1)];


        }



    }
}