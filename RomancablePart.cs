using System;
using XRL.Core;
using XRL.UI;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Qud.API;
using System.Text.RegularExpressions;
using XRL.World.AI.GoalHandlers;
using Qud.API;
using HistoryKit;
using SimpleJSON;
using System.ComponentModel;
using UnityEngine;
using ConsoleLib.Console;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_Romancable : IPart
	{

		[NonSerialized]
		public List<acegiak_RomancePreference> preferences = null;
		
		[NonSerialized]
		public List<acegiak_RomanceBoon> boons = null;

		private int lastseen = 0;
		public int patience = 10;

		public bool lockout = true;
		
		[NonSerialized]
		public GameObject date = null;


		public int? lastQuestion = null;




		public acegiak_Romancable()
		{
			base.Name = "acegiak_Romancable";
			
		}

		public void havePreference(){
			if(ParentObject.IsPlayer()){
				this.preferences = new List<acegiak_RomancePreference>();
				this.boons = new List<acegiak_RomanceBoon>();
				return;
			}
			if(this.preferences == null){
				// IPart.AddPlayerMessage("Populating Preference");
				this.preferences = new List<acegiak_RomancePreference>();
				
				List<acegiak_RomancePreference> possible = new List<acegiak_RomancePreference>();


				if(GameObjectFactory.Factory == null || GameObjectFactory.Factory.BlueprintList == null){
					return;
				}
				foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
				{
					if (!blueprint.IsBaseBlueprint() && blueprint.DescendsFrom("RomancePreference"))
					{
						//IPart.AddPlayerMessage(blueprint.Name);
						GameObject sample = GameObjectFactory.Factory.CreateSampleObject(blueprint.Name);
						if(sample.HasTag("classname") && sample.GetTag("classname") != null && sample.GetTag("classname") != ""){
							try{
                    		acegiak_RomancePreference preference = Activator.CreateInstance(Type.GetType(sample.GetTag("classname")),this) as acegiak_RomancePreference;
							possible.Add(preference);
							}catch(Exception e){
								
							}
						}
					}
				}
				int count = Stat.Rnd2.Next(5);
				for(int i = 0; i<count;i++){
					int w = Stat.Rnd2.Next(possible.Count());
					preferences.Add(possible[w]);
				}


			}



			if(this.boons == null){
				// IPart.AddPlayerMessage("Populating Preference");
				this.boons = new List<acegiak_RomanceBoon>();
				
				List<acegiak_RomanceBoon> possible = new List<acegiak_RomanceBoon>();


				if(GameObjectFactory.Factory == null || GameObjectFactory.Factory.BlueprintList == null){
					return;
				}
				foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
				{
					if (!blueprint.IsBaseBlueprint() && blueprint.DescendsFrom("RomanceBoon"))
					{
						//IPart.AddPlayerMessage(blueprint.Name);
						GameObject sample = GameObjectFactory.Factory.CreateSampleObject(blueprint.Name);
						if(sample.HasTag("classname") && sample.GetTag("classname") != null && sample.GetTag("classname") != ""){
                    		acegiak_RomanceBoon preference = Activator.CreateInstance(Type.GetType(sample.GetTag("classname")), this) as acegiak_RomanceBoon;
							possible.Add(preference);
						}
					}
				}
				int count = Stat.Rnd2.Next(2);
				for(int i = 0; i<count;i++){
					int w = Stat.Rnd2.Next(possible.Count());
					boons.Add(possible[w]);
				}



				
				// IPart.AddPlayerMessage(this.preferences.ToString());
			}





			touch();

		}
		
	


		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "GetInventoryActions");
			Object.RegisterPartEvent(this, "InvCommandGift");
			Object.RegisterPartEvent(this, "PlayerBeginConversation");
			Object.RegisterPartEvent(this, "ShowConversationChoices");
			Object.RegisterPartEvent(this, "VisitConversationNode");
			Object.RegisterPartEvent(this, "OwnerGetInventoryActions");
			Object.RegisterPartEvent(this, "InvCommandArrangeDate");
			Object.RegisterPartEvent(this, "InvCommandBeginDate");
			Object.RegisterPartEvent(this, "CommandRemoveObject");
			base.Register(Object);
		}

        public bool Gift(GameObject who, bool FromDialog){

			havePreference();


            Inventory part2 = XRLCore.Core.Game.Player.Body.GetPart<Inventory>();
            List<XRL.World.GameObject> ObjectChoices = new List<XRL.World.GameObject>();
            List<string> ChoiceList = new List<string>();
            List<char> HotkeyList = new List<char>();
            char ch = 'a';
            part2.ForeachObject(delegate(XRL.World.GameObject GO)
            {
                    ObjectChoices.Add(GO);
                    HotkeyList.Add(ch);
                    ChoiceList.Add(GO.DisplayName);
                    ch = (char)(ch + 1);
            });
            if (ObjectChoices.Count == 0)
            {
                Popup.Show("You have no gifts to give.");
                return false;
            }
            int num12 = Popup.ShowOptionList(string.Empty, ChoiceList.ToArray(), HotkeyList.ToArray(), 0, "Select a gift to give.", 60, bRespectOptionNewlines: false, bAllowEscape: true);
            if (num12 < 0)
            {
                return false;
            }
			int result = (int)((assessGift(ObjectChoices[num12],who).amount+(Stat.Rnd2.Next(1,4) -1.5f))*10);
			

            XRL.World.Event event2 = XRL.World.Event.New("SplitStack", "Number", 1);
            event2.AddParameter("OwningObject", XRLCore.Core.Game.Player.Body);
            ObjectChoices[num12].FireEvent(event2);
            if (!part2.FireEvent(XRL.World.Event.New("CommandRemoveObject", "Object", ObjectChoices[num12])))
            {
                Popup.Show("You can't give that object.");
                return false;
            }
			ObjectChoices[num12].SetStringProperty("GiftedTo",ParentObject.id);
			ParentObject.GetPart<Inventory>().AddObject(ObjectChoices[num12]);
            ParentObject.pBrain.AdjustFeeling(who,result);
				if (who.IsPlayer())
				{
					Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + (result>0?"&Y likes the "+ObjectChoices[num12].pRender.DisplayName+".":"&r is unimpressed by the "+ObjectChoices[num12].pRender.DisplayName+"."));
				}
            return true;
        }



        public acegiak_RomancePreferenceResult assessGift(GameObject GO,GameObject who){
			havePreference();
            float value = 0;

			acegiak_RomancePreferenceResult ret = new acegiak_RomancePreferenceResult(0,"");

			foreach(acegiak_RomancePreference preferece in preferences){
				acegiak_RomancePreferenceResult result = preferece.GiftRecieve(GO,who);

				if(result != null){
					if(GO.GetPart<Commerce>() != null && GO.GetPart<Commerce>().Value >1){
						result.amount = ((float)result.amount)*((float)GO.GetPart<Commerce>().Value);
					}
					ret.amount += result.amount;
					ret.explanation = ret.explanation +"\n"+result.explanation;
					//IPart.AddPlayerMessage("" + ParentObject.the + ParentObject.DisplayNameOnly + "&Y "+result.explanation);
				}
			}
            return ret;
        }

		public void HandleBeginConversation(Conversation conversation, GameObject speaker){
			if(conversation.NodesByID != null
				&& conversation.NodesByID.Count >0
				&& speaker != null
				&& speaker.GetPart<acegiak_Romancable>() != null){

					conversation.NodesByID.ToList().Where(pair => pair.Key.StartsWith("acegiak_romance_")).ToList().ForEach(pair => conversation.NodesByID.Remove(pair.Key));


					string StartID = conversation.NodesByID.Keys.ToArray()[0];
					if(conversation.NodesByID.ContainsKey("Start")){
						StartID = "Start";
					}
					speaker.GetPart<acegiak_Romancable>().havePreference();

					acegiak_RomanceChatNode aboutme = new acegiak_RomanceChatNode();
					aboutme.ID = "acegiak_romance_aboutme";
					aboutme.Text = "Very well.";
					aboutme.ParentConversation = conversation;


					ConversationChoice returntostart = new ConversationChoice();
					returntostart.Text = "Ok.";
					returntostart.GotoID = "End";
					returntostart.ParentNode = aboutme;

					aboutme.Choices.Add(returntostart);

					ConversationChoice romanticEnquiry = new ConversationChoice();
					romanticEnquiry.ParentNode = conversation.NodesByID[StartID];
					romanticEnquiry.ID = "acegiak_romance_askaboutme";
					romanticEnquiry.Text = "Let's chat.";
					romanticEnquiry.GotoID = "acegiak_romance_aboutme";
					romanticEnquiry.Ordinal = 800;
					
					
					conversation.AddNode(aboutme);
					foreach(ConversationNode node in conversation.StartNodes){

						node.Choices.RemoveAll(choice => choice.ID.StartsWith("acegiak_romance_"));
						node.Choices.Add(romanticEnquiry);
						
					}
				}
				
		}

		public acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node){
			havePreference();
			this.lockout = false;
			node.Choices.Clear();
			//IPart.AddPlayerMessage("They are:"+ParentObject.pBrain.GetOpinion(XRLCore.Core.Game.Player.Body)+": "+ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body).ToString()+" patience:"+patience.ToString());
			
			if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) < 1 && patience > 0){
					ParentObject.pBrain.SetFeeling(XRLCore.Core.Game.Player.Body,1);
			}

			if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) < 1 || patience <= 0){
					List<string> Stories = new List<string>(new string[] {
                        "Sorry, I have other things to do.",
                        "Anyway, I'm very busy.",
                        "Maybe we could talk some more another time?"
                    });
					node.Text = node.Text + "\n\n"+ Stories[Stat.Rnd2.Next(0,Stories.Count-1)];

					ConversationChoice returntostart = new ConversationChoice();
					returntostart.Ordinal = 800;
					returntostart.Text = "Ok.";
					returntostart.GotoID = node.ParentConversation.StartNodes[0].ID;
					returntostart.ParentNode = node;
					node.Choices.Add(returntostart);
			}else if(boons.Where(b=>b.BoonReady(XRLCore.Core.Game.Player.Body)).Count() > 0){
				acegiak_RomanceBoon boon = boons.Where(b=>b.BoonReady(XRLCore.Core.Game.Player.Body)).OrderBy(o => Stat.Rnd2.NextDouble()).FirstOrDefault();
				try
				{
					node = boon.BuildNode(node);
					node.InsertMyReaction(ParentObject,XRLCore.Core.Game.Player.Body);
					node.ExpandText(GetSelfEntity());
				}
				catch (Exception e)
				{
					node.Text = e.ToString() + "\n\n" + node.Text;
				}
			}else{
				int c = 0;
				int whichquestion = 0;
				do{
					whichquestion = Stat.Rnd2.Next(0,preferences.Count);
					c++;
				}while(whichquestion == lastQuestion && c<5);
				lastQuestion = whichquestion;
				try
				{
					node = preferences[whichquestion].BuildNode(node);
					node.InsertMyReaction(ParentObject,XRLCore.Core.Game.Player.Body);
					node.ExpandText(GetSelfEntity());
				}
				catch (Exception e)
				{
					node.Text = e.ToString() + "\n\n" + node.Text;
				}

				if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>5){
					acegiak_RomanceChatChoice giftoption = new acegiak_RomanceChatChoice();
					giftoption.Ordinal = 900;
					giftoption.Text = "[Offer A Gift]";
					giftoption.action = "*Gift";
					giftoption.ParentNode = node;
					giftoption.GotoID = "End";
					node.Choices.Add(giftoption);
				}
				if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>=25){
					acegiak_RomanceChatChoice kissoption = new acegiak_RomanceChatChoice();
					kissoption.Ordinal = 910;
					kissoption.Text = "[Propose a Date]";
					kissoption.action = "*Date";
					kissoption.ParentNode = node;
					kissoption.GotoID = "End";
					node.Choices.Add(kissoption);
				}
				if(ParentObject.GetPart<acegiak_Kissable>() != null && ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>=55){
					acegiak_RomanceChatChoice kissoption = new acegiak_RomanceChatChoice();
					kissoption.Ordinal = 910;
					kissoption.Text = "[Attempt to Kiss]";
					kissoption.action = "*Kiss";
					kissoption.ParentNode = node;
					kissoption.GotoID = "End";
					node.Choices.Add(kissoption);
				}



			}
			if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) < 1){
				ParentObject.pBrain.SetFeeling(XRLCore.Core.Game.Player.Body,1);
			}

			

			patience--;


			ConversationChoice liveanddrink = new ConversationChoice();
			liveanddrink.Ordinal = 99999;
			liveanddrink.Text = "Live and drink.";
			liveanddrink.GotoID = "End";
			liveanddrink.ParentNode = node;
			node.Choices.Add(liveanddrink);


			return node;
		}

		public string GetStory(acegiak_RomanceChatNode node){
			string story = preferences[Stat.Rnd2.Next(0,preferences.Count-1)]
				.GetStory(node, GetSelfEntity());
			story = ConsoleLib.Console.ColorUtility.StripFormatting(story);
			if (story.Count() > 0) return story;
			return "&RI was going to tell you a story, but I forgot it.&y";
		}


		public override bool FireEvent(Event E){
            if (E.ID == "GetInventoryActions")
			{
				if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>5){
					E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("Gift", 'G',  false, "&Wg&yift", "InvCommandGift", 10);
				}
			}
			if (E.ID == "InvCommandGift" && Gift(E.GetGameObjectParameter("Owner"), FromDialog: true))
			{
				E.RequestInterfaceExit();
			}
			if (E.ID == "PlayerBeginConversation")
			{
				if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>0){
					HandleBeginConversation(E.GetParameter<Conversation>("Conversation"),E.GetParameter<GameObject>("Speaker"));
					GameObject speaker = E.GetParameter<GameObject>("Speaker");
					if(speaker.GetPart<acegiak_Romancable>() != null){
							
						//IPart.AddPlayerMessage("Ticks passed:"+(XRLCore.Core.Game.TimeTicks - speaker.GetPart<acegiak_Romancable>().lastseen).ToString());
						int newPatience = (int)Math.Floor((float)(XRLCore.Core.Game.TimeTicks - speaker.GetPart<acegiak_Romancable>().lastseen)/1200);

						//IPart.AddPlayerMessage("patience earned:"+(newPatience).ToString());
						if(speaker.GetPart<acegiak_Romancable>().lastseen == 0){
							newPatience = 0;
						}
						if(newPatience>10){newPatience = 10;}
						speaker.GetPart<acegiak_Romancable>().lastseen = (int)XRLCore.Core.Game.TimeTicks;
						speaker.GetPart<acegiak_Romancable>().patience = speaker.GetPart<acegiak_Romancable>().patience+newPatience;
					}
					// if(patience > 5 && ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>0 && ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)<5){
					// 	ParentObject.pBrain.AdjustFeeling(XRLCore.Core.Game.Player.Body,(int)Math.Min(5,patience-5));
					// }
				}
			}
			if (E.ID == "VisitConversationNode")
			{
				if(E.GetParameter<ConversationNode>("CurrentNode") != null && E.GetParameter<ConversationNode>("CurrentNode") is acegiak_RomanceChatNode){
					E.SetParameter("CurrentNode",BuildNode((acegiak_RomanceChatNode)E.GetParameter<ConversationNode>("CurrentNode")));
				}
			}


            if (E.ID == "OwnerGetInventoryActions")
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Object");
				acegiak_Romancable romancable = gameObjectParameter2.GetPart<acegiak_Romancable>();
				if (romancable != null && gameObjectParameter2.pBrain.GetFeeling(ParentObject) > 25)
				{
					E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("ArrangeDate", 'r',  true, "A&Wr&yrange a Date", "InvCommandArrangeDate");
				}
			}
            if (E.ID == "InvCommandArrangeDate")
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Object");
				acegiak_Romancable romancable = gameObjectParameter2.GetPart<acegiak_Romancable>();
				if (romancable != null && gameObjectParameter2.pBrain.GetFeeling(ParentObject) > 25)
				{
					this.date = gameObjectParameter2;
					Popup.Show(gameObjectParameter2.ShortDisplayName+" seems amenable to the idea.");
				}
			}

            if (E.ID == "OwnerGetInventoryActions")
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Object");
				if (date != null && date.pBrain.GetFeeling(ParentObject) > 25)
				{
					E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("BeginDate", 'j',  true, "Invite "+this.date.ShortDisplayName+" to &Wj&yoin you", "InvCommandBeginDate");
				}
			}
            if (E.ID == "InvCommandBeginDate")
			{
				GameObject GO = E.GetGameObjectParameter("Object");
				acegiak_Romancable romancable = this.date.GetPart<acegiak_Romancable>();
				if (romancable != null && date.pBrain.GetFeeling(ParentObject) > 25)
				{
					
					this.date.pBrain.PushGoal(new acegiak_WaitWith(10,ParentObject));
					this.date.pBrain.PushGoal(new acegiak_DateAssess(ParentObject,GO));
					this.date.pBrain.PushGoal(new acegiak_MoveTo(GO,true));
					E.RequestInterfaceExit();

					IPart.AddPlayerMessage(date.ShortDisplayName+" comes to join you at "+GO.the+GO.ShortDisplayName);

				}
			}
			if(E.ID == "CommandRemoveObject" && XRLCore.Core.Game != null &&  !ParentObject.IsPlayer() && ParentObject != null && ParentObject.id != null){
				GameObject G = E.GetGameObjectParameter("Object");
				if(G.GetPropertyOrTag("GiftedTo") == ParentObject.id && assessGift(G,ParentObject).amount>0){
					Popup.Show(ParentObject.The+ParentObject.DisplayNameOnly+" cannot bear to part with "+G.the+G.DisplayNameOnly+".");
				}
			}
			


			return base.FireEvent(E);
		}

		//private HistoricEntity selfHistory;
		private HistoricEntitySnapshot selfEntity;

		static string[] StoryOptionTags =
		{
			"goodThingHappen",
			"badThingHappen",
			"aGoodObject",
			"aBadObject",
			"aGoodPerson",
			"aBadPerson",
			"aGoodWeapon",
			"aBadWeapon",
			"goodArmor",
			"badArmor"
		};
		private HistoricEntitySnapshot GetSelfEntity()
		{
			if (selfEntity != null) return selfEntity;

			havePreference();

			var myBody = ParentObject.GetPart<Body>().GetBody();
			//PronounSet pronouns = ParentObject.GetPronounSet();

			//selfHistory = new HistoricEntity();
			selfEntity = new HistoricEntitySnapshot(null);
			selfEntity.setProperty("name", ParentObject.The + ParentObject.DisplayNameOnlyDirect);

			// Should not use these pronoun forms
            selfEntity.setProperty("subjectPronoun", ParentObject.it);
            selfEntity.setProperty("objectPronoun", ParentObject.them);
            selfEntity.setProperty("possessivePronoun", ParentObject.its);
			selfEntity.setProperty("substantivePossessivePronoun", ParentObject.theirs);

			// Populate entity with storyoptions
			foreach (string option in StoryOptionTags)
			{
				var values = new List<string>();
				foreach (var preference in preferences)
				{
					// Three items from each preference?
					for (int i = 0; i < 3; ++i)
					{
						string value = preference.getstoryoption(option);
						if (value != null && value.Count() != 0)
							values.Add(value);
					}
				}
				if (values.Count() == 0)
				{
					// Backup
					string vague = null;
					for (int i = 0; i < 5; ++i)
					{
						vague = acegiak_RomanceText.ExpandString(
							"<spice.eros.opinion.storyOption." + option + ".!random>",
							selfEntity, null, null);
						if (vague != null && vague.Count() > 0) break;
					}
					
					if (vague == null || vague.Count() == 0)
						vague = "(&R" + option + "&y)";
					else if (vague[0] == '<')
						vague = "(&R" + vague.Substring(1,vague.Count()-2) + "&y)";
					values.Add(vague);
				}
				selfEntity.listProperties.Add(option, values);
			}

			return selfEntity;
		}

		/*public string storyoptions(string key,string alt){
			List<string> output = new List<string>();
			foreach(acegiak_RomancePreference preferece in preferences){
				string newstring = preferece.getstoryoption(key);
				if(newstring != null){
					output.Add(newstring);
				}
			}
			if(output.Count > 0){
				return output[Stat.Rnd2.Next(output.Count)];
			}
			return alt;
		}*/

		public void AssessDate(GameObject Date,GameObject DateObject){
			havePreference();
            float value = (Stat.Rnd2.Next(1,4) -2);
			string output = ParentObject.The+ParentObject.ShortDisplayName+" joins you at "+DateObject.the+DateObject.ShortDisplayName;

			foreach(acegiak_RomancePreference preferece in preferences){
				acegiak_RomancePreferenceResult result = preferece.DateAssess(Date,DateObject);

				if(result != null){
					value += result.amount;
					output += "\n"+result.explanation;
					//IPart.AddPlayerMessage("" + ParentObject.the + ParentObject.DisplayNameOnly + "&Y "+result.explanation);
				}
			}
			Popup.Show(output);
            ParentObject.pBrain.AdjustFeeling(Date,(int)(value*10));
			Date.GetPart<acegiak_Romancable>().date = null;
			if(value <1){
				this.patience -= 2;
			}else{
				this.patience -= 1;
			}
			JournalAPI.AddAccomplishment("&y You took "+ParentObject.a + ParentObject.DisplayNameOnlyDirect +" on a date to "+DateObject.the+DateObject.DisplayNameOnlyDirect+" and "+ParentObject.it+(value>0?" was&G":" was &rnot")+" impressed&y.", "general", null, -1L);

		}
        public void touch()
        {
            // Load our normal data
			foreach(acegiak_RomancePreference preference in this.preferences){
				preference.setRomancable(this);
			}
			foreach(acegiak_RomanceBoon boon in this.boons){
				boon.setRomancable(this);
			}
		}

		//         [NonSerialized]
        // public List<StandAbility> abilities = new List<StandAbility>();

        // SaveData is called when the game is ready to save this object, so we override it here.
        public override void SaveData(SerializationWriter Writer)
        {
			this.havePreference();


			if(Writer == null){
				throw new Exception("The Writer is null!");
			}
			if(preferences == null){
				throw new Exception("The preferences list is null!");
			}

			if(boons == null){
				throw new Exception("The boons list is null!");
			}

			this.preferences.RemoveAll(d=>d==null);
			this.boons.RemoveAll(d=>d==null);

            // We have to call base.SaveData to save all normally serialized fields on our class
            base.SaveData(Writer);
			
            // Writing out the number of items in this list lets us know how many items we need to read back in on Load
            Writer.Write(preferences.Count);
            foreach (acegiak_RomancePreference preference in preferences)
            {
				preference.Save(Writer);
            }
            Writer.Write(boons.Count);
            foreach (acegiak_RomanceBoon boon in boons)
            {
				boon.Save(Writer);
            }

        }

        // Load data is called when loading the save game, we also need to override this
        public override void LoadData(SerializationReader Reader)
        {
            // Load our normal data
            base.LoadData(Reader);


			this.preferences = new List<acegiak_RomancePreference>();
			this.boons = new List<acegiak_RomanceBoon>();


			if(Reader == null){
				throw new Exception("The Reader is null!");
			}
			if(preferences == null){
				throw new Exception("The preferences list is null!");
			}

			if(boons == null){
				throw new Exception("The boons list is null!");
			}
			if(this == null){
				throw new Exception("This is null!");
			}


            // Read the number we wrote earlier telling us how many items there were
            int arraySize = Reader.ReadInt32();
            for (int i = 0; i < arraySize; i++)
            {
               
                acegiak_RomancePreference.Read(Reader,this);
                // Similar to above, if we had a basic type in our list, we would instead use the Reader.Read function specific to our object type.
            }


            int boonarraySize = Reader.ReadInt32();
            for (int j = 0; j < boonarraySize; j++)
            {
                acegiak_RomanceBoon.Read(Reader,this);
                // Similar to above, if we had a basic type in our list, we would instead use the Reader.Read function specific to our object type.
            }
        }

	}
}