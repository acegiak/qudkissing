using Qud.API;
using System;
using System.Collections.Generic;
using System.Reflection;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Conversations
{
	[Serializable]
	public class acegiak_RomanceChatChoice : Choice
	{

        public float OpinionAmount = 0;
        public string ResponseText = "huh?";
        public delegate void acegiak_ChoiceAction();

        public acegiak_ChoiceAction choiceAction = null;

        public string action = null;



		public override void Entered()
		{
			
			base.Entered();
                if(The.Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) < 10 && The.Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) + (int)Math.Floor(this.OpinionAmount) >= 10){
                    this.ResponseText = this.ResponseText+"\n\n *"+The.Speaker.ShortDisplayName+" smiles.*";
                }else
                if(The.Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) < 55 && The.Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) + (int)Math.Floor(this.OpinionAmount) >= 55){
                    this.ResponseText = this.ResponseText+"\n\n *"+The.Speaker.ShortDisplayName+" grins at you.*";
                }else
                if(The.Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) >= 5 && The.Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) + (int)Math.Floor(this.OpinionAmount) < 5){
                    this.ResponseText = this.ResponseText+"\n\n *"+The.Speaker.ShortDisplayName+" frowns.*";
                }
				this.OpinionAmount = this.OpinionAmount*2f;
                //if(The.Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) + (int)Math.Floor(this.OpinionAmount) > 0){
                    The.Speaker.pBrain.AdjustFeeling(XRLCore.Core.Game.Player.Body,(int)Math.Floor(this.OpinionAmount));
					The.Speaker.GetPart<acegiak_Romancable>().storedFeeling = The.Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body);
                //}
                if(this.OpinionAmount >= 1){
                    The.Speaker.GetPart<acegiak_Romancable>().patience += 0;
                }
                if(this.OpinionAmount <= -1){
                    The.Speaker.GetPart<acegiak_Romancable>().patience -= 2;
                }
				The.Speaker.GetPart<acegiak_Romancable>().DisplayNext = this.ResponseText;
				IPart.AddPlayerMessage("DEBUG:("+The.Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body).ToString()+"rep) CHOSE "+ID+" RESPONSE: "+ResponseText);
                if(choiceAction != null){
                    choiceAction.Invoke();
                }
		}



    }
}