using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.Rules;
using XRL.World;
using XRL.World.Encounters;
using Qud.API;
using System.Linq;
using HistoryKit;
using System.Text;


namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_SultanInterestPreference : acegiak_RomancePreference
	{
        HistoricEntity faveSultan = null;
        float amount = 0;


        List<HistoricEvent> tales = new List<HistoricEvent>();
        List<string> mytales = new List<string>();



        public acegiak_SultanInterestPreference(acegiak_Romancable romancable){
            Romancable = romancable;
            amount = (float)((Stat.Rnd2.NextDouble()*2)-0.9);

            History sultanHistory = XRLCore.Core.Game.sultanHistory;

            HistoricEntityList entitiesWherePropertyEquals = sultanHistory.GetEntitiesWherePropertyEquals("type", "sultan");
            this.faveSultan = entitiesWherePropertyEquals.GetRandomElement(Stat.Rand);


            List<HistoricEvent> list = new List<HistoricEvent>();
            for (int i = 0; i < this.faveSultan.events.Count; i++)
            {
                if (this.faveSultan.events[i].hasEventProperty("gospel"))
                {
                    list.Add(this.faveSultan.events[i]);
                }
            }
            if (list.Count > 0)
            {
                while(this.tales.Count<2){
                    this.tales.Add(list[Stat.Random(0, list.Count - 1)]);
                }
            }
            //IPart.AddPlayerMessage("They "+(amount>0?"like":"dislike")+" "+this.faveSultan.GetCurrentSnapshot().GetProperty("name","sultan"));

        }


        public override acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
            float retamount = 0;
            string retexplain = "";


            string sultancult = this.faveSultan.GetCurrentSnapshot().GetProperty("cultName");
            if(gift.GetPart<AddsRep>() != null && gift.GetPart<AddsRep>().Faction == sultancult){
                return new acegiak_RomancePreferenceResult(amount,(amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+"depiction of "+this.faveSultan.GetCurrentSnapshot().GetProperty("name","a sultan")+"&Y.");
            }
            // if(getType(gift) == wantedType){
            //     return new acegiak_RomancePreferenceResult(amount,(amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.ShortDisplayName+"&Y.");
            // }
            return null;
        }

        public override acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
            string bodytext = "whoah";

            float g = (float)Stat.Rnd2.NextDouble();

            var vars = new Dictionary<string, string>();
            vars["*sultan*"]   = this.faveSultan.GetCurrentSnapshot().GetProperty("name","a sultan");
            vars["*sultanLong*"] = vars["*sultan*"] + ", "
                + this.faveSultan.GetCurrentSnapshot().GetRandomElementFromListProperty("cognomen", "the ancient sultan", Stat.Rnd2);

            if(g<1){
                return Build_QA_Node(node, "sultan.qa.heard", (amount > 0) ? "gen_good" : "gen_bad", vars, this.faveSultan.GetCurrentSnapshot());

                /*bodytext = "Have you heard of "+this.faveSultan.GetCurrentSnapshot().GetProperty("name","a sultan")+", "+this.faveSultan.GetCurrentSnapshot().GetRandomElementFromListProperty("cognomen", "the ancient sultan", Stat.Rnd2)+"?";
                node.AddChoice("likethem","I have! An admirable hero.",amount>0?"Wasn't "+this.faveSultan.GetCurrentSnapshot().GetProperty("subjectPronoun","sultan")+" amazing?":"Oh, I think I disagree.",amount>0?1:-1);
                node.AddChoice("dislikethem","I am familiar with the tales of this villain.",amount>0?"That's very judgemental.":"Wasn't "+this.faveSultan.GetCurrentSnapshot().GetProperty("subjectPronoun","sultan")+" horrible?",amount>0?-1:1);
                node.AddChoice("dontknow","I've not heard of this "+this.faveSultan.GetCurrentSnapshot().GetProperty("name","sultan")+".",amount>0?"Well, "+this.faveSultan.GetCurrentSnapshot().GetProperty("subjectPronoun","sultan")+" was a hero.":"Well "+this.faveSultan.GetCurrentSnapshot().GetProperty("subjectPronoun","sultan")+" was a terror.",amount>0?-1:1);*/
            }

            if(Romancable != null){
                node.Text = node.Text+"\n\n"+Romancable.GetStory(node);
            }
            node.Text = node.Text+"\n\n"+bodytext;

            return node;
        }


        public override acegiak_RomancePreferenceResult DateAssess(GameObject Date, GameObject DateObject){
            if(DateObject.GetPart<SultanShrine>() != null && DateObject.GetPart<SultanShrine>().sultan == this.faveSultan){
                return new acegiak_RomancePreferenceResult(amount,Romancable.ParentObject.The+Romancable.ParentObject.ShortDisplayName+Romancable.ParentObject.GetVerb("stare")+" at "+DateObject.the+DateObject.ShortDisplayName+(amount>0?" in awe.":"in disgust."));
            }
            return null;
        }

        

        public override string GetStory(acegiak_RomanceChatNode node, HistoricEntitySnapshot entity){

            if(Stat.Rnd2.NextDouble()>0.25){
                var vars = new Dictionary<string, string>();
                vars["*sultan*"]   = this.faveSultan.GetCurrentSnapshot().GetProperty("name","a sultan");
                string storyTag = ((amount > 0) ?
                    "<spice.eros.opinion.sultan.like.story.!random>" :
                    "<spice.eros.opinion.sultan.dislike.story.!random>");
                while(this.mytales.Count < 5){
                    vars["*sultanLong*"] = vars["*sultan*"] + ", "
                        + this.faveSultan.GetCurrentSnapshot().GetRandomElementFromListProperty("cognomen", "the ancient sultan", Stat.Rnd2);
                    this.mytales.Add(//"  &K"+storyTag.Substring(1,storyTag.Count()-2)+"&y\n"+
                        acegiak_RomanceText.ExpandString(
                        storyTag, entity, vars));
                }
                return mytales[Stat.Rnd2.Next(tales.Count)];
            }

            HistoricEvent e = tales[Stat.Rnd2.Next(tales.Count)];
            node.OnLeaveNode = delegate{
                SultanShrine.RevealBasedOnHistoricalEvent(e);
			};

            return "<Did you know|I've heard that|There is a tale that says> "+e.GetEventProperty("gospel")+(amount>0?" <Isn't that interesting?|It's so fascinating!|At least, that's what I heard.>":" <Isn't that terrible?|Isn't that horrible?|At least, that's what I heard.>");

        }
        public override string getstoryoption(string key){
            var vars = new Dictionary<string, string>();
            vars["*sultan*"]   = this.faveSultan.GetCurrentSnapshot().GetProperty("name","a sultan");
            vars["*sultanLong*"] = vars["*sultan*"] + ", "
                + this.faveSultan.GetCurrentSnapshot().GetRandomElementFromListProperty("cognomen", "the ancient sultan", Stat.Rnd2);
            vars["*sultanObject*"] = //"a " +
                GetSpice(this.faveSultan.GetCurrentSnapshot().GetRandomElementFromListProperty("elements", "salt", Stat.Rnd2),"nouns");
            
            return acegiak_RomanceText.ExpandString(
                "<spice.eros.opinion.sultan." + ((amount > 0) ? "like." : "dislike.") + key + ".!random>",
                vars);
        }



        public static string GetSpice(string input,string lookfor)
		{
			HistoricSpice.CheckInit();
			input = input.ToLower();
			lookfor = lookfor.ToLower();
            return HistoricSpice.root["elements"][input][lookfor][Stat.Rnd2.Next(HistoricSpice.root["elements"][input][lookfor].Count)];
		}


        public override void Save(SerializationWriter Writer){
            base.Save(Writer);
            this.faveSultan.Save(Writer);
            Writer.Write(amount);
            Writer.Write(tales.Count);
            foreach(HistoricEvent tale in tales){
                tale.Save(Writer);
            }
            Writer.Write(mytales.Count);
            foreach(string tale in mytales){
                Writer.Write(tale);
            }
        }

        public override void Load(SerializationReader Reader){
            this.faveSultan = HistoricEntity.Load(Reader, XRLCore.Core.Game.sultanHistory);
            this.amount = Reader.ReadSingle();
            int countTales = Reader.ReadInt32();
            this.tales = new List<HistoricEvent>();
            for(int i = 0; i < countTales; i++){
                this.tales.Add(HistoricEvent.Load(Reader));
            }
            int countmytales = Reader.ReadInt32();
            this.mytales = new List<string>();
            for(int i = 0; i < countmytales; i++){
                this.mytales.Add(Reader.ReadString());
            }
        }
        

    }
}