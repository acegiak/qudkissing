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

	public class acegiak_MealBoon: acegiak_RomanceBoon
	{
        CookingRecipe reward;
        acegiak_Romancable Romancable = null;


        public CookingRecipe Reward(){
            if(reward == null){
                
                    if(Romancable.ParentObject.GetPart<acegiak_Romancable>() == null){
                        //IPart.AddPlayerMessage("no romancable");
                        return null;
                    }
                    acegiak_FoodPreference food = (acegiak_FoodPreference)Romancable.preferences.Where(b=>b is acegiak_FoodPreference).FirstOrDefault();
                    if(food == null){
                        //IPart.AddPlayerMessage("no food preference");
                        return null;
                    }
                    GameObject G = food.exampleObject();
                    if(G == null){
                        //IPart.AddPlayerMessage("no example object");
                        return null;
                    }
                    List<GameObject> GList = new List<GameObject>();
                    GList.Add(G);
                    reward = CookingRecipe.FromIngredients(GList);
                

            }
            return reward;
        }
        public acegiak_MealBoon(acegiak_Romancable romancable){
            Romancable = romancable;
        }

        public bool BoonPossible(GameObject talker){
            return Romancable != null;
        }

        public bool BoonReady(GameObject player){
            if(Reward() == null){
                return false;
            }
            return this.Romancable.ParentObject.pBrain.GetFeeling(player) > 60;
        }

        public acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            node.Text = "Are you hungry? I'd like to make you "+Reward().GetDisplayName()+".";

            node.AddChoice("End","Thankyou! [Eat "+Reward().GetDisplayName()+"].","You're very welcome.",-30,delegate(){
				IPart.PlayUISound("Human_Eating");
                Reward().ApplyEffectsTo(XRLCore.Core.Game.Player.Body);
                Popup.ShowBlock("You eat "+this.Romancable.ParentObject.the+this.Romancable.ParentObject.DisplayNameOnly+"'s " + Reward().GetDisplayName() + "!");
                this.reward = null;
            });
            node.AddChoice("rejectgift","No thankyou.","Oh I'm sorry. That makes sense.",-30);
            return node;
        }

        public static void GiveGift(){

        }

    }
}