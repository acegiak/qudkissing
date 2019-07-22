using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.Rules;
using XRL.World.Encounters;
using Qud.API;
using System.Linq;
using XRL.World.Parts.Effects;
using System.Text.RegularExpressions;
using HistoryKit;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_FoodPreference : acegiak_RomancePreference
	{
        public static bool cooked = false;
        string wantedType = "tasty";
        public float amount = 0;


        string ExampleName = "corpse";
        List<string> tales = new List<string>();




        public acegiak_FoodPreference(acegiak_Romancable romancable){
            GameObject sample = EncountersAPI.GetAnObject((GameObjectBlueprint b) => b.HasPart("PreparedCookingIngredient"));
            if(sample == null){
                sample = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("Food"));
            }
            
            sample.MakeUnderstood();
            this.wantedType = getType(sample);

            this.ExampleName = sample.DisplayNameOnly;
            Romancable = romancable;
            amount = (float)((Stat.Rnd2.NextDouble()*2)-0.9);
            //IPart.AddPlayerMessage("They "+(amount>0?"like":"dislike")+" "+this.wantedType);
        }
        string normalisename(string name){
            if(name == null){
                name = "";
            }
            return Regex.Replace(name.ToLower().Replace("elder","").Replace("less","").Replace("lowtier","").Replace("minor","").Replace("hightier","").Replace("cookingdomain","").Replace(" ",""),"_[^_]*$","");
        }

        string getType(GameObject sample){
            sample.MakeUnderstood();
            if(sample.GetPart<PreparedCookingIngredient>() != null){
                return normalisename(sample.GetPart<PreparedCookingIngredient>().type);
            }else if(sample.GetPart<PreservableItem>() != null){
                GameObject newsample = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf(sample.GetPart<PreservableItem>().Result));
                if(newsample != null && sample.GetPart<PreparedCookingIngredient>() != null){
                    return normalisename(newsample.GetPart<PreparedCookingIngredient>().type);
                }
            }
            if(sample.GetPart<Food>() != null){
                return sample.GetPart<Food>().Satiation;
            }
            return "tasty";
        }

        public GameObject exampleObject(){

            GameObject sample = null;
            
            sample = EncountersAPI.GetAnObject(
                (GameObjectBlueprint b) => 
                normalisename(b.GetPartParameter("PreparedCookingIngredient","type")) == this.wantedType 
                || normalisename(b.GetPartParameter("Food","Satiation")) == this.wantedType 
                // || b.GetPartParameter("PreservableItem","Result")==null?false:
                // GameObjectFactory.Factory.CreateSampleObject(b.GetPartParameter("PreservableItem","Result"))==null?false:
                // GameObjectFactory.Factory.CreateSampleObject(b.GetPartParameter("PreservableItem","Result")).GetPart<PreparedCookingIngredient>()==null?false:
                // normalisename(GameObjectFactory.Factory.CreateSampleObject(b.GetPartParameter("PreservableItem","Result")).GetPart<PreparedCookingIngredient>().type) == this.wantedType
            );
            if(sample == null){
                //IPart.AddPlayerMessage("Can't find food for:"+this.wantedType);
                return null;
            }
            
            sample.MakeUnderstood();
            return sample;
        }


        public override acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
            float retamount = 0;
            string retexplain = "";
            if(getType(gift) == wantedType){
                return new acegiak_RomancePreferenceResult(amount,(amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.DisplayNameOnly+"&Y.");
            }
            return null;
        }



        public override acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            string bodytext = "whoah";

            float g = (float)Stat.Rnd2.NextDouble();

            var vars = new Dictionary<string, string>();
            if(g<0.3 ){
                //SetSampleObject(vars, exampleObject());
                GameObject sample = exampleObject();
                bodytext = "Have you ever eaten a "+sample.DisplayNameOnly+"?";
                node.AddChoice("yesseen","Oh yes, it was delicious.",amount>0?"Wow, how excellent!":"Oh, I don't think I would agree.",amount>0?1:-1);
                node.AddChoice("yesseendislike","I have but it was disgusting.",amount>0?"Oh, I guess we have different tastes.":"I agree, I ate one once and didn't like it.",amount>0?-1:1);
                node.AddChoice("notseen","No, I've not seen such a thing.",amount>0?"Oh, that's disappointing.":"That's probably for the best.",amount>0?-1:1);
            }else if(g<0.6){
                //SetSampleObject(vars, exampleObject());

                GameObject sample = exampleObject();

                if(sample == null){
                    bodytext = "I heard that some foods can be preserved over a campfire.";
                }else{
                    sample.MakeUnderstood();
                    if(sample.GetPart<PreparedCookingIngredient>() != null){
                        bodytext = "Did you know that <cooking|brewing|broiling|frying> "+sample.DisplayNameOnly+" can sometimes make <a meal|food> that bestows ";
                        bodytext += GameObjectFactory.Factory.CreateSampleObject("ProceduralCookingIngredient_"+sample.GetPart<PreparedCookingIngredient>().type).GetTag("Description");
                        bodytext += " effects on whoever eats it?";
                    }else
                    if(sample.GetPart<PreservableItem>() != null){
                        bodytext = "I've heard that "+sample.DisplayNameOnly+" can be <preserved|cooked|made> into "+GameObjectFactory.Factory.CreateSampleObject(sample.GetPart<PreservableItem>().Result).DisplayNameOnly+".";
                    }else
                    if(sample.GetPart<Food>() != null){
                        bodytext = "I hear some people eat "+sample.DisplayNameOnly+" as a "+sample.GetPart<Food>().Satiation+".";
                    }
                }
               
                node.AddChoice("approve","Yes, it's amazing.",amount>0?"How fascinating!":"It sounds horrible!",amount>0?1:-1);
                node.AddChoice("disprove","That is, unfortunately, true.",amount>0?"Oh? I think it sounds wonderful.":"Yes it seems quite unsettling.",amount>0?-1:1);
                node.AddChoice("disagree","I'm not sure that is true.","Oh, isn't it? How odd.",-1);
            }else{
                bodytext = "Do you have any <interesting|tasty|exotic> food?";
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
                    PreparedCookingIngredient mw = null;
                    mw = GO.GetPart<PreparedCookingIngredient>();
                    PreservableItem rw = null;
                    rw = GO.GetPart<PreservableItem>();
                    if((rw != null || mw != null) && GO.GetPart<Salve_Tonic_Applicator>()==null){
                        if(Romancable.assessGift(GO,XRLCore.Core.Game.Player.Body).amount > 0){
                            node.AddChoice("food"+c.ToString(),"I have <this|a> "+GO.DisplayName+".",amount>0?"Wow, that looks delicious!":"Oh, that's disgusting!",amount>0?2:-1);
                            s++;
                        }else{
                            node.AddChoice("food"+c.ToString(),"I have <this|a> "+GO.DisplayName+".",amount>0?"Oh, is that all?":"Oh, I guess that IS edible.",amount>0?0:0);
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
                node.Text = node.Text+"\n\n"+Romancable.GetStory(node);
            }
            node.Text = node.Text+"\n\n"+bodytext;

            return node;
        }


        public override acegiak_RomancePreferenceResult DateAssess(GameObject Date, GameObject DateObject){
            if(DateObject.GetPart<Campfire>() != null && (cooked || DateObject.GetPart<Campfire>().Cook())){
                cooked = true;
                float count = 0.25f;
                foreach(Effect effect in XRLCore.Core.Game.Player.Body.Effects){
                    if(effect is ProceduralCookingEffect){
                        foreach(ProceduralCookingEffectUnit unit in ((ProceduralCookingEffect)effect).units){
                            //classname += normalisename(unit.GetType().Name)+"\n";
                            if(normalisename(unit.GetType().Name) == this.wantedType){
                                count += this.amount;
                            }
                        }
                    }
                }

                if(this.wantedType == "snack"){
                    count += 0.25f;
                }

                if(this.wantedType == "meal"){
                    count += 0.5f;
                }

                string message = "";
                if(amount >0.3){
                    message = Romancable.ParentObject.The+Romancable.ParentObject.DisplayNameOnly+" greatly"+Romancable.ParentObject.GetVerb("enjoy")+" your cooking.";
                }else
                if(amount >0.1){
                    message = Romancable.ParentObject.The+Romancable.ParentObject.DisplayNameOnly+Romancable.ParentObject.GetVerb("appreciate")+" your cooking.";
                }else
                if(amount <=0.1){
                    message = Romancable.ParentObject.The+Romancable.ParentObject.DisplayNameOnly+Romancable.ParentObject.GetVerb("dislike")+" your "+message+" cooking.";
                }
                return new acegiak_RomancePreferenceResult(count,message);
            }
            return null;
        }



        public override string GetStory(acegiak_RomanceChatNode node, HistoricEntitySnapshot entity){
            var vars = new Dictionary<string, string>();
            string storyTag = ((amount > 0) ?
                "<spice.eros.opinion.food.like.story.!random>" :
                "<spice.eros.opinion.food.dislike.story.!random>");
            while(this.tales.Count < 5){
                SetSampleObject(vars, exampleObject());
                this.tales.Add(//"  &K"+storyTag.Substring(1,storyTag.Count()-2)+"&y\n"+
                    acegiak_RomanceText.ExpandString(
                    storyTag, entity, vars));
            }
            return tales[Stat.Rnd2.Next(tales.Count)];
        }

        public override string getstoryoption(string key){
            GameObject GO = exampleObject();
            if(GO == null){
                return null;
            }
            
            var vars = new Dictionary<string, string>();
            SetSampleObject(vars, GO);
            return acegiak_RomanceText.ExpandString(
                "<spice.eros.opinion.food." + ((amount > 0) ? "like." : "dislike.") + key + ".!random>",
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