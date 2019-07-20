using Qud.API;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts;
using XRL.World;
using XRL.Rules;
using HistoryKit;

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

        public void AddChoice(string id, string title,string response, float opinionchange,acegiak_RomanceChatChoice.acegiak_ChoiceAction action = null){
                acegiak_RomanceChatChoice choice = new acegiak_RomanceChatChoice();
                choice.ID = "acegiak_romance_"+id;
                choice.Text = title;
                choice.ResponseText = response;
                choice.OpinionAmount = opinionchange;
                choice.GotoID = "acegiak_romance_aboutme";

                if(id == "End"){
                    choice.GotoID = "End";
                }
                choice.ParentNode = this;
                if(action != null){
                    choice.choiceAction = action;
                }
                this.Choices.Add(choice);
        }



        static bool InitializedSpice = false;

        static void InitializeSpice()
        {
            ModManager.ForEachFileIn("acegiak_romance", (string filePath, ModInfo modInfo) =>
            {
                if (filePath.ToLower().Contains(".json"))
                    acegiak_HistoricSpicePatcher.Patch(filePath);
            });
        }
        public void ExpandText(
            HistoricEntitySnapshot     entity,
            Dictionary<string, string> vars = null)
        {
            InitializeSpice();

            Text = "&C<spice.eros.react.jumble.!random>&Y\n\n"
                + FilterRandom(Text);
            Text = HistoricStringExpander.ExpandString(
                Text, entity, null, vars);
            foreach(ConversationChoice choice in Choices){
                choice.Text = FilterRandom(choice.Text);
                choice.Text = HistoricStringExpander.ExpandString(
                    choice.Text, entity, null, vars);
            }
        }
        public static string FilterRandom(string s)
        {
			return Regex.Replace(s, @"<([^\|>]+\|[^>]+)>", delegate(Match match)
			{
				string[] v = match.Groups[1].ToString().Split('|');
				return v[Stat.Rnd2.Next(v.Length)];
			});
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