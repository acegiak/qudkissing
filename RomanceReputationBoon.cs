using System;
using XRL.Core;
using XRL.UI;
using XRL.Rules;
using XRL.World.Capabilities;

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
	public class acegiak_ReputationBoon: acegiak_RomanceBoon
	{


        public acegiak_ReputationBoon(acegiak_Romancable romancable){
            Romancable = romancable;
        }

        public override bool BoonPossible(GameObject talker){
            return true;
        }

        public override bool BoonReady(GameObject player){
            int? difference = DifficultyEvaluation.GetDifficultyRating(Romancable.ParentObject,player);
            if(difference == null){difference = 0;}
            return this.Romancable.ParentObject.pBrain.GetFeeling(player) > 65 + difference;
        }

        public override acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            node.Text = "I hope you don't mind, I have been telling some friends and people I know about how great you are. Is that ok?";

            node.AddChoice("End","That is absolutely ok. [Accept "+this.Romancable.ParentObject.pBrain.GetPrimaryFaction()+" reputation].","Excellent! We will have many adventures together!",-30,delegate(){
                    The.Game.PlayerReputation.modify(this.Romancable.ParentObject.pBrain.GetPrimaryFaction(),5,true);});
            node.AddChoice("rejectgift","I'd really rather you didn't.'.","Oh I'm sorry. That makes sense.",-5);
            return node;
        }


    }
}