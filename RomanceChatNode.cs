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

        public void InsertMyReaction(
            GameObject me, GameObject them)
        {
            // Must be kissable
            var kissable = me.GetPart<acegiak_Kissable>();
            if (kissable == null)
            {
                Text = "  &RNOT KISSABLE&y\n\n" + Text;
                return;
            }

            // Check a random kissing-preference
            kissable.havePreference();
            var preference = kissable.preferences
                [Stat.Rnd2.Next(kissable.preferences.Count)];
            var assess = preference.attractionAmount(me, them);

            // Generate a reaction
            string spiceKey = "<spice.eros.react." + assess.reactPath +
                ((assess.amount > 0f) ? ".like" : ".dislike") + ".!random>";
            string reaction = HistoricStringExpander.ExpandString(spiceKey);

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
            InitializeSpice();

            Text = FilterRandom(Text);
            Text =
                HistoricStringExpander.ExpandString(
                Text, entity, null, vars)
                + "&k"; // Black out village text, mrah
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