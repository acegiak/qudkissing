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


		public override ConversationNode Goto(GameObject Speaker, bool peekOnly = false)
		{
            Speaker.pBrain.AdjustFeeling(XRLCore.Core.Game.Player.Body,(int)Math.Floor(this.OpinionAmount));
            ConversationNode goingto = base.Goto(Speaker,peekOnly);
			if(goingto != null && goingto is acegiak_RomanceChatNode){
                goingto.Text = this.ResponseText;
            }
            return goingto;
		}



    }
}