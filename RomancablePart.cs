using System;
using XRL.Core;
using XRL.UI;
using XRL.Rules;
using XRL.Names;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;
using System.Linq;
using System.Collections.Generic;
using Qud.API;
using System.Text.RegularExpressions;
using HistoryKit;
using UnityEngine;
using XRL.World.Conversations;


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

		public bool annoyed = false;
		
		[NonSerialized]
		public GameObject date = null;


		public int? lastQuestion = null;
		public string namegenerated = null;

		public int? storedFeeling = null;

		public string DisplayNext = null;

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
			bool loading = false;
			if(this.preferences == null || this.boons == null){
            	Loading.SetLoadingStatus("Compiling preferences...");
				loading = true;
			}
			if(!ParentObject.pBrain.HasPersonalFeeling(XRLCore.Core.Game.Player.Body) && storedFeeling != null){
				ParentObject.pBrain.SetFeeling(XRLCore.Core.Game.Player.Body,storedFeeling.Value);
			}
			if(this.preferences == null){
				//IPart.AddPlayerMessage("Populating Preference for "+ParentObject.DisplayNameOnly);
				this.preferences = new List<acegiak_RomancePreference>();
				
				List<acegiak_RomancePreference> possible = new List<acegiak_RomancePreference>();


				if(GameObjectFactory.Factory == null || GameObjectFactory.Factory.BlueprintList == null){
					return;
				}
				GameObjectBlueprint[] blueprints = GameObjectFactory.Factory.BlueprintList.ToArray();
				for (int i = blueprints.Length-1;i>=0;i--)
				{
					GameObjectBlueprint blueprint = blueprints[i];
					if (!blueprint.IsBaseBlueprint() && blueprint.DescendsFrom("RomancePreference"))
					{
						//IPart.AddPlayerMessage("possible preference type:"+blueprint.Name);
						GameObject sample = GameObjectFactory.Factory.CreateSampleObject(blueprint.Name);
						if(sample.HasTag("classname") && sample.GetTag("classname") != null && sample.GetTag("classname") != ""){
							try{
                    			acegiak_RomancePreference preference = Activator.CreateInstance(Type.GetType(sample.GetTag("classname")),this) as acegiak_RomancePreference;
								possible.Add(preference);
							}catch(Exception e){
								LogWarning(e.ToString());
							}
						}
					}
				}
				//IPart.AddPlayerMessage("possible prefs checked.");


				int count = Stat.Rnd2.Next(3)+4;
				//IPart.AddPlayerMessage("choosing "+count.ToString()+" out of "+possible.Count().ToString()+" prefs");

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


			if(loading){
				Loading.SetLoadingStatus(null);
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


		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != OwnerGetInventoryActionsEvent.ID )
			{
				return ID == InventoryActionEvent.ID;
			}
			return true;
		}

		// public override bool HandleEvent(BeginConversationEvent E){
		// 	havePreference();

		// 	if(!E.SpeakingWith.IsPlayer() && E.Actor.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>0){
		// 		GameObject speaker = E.SpeakingWith;
				
		// 		if(E.SpeakingWith.GetPart<acegiak_Romancable>() != null){
						
		// 			float patienceRate = 200f; //DEFAULT: 1200
		// 			long ticks = XRLCore.Core.Game.TimeTicks - E.SpeakingWith.GetPart<acegiak_Romancable>().lastseen;
		// 			int newPatience = (int)Math.Floor(((float)(ticks))/patienceRate);
		// 			if(E.SpeakingWith.GetPart<acegiak_Romancable>().lastseen <= 0){
		// 				newPatience = 5;
		// 			}
		// 			if(newPatience>5){newPatience = 10;}
		// 			if(newPatience < 0){
		// 				newPatience = 0;
		// 			}
		// 			E.SpeakingWith.GetPart<acegiak_Romancable>().lastseen = (int)XRLCore.Core.Game.TimeTicks;
		// 			E.SpeakingWith.GetPart<acegiak_Romancable>().patience = (int)Mathf.Min(10f,Mathf.Max(0f,E.SpeakingWith.GetPart<acegiak_Romancable>().patience+newPatience));
		// 		}
		// 		HandleBeginConversation(E.Conversation,E.SpeakingWith);

		// 	}
		// 	return base.HandleEvent(E); 
		// }

		public override bool HandleEvent(GetInventoryActionsEvent E)
		{
			if(!ParentObject.IsPlayer()){
				E.AddAction("Gift", "Gift", "GiveGift", null, 'G',  false, 10);
			


				GameObject gameObjectParameter2 = E.Object;
				acegiak_Romancable romancable = gameObjectParameter2.GetPart<acegiak_Romancable>();
				Brain brain = gameObjectParameter2.GetPart<Brain>();
				if (romancable != null && brain != null && brain.GetFeeling(ParentObject) > 25 && gameObjectParameter2 != ParentObject)
				{
					E.AddAction("ArrangeADate","Arrange a Date","ArrangeDate",null,'r',false,10);
				}
			}

			return base.HandleEvent(E);
		}

		public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
		{
			
				if (date != null && date.pBrain.GetFeeling(ParentObject) > 25 && date != ParentObject)
				{
					E.AddAction("BeginDate","Invite "+this.date.ShortDisplayName+" to &Wj&yoin you","BeginDate",null,'j',true,10);
				}
			

			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "GiveGift" && Gift(E.Actor,  true))
			{
				E.RequestInterfaceExit();
			}

			if (E.Command == "BeginDate")
			{
				GameObject GO = E.Item;
				GameObject d = E.Actor.GetPart<acegiak_Romancable>().date;
				acegiak_Romancable romancable = d.GetPart<acegiak_Romancable>();
				if(romancable == null){
					IPart.AddPlayerMessage("date is not romancable");
				}else if(d.pBrain.GetFeeling(E.Actor) < 25){
					IPart.AddPlayerMessage("date does not approve of you");

				}else
				{
					d.pBrain.PushGoal(new acegiak_WaitWith(15,E.Actor));
					d.pBrain.PushGoal(new acegiak_DateAssess(E.Actor,GO));
					d.pBrain.PushGoal(new MoveTo(GO,true));
					IPart.AddPlayerMessage(d.ShortDisplayName+" comes to join you at "+GO.the+GO.ShortDisplayName);
					JournalAPI.AddAccomplishment("&y"+d.a + d.DisplayNameOnlyDirect +" joined you for a date at "+GO.the+GO.DisplayNameOnlyDirect, "general", null);
					E.RequestInterfaceExit();
				}
				
			}

            if (E.Command == "ArrangeDate")
			{
				GameObject gameObjectParameter2 = E.Item;
				acegiak_Romancable romancable = gameObjectParameter2.GetPart<acegiak_Romancable>();
				if (romancable != null && gameObjectParameter2.pBrain.GetFeeling(E.Actor) > 25 && gameObjectParameter2 != E.Actor)
				{
					this.date = gameObjectParameter2;
					Popup.Show(gameObjectParameter2.ShortDisplayName+" seems amenable to the idea.");
				}
			}

			return base.HandleEvent(E);
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
            int num12 = Popup.ShowOptionList(string.Empty, ChoiceList.ToArray(), HotkeyList.ToArray(), 0, "Select a gift to give.", 60);
            if (num12 < 0)
            {
                return false;
            }
			int result = (int)((assessGift(ObjectChoices[num12],who).amount+(Stat.Rnd2.NextDouble() -1))*10);
			

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
			int feeling  = ParentObject.pBrain.GetFeeling(who);
			string message = "";
			if(result > 0){
				message = "&Y likes the "+ObjectChoices[num12].pRender.DisplayName+".";
				annoyed = false;
			}else{
				message = "&r is unimpressed by the "+ObjectChoices[num12].pRender.DisplayName+".";
			}
			if(result < 0){
				if(feeling + result < 0){
					message = "&r is annoyed by your bothersome gift!";
					if(feeling >= 0 && !annoyed){
						result = feeling * -1;
						annoyed = true;
					}
				}else{
					annoyed = false;
				}
			}
			
            ParentObject.pBrain.AdjustFeeling(who,result);
			storedFeeling = ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body);
			if (who.IsPlayer())
			{
				Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + (message));
			}
            return true;
        }

		public bool isGOaFactionHeirloom(GameObject GO){
			foreach (KeyValuePair<string, int> item in ParentObject.pBrain.FactionMembership)
			{
				if (ParentObject.pBrain.GetFactionAllegiance(item.Key) == Brain.FactionAllegiance.member)
				{
					Faction faction = XRL.World.Factions.getIfExists(item.Key);
					string heirloom = faction.Heirloom.ToLower();
					heirloom = Regex.Replace(heirloom, @"s$", "");
					heirloom = Regex.Replace(heirloom,"body","basearmor");
					//IPart.AddPlayerMessage(heirloom+" is SOUGHT");

					GameObjectBlueprint bp = GameObjectFactory.Factory.GetBlueprint(GO.Blueprint);
					if(bp.Name.ToLower().Contains(heirloom)){
						//IPart.AddPlayerMessage(GO.Blueprint+" is FAVOURABLE");

						return true;
					}
					while(bp != null && bp.Inherits != null){
						if(bp.Name.ToLower().Contains(heirloom)){
							//IPart.AddPlayerMessage(GO.Blueprint+" is FAVOURABLE");

							return true;
						}
						bp = GameObjectFactory.Factory.GetBlueprint(bp.Inherits);
						
					}
				}
			}
			//IPart.AddPlayerMessage(GO.Blueprint+" is NOT FAVOURABLE");

			return false;
		}

        public acegiak_RomancePreferenceResult assessGift(GameObject GO,GameObject who){
			havePreference();

			acegiak_RomancePreferenceResult ret = new acegiak_RomancePreferenceResult(0,"");

			foreach(acegiak_RomancePreference preferece in preferences){
				acegiak_RomancePreferenceResult result = preferece.GiftRecieve(who,GO);

				if(result != null){
					if(GO.GetPart<Commerce>() != null && GO.GetPart<Commerce>().Value >1){
						result.amount = ((float)result.amount)*((float)GO.GetPart<Commerce>().Value);
					}
					ret.amount += result.amount;
					ret.explanation = ret.explanation +"\n"+result.explanation;
					//IPart.AddPlayerMessage("" + ParentObject.the + ParentObject.DisplayNameOnly + "&Y "+result.explanation);
					
				}
			}
			if(isGOaFactionHeirloom(GO)){
				ret.amount += 1;
				ret.explanation = ret.explanation +"\nIt is favorable to "+XRL.World.Factions.getIfExists(ParentObject.pBrain.GetPrimaryFaction()).DisplayName;
			}
			//Log("GIFT ASSESS:"+GO.DisplayNameOnly+":"+ret.amount.ToString()+"\n"+ret.explanation);
            return ret;
        }

		public void HandleBeginConversation(Conversation conversation, GameObject speaker){
			if(conversation.NodesByID != null
				//&& conversation.NodesByID.Count >0
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
					//aboutme.ParentConversation = conversation;


					Choice returntostart = new Choice();
					returntostart.Parent = The.Conversation.GetStart();

					returntostart.Text = "Ok.";
					returntostart.Target = "End";
					//returntostart.ParentNode = aboutme;

					aboutme.Choices.Add(returntostart);

					ConversationChoice romanticEnquiry = new ConversationChoice();
					romanticEnquiry.ParentNode = conversation.NodesByID[StartID];
					romanticEnquiry.ID = "acegiak_romance_askaboutme";
					romanticEnquiry.Text = "[Flirt]";
					romanticEnquiry.GotoID = "acegiak_romance_aboutme";
					romanticEnquiry.Ordinal = 800;
					
					
					//conversation.AddNode(aboutme);
					foreach(ConversationNode node in conversation.StartNodes){

						node.Choices.RemoveAll(choice => choice.ID.StartsWith("acegiak_romance_"));
						node.Choices.Add(romanticEnquiry);
						
					}
				}
				
		}

		public void calculatePatience(){
						
			float patienceRate = 200f; //DEFAULT: 1200
			long ticks = XRLCore.Core.Game.TimeTicks - lastseen;
			int newPatience = (int)Math.Floor(((float)(ticks))/patienceRate);
			if(lastseen <= 0){
				newPatience = 5;
			}
			if(newPatience>5){newPatience = 10;}
			if(newPatience < 0){
				newPatience = 0;
			}
			lastseen = (int)XRLCore.Core.Game.TimeTicks;
			patience = (int)Mathf.Min(10f,Mathf.Max(0f,patience+newPatience));
				
		}

		public acegiak_RomanceChatNode BuildNode(){
			havePreference();
			calculatePatience();
			this.lockout = false;
			acegiak_RomanceChatNode node = new acegiak_RomanceChatNode();
			node.Choices.Clear();
			//IPart.AddPlayerMessage("They("+ParentObject.DisplayNameOnly+") are:"+ParentObject.pBrain.GetOpinion(XRLCore.Core.Game.Player.Body)+": "+ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body).ToString()+" patience:"+patience.ToString()+" personal?"+ParentObject.pBrain.HasPersonalFeeling(XRLCore.Core.Game.Player.Body).ToString());



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

					Choice returntostart = new Choice();
					returntostart.Parent = The.Conversation.GetStart();
					returntostart.Ordinal = 800;
					returntostart.Text = "Ok.";
					returntostart.Target = "End";
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
				if(preferences.Count <= 0){
					node.Text = "Alas, I have no preferences!";
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

					if(
						//ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>5 &&
						true){
						acegiak_RomanceChatChoice giftoption = new acegiak_RomanceChatChoice();
						giftoption.Ordinal = 900;
						giftoption.Text = "[Offer A Gift]";
						giftoption.action = "*Gift";
						giftoption.Target = "End";
						giftoption.choiceAction = delegate{
							Gift(XRLCore.Core.Game.Player.Body,  true);
						};
						node.Choices.Add(giftoption);
					}
					if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>=25){
						acegiak_RomanceChatChoice kissoption = new acegiak_RomanceChatChoice();
						kissoption.Ordinal = 910;
						kissoption.Text = "[Propose a Date]";
						kissoption.action = "*Date";
						kissoption.Target = "End";
						kissoption.choiceAction = delegate{
							acegiak_Romancable romancable = ParentObject.GetPart<acegiak_Romancable>();
							if (romancable != null && ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) > 25 && ParentObject != XRLCore.Core.Game.Player.Body)
							{
								XRLCore.Core.Game.Player.Body.GetPart<acegiak_Romancable>().date = ParentObject;
								Popup.Show(ParentObject.ShortDisplayName+" seems amenable to the idea.");
							}
						};
						node.Choices.Add(kissoption);
					}
					if(ParentObject.GetPart<acegiak_Kissable>() != null && ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>=55){
						acegiak_RomanceChatChoice kissoption = new acegiak_RomanceChatChoice();
						kissoption.Ordinal = 910;
						kissoption.Text = "[Attempt to Kiss]";
						kissoption.action = "*Kiss";
						kissoption.Target = "End";
						kissoption.choiceAction = delegate{
											ParentObject.GetPart<acegiak_Kissable>().Kiss(XRLCore.Core.Game.Player.Body);
						};
						node.Choices.Add(kissoption);
					}
				}


			}
			if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) < 1){
				ParentObject.pBrain.SetFeeling(XRLCore.Core.Game.Player.Body,1);
			}

			

			patience--;


			Choice liveanddrink = new Choice();
			liveanddrink.Parent = The.Conversation.GetStart();
			liveanddrink.Ordinal = 99999;
			liveanddrink.Text = "Live and drink.";
			liveanddrink.Target = "End";
			node.Choices.Add(liveanddrink);
			
			node = NameCheck(node);
			
			return node;
		}

		public acegiak_RomanceChatNode NameCheck(acegiak_RomanceChatNode node){
			if(!ParentObject.HasProperName
			&& namegenerated == null
			&& ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>7){
				string text9 = NameMaker.MakeName(ParentObject, null, null, null, null, null, null, null, null, null, null, FailureOkay: false, SpecialFaildown: true);

				namegenerated = text9;
				ParentObject.SetIntProperty("ProperNoun", 1);
				ParentObject.SetIntProperty("Renamed", 1);
				node.Text = "["+ParentObject.The+ParentObject.pRender.DisplayName+" tells you "+ParentObject.its+" name: "+namegenerated+"]\n\n"+node.Text;
				ParentObject.pRender.DisplayName = namegenerated+", "+ParentObject.pRender.DisplayName;
				JournalAPI.AddAccomplishment("&y"+ParentObject.pRender.DisplayName +" shared "+ParentObject.its+" name with you.", "general", null);

			}
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
            // if (E.ID == "GetInventoryActions")
			// {
			// 	if(
			// 		//ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>5 && 
			// 		!ParentObject.IsPlayer()){
			// 		E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("Gift", 'G',  false, "&Wg&yift", "InvCommandGift");
			// 	}
			// }
			// if (E.ID == "InvCommandGift" && Gift(E.GetGameObjectParameter("Owner"), FromDialog: true))
			// {
			// 	E.RequestInterfaceExit();
			// }
			// if (E.ID == "PlayerBeginConversation")
			// {
			// 		// if(patience > 5 && ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>0 && ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)<5){
			// 		// 	ParentObject.pBrain.AdjustFeeling(XRLCore.Core.Game.Player.Body,(int)Math.Min(5,patience-5));
			// 		// }
			// 	}
			// }
			if (E.ID == "VisitConversationNode")
			{
				if(E.GetParameter<ConversationNode>("CurrentNode") != null && E.GetParameter<ConversationNode>("CurrentNode") is acegiak_RomanceChatNode){
					E.SetParameter("CurrentNode",BuildNode() );
				}
			}


            // if (E.ID == "OwnerGetInventoryActions")
			// {
			// 	GameObject gameObjectParameter2 = E.GetGameObjectParameter("Object");
			// 	acegiak_Romancable romancable = gameObjectParameter2.GetPart<acegiak_Romancable>();
			// 	if (romancable != null && gameObjectParameter2.pBrain.GetFeeling(ParentObject) > 25 && gameObjectParameter2 != ParentObject)
			// 	{
			// 		E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("ArrangeDate", 'r',  true, "A&Wr&yrange a Date", "InvCommandArrangeDate");
			// 	}
			// }


            // if (E.ID == "OwnerGetInventoryActions")
			// {
			// 	GameObject gameObjectParameter2 = E.GetGameObjectParameter("Object");
			// 	if (date != null && date.pBrain.GetFeeling(ParentObject) > 25 && date != ParentObject)
			// 	{
			// 		E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("BeginDate", 'j',  true, "Invite "+this.date.ShortDisplayName+" to &Wj&yoin you", "InvCommandBeginDate");
			// 	}
			// }

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
			System.Random r = new System.Random();
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
						if (value != null && value.Count() != 0 && r.Next(0,10) < ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)){
							values.Add(value);
						}
					}
				}
				if (values.Count() <= 3)
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

			if(DateObject.GetPart<Bed>() != null){
				if(ParentObject.GetPart<acegiak_Kissable>() != null){
					float attractionAmount =  ParentObject.GetPart<acegiak_Kissable>().attractionAmount(Date);
					value += attractionAmount;
					if(attractionAmount>0){
						output+= "\nYou sleep with "+ParentObject.The+ParentObject.ShortDisplayName+".";
						ParentObject.ApplyEffect(new Asleep(100));
						Date.ApplyEffect(new Asleep(100));
					}
					if(attractionAmount<0){
						output+= "\n"+ParentObject.The+ParentObject.ShortDisplayName+" isn't attracted to you.";
					}
				}
			}


			Popup.Show(output);
            ParentObject.pBrain.AdjustFeeling(Date,(int)(value*10));
			this.storedFeeling = ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body);
			Date.GetPart<acegiak_Romancable>().date = null;
			if(value <1){
				this.patience -= 2;
			}else{
				this.patience -= 1;
			}
			JournalAPI.AddAccomplishment("&y You took "+ParentObject.a + ParentObject.DisplayNameOnlyDirect +" on a date to "+DateObject.the+DateObject.DisplayNameOnlyDirect+" and "+ParentObject.it+(value>0?" was&G":" was &rnot")+" impressed&y.", "general", null);

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
			Writer.Write(annoyed?1:0);
			Writer.Write(storedFeeling==null?0:1);
			if(storedFeeling != null){
				Writer.Write(storedFeeling.Value);
			}
			Writer.Write(patience);
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


			annoyed = Reader.ReadInt32()>0;
			if(Reader.ReadInt32()>0){
				storedFeeling = Reader.ReadInt32();
			}else{
				storedFeeling = null;
			}
			patience = Reader.ReadInt32();
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