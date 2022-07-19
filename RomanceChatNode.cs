using Qud.API;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts;
using XRL.World;
using XRL.Rules;
using HistoryKit;
using ConsoleLib.Console;
using XRL.World.Conversations;

namespace XRL.World
{
	[Serializable]
	public class acegiak_RomanceChatNode
	{
		public List<Choice> Choices = new List<Choice>();
        public float Amount = 0;
        public string Title = "huh?";
		public string Text = "";
		public string ID = "";
		public float OpinionAmount = 0f;

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
                choice.Target = "acegiak_romance_aboutme";
				choice.Parent = The.Conversation.GetStart();


                if(id == "End"){
                    choice.Target = "End";
                }
                if(action != null){
                    choice.choiceAction = action;
                }
                this.Choices.Add(choice);
        }

        public void InsertMyReaction(
            GameObject me, GameObject them)
        {
            // Must be kissable
            var kissable = me.GetPart<acegiak_Kissable>();
            if (kissable == null)
            {
                Text = "  &R(this character is not kissable)&y\n\n" + Text;
                return;
            }

            // Find a random applicable kissing-preference
            kissable.havePreference();
            acegiak_KissingPreferenceResult assess = null;
            for (int i = 0; i < 1; ++i)
            {
                var preference = kissable.preferences.GetRandomElement(Stat.Rnd2);
                if (preference == null)
                {
                    acegiak_RomanceText.Log("ERROR drew a null kissing preference");
                    return;
                }
                    //[Stat.Rnd2.Next(kissable.preferences.Count)];
                assess = preference.attractionAmount(me, them);
                if (assess.amount != 0f) break;
            }
            if (assess.amount == 0f)
            {
                //Text = "  &c(no reaction)&y\n\n" + Text;
                return;
            }


			Text = "==verbalopinion==\n\n"+Text;

            // Generate a reaction
            string spiceKey = "<spice.eros.react." + assess.reactPath +
                ((assess.amount > 0f) ? ".like" : ".dislike") + ".!random>";
            string reaction = acegiak_RomanceText.ExpandString(spiceKey, assess.reactVars);

            // Apply some formatting and prepend
            if (reaction != null && reaction.Count() > 0)
            {
                Text = "  &M" + reaction
                    // + " &k" + assess.reactPath
                    + "&y\n\n" + Text;
            }
            else
            {
                Text = "  &R"
                    + spiceKey.Substring(1,spiceKey.Count()-2)+"&y\n\n" + Text;
            }
        }
        public void ExpandText(
            HistoricEntitySnapshot     entity,
            Dictionary<string, string> vars = null)
        {
            Text = FilterRandom(Text);
            Text = Text
                //acegiak_RomanceText.ExpandString(
                //Text, entity, vars)
                + "&k"; // Black out village text, mrah
            foreach(Choice choice in Choices){
                choice.Text = FilterRandom(choice.Text);
                //choice.Text = acegiak_RomanceText.ExpandString(
                //    choice.Text, entity, vars);
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