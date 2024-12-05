using XRL.UI;
using XRL.World.Parts;
using System;
using System.Diagnostics;
using XRL.Core;
using XRL.World.AI;


namespace XRL.World.Conversations.Parts
{
	public class Flirt : IConversationPart
	{
		public static bool Enabled = true;
		public acegiak_RomanceChatNode current = null;
		

		public override bool WantEvent(int ID, int Propagation)
		{
			if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != EnterElementEvent.ID && ID != PrepareTextEvent.ID)
			{
				return ID == IsElementVisibleEvent.ID;
			}
			return true;
		}


		public override bool HandleEvent(PrepareTextEvent E)
		{
		
			E.Text.Clear();
			string whatIthink = "";
			if(The.Speaker.GetPart<acegiak_Romancable>().DisplayNext != null){
				whatIthink = The.Speaker.GetPart<acegiak_Romancable>().DisplayNext;
				The.Speaker.GetPart<acegiak_Romancable>().DisplayNext = null;
			}
			if(The.Speaker.GetPart<acegiak_Romancable>().OnLeaveNode != null){
				The.Speaker.GetPart<acegiak_Romancable>().OnLeaveNode();
			}
			E.Text.Append(current.Text.Replace("==verbalopinion==",whatIthink));
		
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetChoiceTagEvent E)
		{
			E.Tag = "{{g|[flirt]}}";
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EnterElementEvent E)
		{

			//TradeUI.ShowTradeScreen(The.Speaker);
			GameObject speaker = The.Speaker;
			speaker.Brain.RemoveOpinion<acegiak_FlirtOpinion>(XRLCore.Core.Game.Player.Body);
			speaker.Brain.AddOpinion<acegiak_FlirtOpinion>(XRLCore.Core.Game.Player.Body,1F*speaker.GetPart<acegiak_Romancable>().romancevalue);
			speaker.GetPart<acegiak_Romancable>().havePreference();

			if(current != null){
				speaker.GetPart<acegiak_Romancable>().OnLeaveNode = current.OnLeaveNode;
			}
			// IPart.AddPlayerMessage(speaker.DisplayNameOnly+" has patience "+speaker.GetPart<acegiak_Romancable>().patience.ToString()+", romance "+speaker.GetPart<acegiak_Romancable>().romancevalue.ToString()+", and feeling "+speaker.Brain.GetFeeling(XRLCore.Core.Game.Player.Body).ToString());

			current = speaker.GetPart<acegiak_Romancable>().BuildNode();
			ParentElement.Elements.Clear();
			foreach(Choice choice in current.Choices){
				if(choice.Text == null || choice.Text.Length < 1){
					continue;
				}
				if(choice.ID == null || choice.ID.Length < 1){
            		Guid myuuid = Guid.NewGuid();
					choice.ID = myuuid.ToString();
				}
				if(choice.Target != "End"){
					choice.Target = "DoFlirt";
				}
				if(choice is acegiak_RomanceChatChoice){
					ConversationText s = new ConversationText();
				}
				choice.Parent = ParentElement;
				ParentElement.Elements.Add(choice);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(IsElementVisibleEvent E)
		{
			return base.HandleEvent(E);
		}

	}
}
