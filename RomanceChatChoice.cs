using Qud.API;
using System;
using System.Collections.Generic;
using System.Reflection;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World
{
	[Serializable]
	public class acegiak_RomanceChatChoice : ConversationChoice
	{

        public float OpinionAmount = 0;
        public string ResponseText = "huh?";
        public delegate void acegiak_ChoiceAction();

        public acegiak_ChoiceAction choiceAction = null;

        public string action = null;


		public override ConversationNode Goto(GameObject Speaker, bool peekOnly = false, Conversation conversation = null)
		{
            if(!peekOnly){
                if (action == "*Kiss")
                {
                    Speaker.FireEvent(Event.New("InvCommandKiss", "Owner", XRLCore.Core.Game.Player.Body));
                    return null;
                }
                if (action == "*Gift")
                {
                    Speaker.FireEvent(Event.New("InvCommandGift", "Owner", XRLCore.Core.Game.Player.Body));
                    return null;
                }
                if (action == "*Date")
                {
                    XRLCore.Core.Game.Player.Body.FireEvent(Event.New("InvCommandArrangeDate", "Object", Speaker));
                    return null;
                }
                if(Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) < 10 && Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) + (int)Math.Floor(this.OpinionAmount) >= 10){
                    this.ResponseText = this.ResponseText+"\n\n *"+Speaker.ShortDisplayName+" smiles.*";
                }else
                if(Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) < 55 && Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) + (int)Math.Floor(this.OpinionAmount) >= 55){
                    this.ResponseText = this.ResponseText+"\n\n *"+Speaker.ShortDisplayName+" grins at you.*";
                }else
                if(Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) >= 5 && Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) + (int)Math.Floor(this.OpinionAmount) < 5){
                    this.ResponseText = this.ResponseText+"\n\n *"+Speaker.ShortDisplayName+" frowns.*";
                }
                if(Speaker.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) + (int)Math.Floor(this.OpinionAmount) > 0){
                    Speaker.pBrain.AdjustFeeling(XRLCore.Core.Game.Player.Body,(int)Math.Floor(this.OpinionAmount));
                }
                if(this.OpinionAmount >= 1){
                    Speaker.GetPart<acegiak_Romancable>().patience += 0;
                }
                if(this.OpinionAmount <= -1){
                    Speaker.GetPart<acegiak_Romancable>().patience -= 2;
                }
                if(choiceAction != null){
                    choiceAction.Invoke();
                }
            }
            ConversationNode goingto = base.Goto(Speaker, peekOnly, conversation);
			if(goingto != null && goingto is acegiak_RomanceChatNode){
                goingto.Text = this.ResponseText;
            }
            return goingto;
		}



    }
}