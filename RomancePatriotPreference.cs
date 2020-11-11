using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.Rules;
using XRL.World;
using XRL.World.Encounters;
using Qud.API;
using HistoryKit;
using System.Linq;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_PatriotPreference : acegiak_RomancePreference
	{
        HistoricEntitySnapshot village = null;
        Faction faction = null;
        float amount = 0;


        List<JournalVillageNote> historytales = new List<JournalVillageNote>();


        List<string> tales = new List<string>();



        public acegiak_PatriotPreference(acegiak_Romancable romancable){
            Romancable = romancable;


            List<HistoricEntity> villages = HistoryAPI.GetVillages();
            // .Name.Contains("villagers")
            int high = 0;
            foreach (KeyValuePair<string, int> item in  romancable.ParentObject.pBrain.FactionMembership)
			{
                if(item.Key.Contains("villagers")){
                    if(item.Value > high && villages.Select(v=>v.GetCurrentSnapshot()).Where(v=>item.Key.Contains(v.Name)).Count()>0){
                        village = villages.Select(v=>v.GetCurrentSnapshot()).Where(v=>item.Key.Contains(v.Name)).FirstOrDefault();
                        high = item.Value;
                        faction = Factions.get(item.Key);
                    }
                }
			}

			

            amount = (float)((Stat.Rnd2.NextDouble()*1.5)-0.5);


            
            if(village == null || faction == null){
                throw new Exception("Not a villager.");
            }
            //IPart.AddPlayerMessage("They "+(amount>0?"like":"dislike")+" "+this.interestedFaction);
            List<JournalVillageNote> notesForVillage = JournalAPI.GetNotesForVillage(village.entity.id);

            while(this.historytales.Count<2){
                this.historytales.Add(notesForVillage[Stat.Rnd2.Next(0, notesForVillage.Count)]);
            }
        }

        string factionName(){
            return faction.getFormattedName();
        }
        string townName(){
            return village.Name;
        }

        public List<string> goods (){
            if(village == null){
                throw new Exception("Village does not exist.");
            }
            List<string> good = village.GetList("sacredThings");
            if(good == null){good = new List<string>();}
            good.Add(village.GetProperty("defaultSacredThing"));
            good.Add(village.GetProperty("signatureItem"));
            good.Add(village.GetProperty("signatureHistoricObjectName"));
            good.RemoveAll(r=>r==null||r=="unknown");
            return good;
        }

        public List<string> bads(){
            if(village == null){
                throw new Exception("Village does not exist.");
            }
            List<string> bad = village.GetList("profaneThings");
            if(bad == null){bad = new List<string>();}
            bad.Add(village.GetProperty("defaultProfaneThing"));
            bad.RemoveAll(r=>r==null||r=="unknown");
            return bad;
        }

        public string randomGood(){
            string element = goods().GetRandomElement(Stat.Rnd2);
            // GameObject GO = GameObject.create(element);
            // if(GO == null){
                return element;
            // }
            // return GO.a+GO.DisplayNameOnly;
        }
        public string randomBad(){
            string element = bads().GetRandomElement(Stat.Rnd2);
            // GameObject GO = GameObject.create(element);
            // if(GO == null){
                return element;
            // }
            // return GO.a+GO.DisplayNameOnly;
        }

        public override acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift){
            float retamount = 0;
            string retexplain = "";


            if(goods().Contains(gift.GetBlueprint().Name)){
                retamount += this.amount;
                retexplain = (amount >= 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.DisplayNameOnly+"'s &Y sacredness.";
            }
            if(bads().Contains(gift.GetBlueprint().Name)){
                retamount += this.amount*-1f;

                retexplain = (amount < 0 ?"&Glikes&Y the ":"&rdislikes&Y the ")+gift.ShortDisplayName+"'s&Y profanity.";
            }
            
            if(retamount != 0 || retexplain != ""){
                return new acegiak_RomancePreferenceResult(retamount,retexplain);
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

            if(g<0.5){
                vars["*sacredThing*"] = randomGood();
                return Build_QA_Node(node, "patriot.qa.sacredThing", (amount > 0) ? "gen_good" : "gen_bad", vars);

                /*bodytext = "<What do you think of|How do you feel about|What is your opinion of> "+randomGood()+"?";
                node.AddChoice("likethem","I approve.",amount>0?"They are lovely, aren't they?":"Oh, You must keep awful company.",amount>0?1:-1);
                node.AddChoice("dislikethem","It is terrible.",amount>0?"That's very judgemental":"Aren't they horrible?",amount>0?-1:1);*/
            }else
            if(g<1){
                vars["*profaneThing*"] = randomBad();
                return Build_QA_Node(node, "patriot.qa.profaneThing", (amount > 0) ? "gen_good" : "gen_bad", vars);

                /*bodytext = "<What do you think of|How do you feel about|What is your opinion of> "+randomBad()+"?";
                node.AddChoice("likethem","I approve.",amount>0?"Such blasphemy!":"They aren't so bad, are they?.",amount>0?1:-1);
                node.AddChoice("dislikethem","It is terrible.",amount>0?"Isn't it horrid?":"Then you are a fool.",amount>0?-1:1);*/
            }

            if(Romancable != null){
                node.Text = node.Text+"\n\n"+Romancable.GetStory(node);
            }
            node.Text = node.Text+"\n\n"+bodytext;

            return node;
        }


        public override acegiak_RomancePreferenceResult DateAssess(GameObject Date, GameObject DateObject){
            // if(DateObject.GetPart<Pettable>() != null){
            //     return new acegiak_RomancePreferenceResult(0,Romancable.ParentObject.The+Romancable.ParentObject.ShortDisplayName+Romancable.ParentObject.GetVerb("pet")+DateObject.the+DateObject.ShortDisplayName+".");
            // }
            return null;
        }

        

        public override string GetStory(acegiak_RomanceChatNode node, HistoricEntitySnapshot entity){
            if(Stat.Rnd2.NextDouble()<0.5f){
                var vars = new Dictionary<string, string>();
                string storyTag = ((amount > 0) ?
                    "<spice.eros.opinion.patriot.like.story.!random>" :
                    "<spice.eros.opinion.patriot.dislike.story.!random>");
                while(this.tales.Count < 5){
                    vars["*sacredThing*"]  = randomGood();
                    vars["*profaneThing*"] = randomBad();
                    this.tales.Add(//"  &K"+storyTag.Substring(1,storyTag.Count()-2)+"&y\n"+
                        acegiak_RomanceText.ExpandString(
                        storyTag, entity, vars));
                }
                return tales[Stat.Rnd2.Next(tales.Count)];
            }

            JournalVillageNote e = historytales[Stat.Rnd2.Next(historytales.Count)];
            	node.OnLeaveNode = delegate{
					e.Reveal();
				};

            return "<Did you know|I've heard that|There is a tale that says> "+e.GetDisplayText()+(amount>0?" <Isn't that interesting?|It's so fascinating!|At least, that's what I heard.>":" <Isn't that terrible?|Isn't that horrible?|At least, that's what I heard.>");

        }
        public override string getstoryoption(string key){
            var vars = new Dictionary<string, string>();
            vars["*sacredThing*"] = randomGood();
            vars["*profaneThing*"] = randomBad();
            
            return acegiak_RomanceText.ExpandString(
                "<spice.eros.opinion.patriot." + ((amount > 0) ? "like." : "dislike.") + key + ".!random>",
                vars);
        }
         public override void Save(SerializationWriter Writer){
            base.Save(Writer);
            this.village.entity.Save(Writer);
            Writer.Write(amount);
            Writer.Write(faction.Name);
            Writer.Write(historytales.Count);
            foreach(JournalVillageNote tale in historytales){
                 Writer.Write(tale.secretid);
            }
            Writer.Write(tales.Count);
            foreach(string tale in tales){
                Writer.Write(tale);
            }
        }

        public override void Load(SerializationReader Reader){
            this.village = HistoricEntity.Load(Reader, XRLCore.Core.Game.sultanHistory).GetCurrentSnapshot();
            this.amount = Reader.ReadSingle();
            this.faction = Factions.get(Reader.ReadString());

            int countTales = Reader.ReadInt32();
            this.historytales = new List<JournalVillageNote>();
            List<JournalVillageNote> allnotes= JournalAPI.GetNotesForVillage(village.entity.id);
            for(int i = 0; i < countTales; i++){
                string thissecretid = Reader.ReadString();
                JournalVillageNote note = allnotes.FirstOrDefault(c=>c.secretid == thissecretid);
                if(note != null){
                    this.historytales.Add(note);
                }
            }
            int counttales = Reader.ReadInt32();
            this.tales = new List<string>();
            for(int i = 0; i < counttales; i++){
                this.tales.Add(Reader.ReadString());
            }
        }

    }
}