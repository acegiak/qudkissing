using System;
using XRL.Core;
using XRL.UI;
using XRL.Rules;

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
	public class acegiak_GiftBoon: acegiak_RomanceBoon
	{
        
        [NonSerialized]
        GameObject reward;



        public GameObject Reward(){
            if(reward == null){
                
                if(Romancable.ParentObject.GetPart<Inventory>() != null && Romancable.ParentObject.GetPart<Inventory>().GetObjectCount() > 0){
                    reward = Romancable.ParentObject.GetPart<Inventory>().GetObjects()[Stat.Rnd2.Next(Romancable.ParentObject.GetPart<Inventory>().GetObjectCount())];
                }

            }
            return reward;
        }
        public acegiak_GiftBoon(acegiak_Romancable romancable){
            Romancable = romancable;
        }

        public override bool BoonPossible(GameObject talker){
			if(Reward() != null){
				return true;
			}
            return false;
        }

        public override bool BoonReady(GameObject player){
            if(Reward() == null || this.Romancable == null || this.Romancable.ParentObject == null || this.Romancable.ParentObject.pBrain == null || player == null){
                return false;
            }
            float diff = 0;
            if(Reward().GetPart<Commerce>() != null){
                diff = (float)Reward().GetPart<Commerce>().Value;
            }
            return this.Romancable.ParentObject.pBrain.GetFeeling(player) > 60+diff;
        }

        public override acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            node.Text = "I hope you don't mind, but I got you a gift. It's "+Reward().a+Reward().DisplayNameOnly+".";

            node.AddChoice("acceptgift","Thankyou! [Accept "+Reward().the+Reward().DisplayNameOnly+"].","You're very welcome.",-5,delegate(){Reward().ForceUnequipAndRemove();XRLCore.Core.Game.Player.Body.GetPart<Inventory>().AddObject(Reward()); Popup.ShowBlock("You receive " + Reward().a + Reward().DisplayNameOnly + "!");this.reward = null;});
            node.AddChoice("rejectgift","I couldn't possibly accept such a gift.","Oh I'm sorry. That makes sense.",-5);
            return node;
        }
    }
}