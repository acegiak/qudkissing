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
	public class acegiak_FollowBoon: acegiak_RomanceBoon
	{


        public acegiak_FollowBoon(acegiak_Romancable romancable){
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
            node.Text = "I have a suggestion: I would like to join you and follow you on your adventures.";

            node.AddChoice("End","Very well. [Accept "+this.Romancable.ParentObject.the+this.Romancable.ParentObject.DisplayNameOnly+" into your party].","Excellent! We will have many adventures together!",-30,delegate(){
                    this.Romancable.ParentObject.GetPart<Brain>().BecomeCompanionOf(XRLCore.Core.Game.Player.Body);
                    this.Romancable.ParentObject.GetPart<Brain>().IsLedBy(XRLCore.Core.Game.Player.Body);
                    this.Romancable.ParentObject.GetPart<Brain>().Goals.Clear();
                    Popup.Show(this.Romancable.ParentObject.The+this.Romancable.ParentObject.DisplayNameOnly+" joins your party!");});
            node.AddChoice("rejectgift","I'm sorry but you cannot join me.","Oh I'm sorry. That makes sense.",-5);
            return node;
        }


    }
}