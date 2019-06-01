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
	public class acegiak_RomanceChatNode : ConversationNode
	{

        float Amount = 0;
        string Title = "huh?";
        string Back = "ok.";

        public acegiak_RomanceChatNode(){}

        public acegiak_RomanceChatNode(string id, string title, string text, float opinionamount){
            this.Text = text;
            this.ID = id;
            this.Amount = opinionamount;
            this.Title = title;
        }

        public void AddChoice(string id, string title,string response, float opinionchange){
                acegiak_RomanceChatChoice choice = new acegiak_RomanceChatChoice();
                choice.ID = "acegiak_romance_"+id;
                choice.Text = title;
                choice.ResponseText = response;
                choice.OpinionAmount = opinionchange;
                choice.GotoID = "acegiak_romance_aboutme";
                this.Choices.Add(choice);
        }


		public void Visit(GameObject speaker, GameObject player){
            base.Visit(speaker,player);


            if(speaker.GetPart<acegiak_Romancable>() !=null){
               acegiak_RomanceChatNode temp = speaker.GetPart<acegiak_Romancable>().BuildNode(this);
               this.Text = temp.Text;
               this.Choices = temp.Choices;

            }

        }


        

        // public virtual void Visit(GameObject speaker, GameObject player){
        //     if(speaker != null && speaker.pBrain != null && player != null){
		// 	    ParentObject.pBrain.AdjustFeeling(player,Amount);                
        //     }
        //     base.Visit(speaker,player);
        // }

        // public void AttachTo(ConversationNode parentNode){
        //     parentNode.ParentConversation.NodesById.Remove(this.ID);
        //     parentNode.ParentConversation.AddNode(this);

        //     ConversationChoice choice = new ConversationChoice();
        //     choice.ID = this.ID+"_choice";
        //     choice.GotoID = this.ID;
        //     choice.Text = this.Title;

        //     ConversationChoice backchoice = new ConversationChoice();
        //     backchoice.ID = this.ID+"_return";
        //     backchoice.GotoID = parentNode.ID;
        //     backchoice.Text = this.Back;

        //     this.ParentNode = parentNode;
            
        //     parentNode.Choices.RemoveAll(x => x.ID == choice.ID || choice.GotoID);
        //     parentNode.Choices.Add(choice);


        // }


    }
}