using System;
using XRL.Core;
using XRL.UI;
using XRL.Rules;
using XRL.World.Skills.Cooking;
using System.Collections.Generic;
using System.Linq;
namespace XRL.World.Parts
{
    // public class acegiak_RomancePreferenceResult{
    //     public string explanation;
    //     public float amount;
    //     public acegiak_RomancePreferenceResult(float amount,string explanation){
    //         this.amount = amount;
    //         this.explanation = explanation;
    //     }
    // }

	[Serializable]
	public class acegiak_MealBoon: acegiak_RomanceBoon
	{

        [NonSerialized]
        CookingRecipe reward;



        public CookingRecipe Reward(){
            if(reward == null){
                
                    if(Romancable.ParentObject.GetPart<acegiak_Romancable>() == null){
                        //IPart.AddPlayerMessage("no romancable");
                        return null;
                    }

                    List<GameObject> foods = Romancable.preferences
                        .Where(b=>b is acegiak_FoodPreference && ((acegiak_FoodPreference)b).amount > 0).Select(b=>(acegiak_FoodPreference)b)
                        .OrderBy(o=>Stat.Rnd2.NextDouble())
                        .Select(b=>b.exampleObject())
                        .ToList();
					
					// IPart.AddPlayerMessage("food options: "+foods.Select(c=>c.DisplayNameOnly).Aggregate(string.Empty,(a,b)=>a+", "+b));

                    
                    if(foods.Count() <= 0){
                        return null;
                    }
                    
                    reward = CookingRecipe.FromIngredients(foods,null,Romancable.ParentObject.DisplayNameOnly.Split(',')[0]);
            }
            return reward;
        }
        public acegiak_MealBoon(acegiak_Romancable romancable){
            Romancable = romancable;
        }

        public override bool BoonPossible(GameObject talker){
			if (Romancable == null){
				return false;
			}
			 if(Reward() == null){
                return false;
            }
			return false;
        }

        public override bool BoonReady(GameObject player){
            if(Reward() == null){
                return false;
            }
            return this.Romancable.ParentObject.pBrain.GetFeeling(player) > 60;
        }

        public override acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            node.Text = "Are you hungry? I'd like to make you "+Reward().GetDisplayName()+".";

            node.AddChoice("End","Thankyou! [Eat "+Reward().GetDisplayName()+"].","You're very welcome.",-5,delegate(){
				IPart.PlayUISound("Human_Eating");
                Reward().ApplyEffectsTo(XRLCore.Core.Game.Player.Body);
                Popup.ShowBlock("You eat "+this.Romancable.ParentObject.the+this.Romancable.ParentObject.DisplayNameOnly+"'s " + Reward().GetDisplayName() + "!");
                this.reward = null;
            });
            node.AddChoice("rejectgift","No thankyou.","Oh I'm sorry. That makes sense.",-5);
            return node;
        }
    }
}