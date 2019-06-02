using System;
using XRL.Core;
using XRL.UI;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Qud.API;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_Romancable : IPart
	{
		public string useFactionForFeelingFloor;

		public bool kissableIfPositiveFeeling;

		private bool bOnlyAllowIfLiked = true;

		private Dictionary<string,int> FavoriteThings = null;

		private List<acegiak_RomancePreference> preferences = null;

		private int lastseen = 0;
		private int patience = 5;



		private static Random rng = new Random();  
		public acegiak_Romancable()
		{
			base.Name = "acegiak_Romancable";
			
		}

		public void havePreference(){
			if(preferences == null){
				preferences = new List<acegiak_RomancePreference>();
				Random random = new Random();
				int count = random.Next(4)+1;
				for(int i = 0; i<count;i++){
					switch (random.Next(2)){
					case 0:
						preferences.Add(new acegiak_WeaponPreference(this));
						break;
					case 1:
						preferences.Add(new acegiak_MissilePreference(this));
						break;
					}
				}
			}
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
			int result = assessGift(ObjectChoices[num12],who);
			

            XRL.World.Event event2 = XRL.World.Event.New("SplitStack", "Number", 1);
            event2.AddParameter("OwningObject", XRLCore.Core.Game.Player.Body);
            ObjectChoices[num12].FireEvent(event2);
            if (!part2.FireEvent(XRL.World.Event.New("CommandRemoveObject", "Object", ObjectChoices[num12])))
            {
                Popup.Show("You can't give that object.");
                return false;
            }
            ParentObject.pBrain.AdjustFeeling(who,result);
				if (who.IsPlayer())
				{
					Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + (result>0?"&Y likes the "+ObjectChoices[num12].pRender.DisplayName+".":"&r is unimpressed by the "+ObjectChoices[num12].pRender.DisplayName+"."));
				}
            return true;
        }

        public int assessGift(GameObject GO,GameObject who){
			havePreference();
            float value = (rng.Next(1,4) -2);

			foreach(acegiak_RomancePreference preferece in preferences){
				acegiak_RomancePreferenceResult result = preferece.GiftRecieve(GO,who);
				if(result != null){
					value += result.amount;
					IPart.AddPlayerMessage("" + ParentObject.the + ParentObject.DisplayNameOnly + "&Y "+result.explanation);
				}
			}
            return (int)(value*10);
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
					aboutme.Text = ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>=50?"Oh, yes ok!":"Hmm...";
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
			node.Choices.Clear();
			Random r = new Random();
			IPart.AddPlayerMessage("They are:"+ParentObject.pBrain.GetOpinion(XRLCore.Core.Game.Player.Body)+": "+ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body).ToString()+" patience:"+patience.ToString());
			
			if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) < 1 && patience > 0){
					ParentObject.pBrain.SetFeeling(XRLCore.Core.Game.Player.Body,1);
			}

			if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body) < 1 || patience <= 0){
					List<string> Stories = new List<string>(new string[] {
                        "Sorry, I have other things to do.",
                        "Anyway, I'm very busy.",
                        "Maybe we could talk some more another time?"
                    });
					node.Text = node.Text + "\n\n"+ Stories[r.Next(0,Stories.Count-1)];

					ConversationChoice returntostart = new ConversationChoice();
					returntostart.Ordinal = 800;
					returntostart.Text = "Ok.";
					returntostart.GotoID = node.ParentConversation.StartNodes[0].ID;
					returntostart.ParentNode = node;
					node.Choices.Add(returntostart);
			}else{
				node = preferences[r.Next(0,preferences.Count-1)].BuildNode(node);
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

		public string GetStory(){
			Random r = new Random();
			string story = preferences[r.Next(0,preferences.Count-1)].GetStory();
			return story;
		}


		public override bool FireEvent(Event E){
            if (E.ID == "GetInventoryActions")
			{
				if(ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body)>10){
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
					int newPatience = (int)Math.Floor((float)(XRLCore.Core.Game.TimeTicks - lastseen)/1200);
					if(lastseen == 0){
						newPatience = 0;
					}
					lastseen = (int)XRLCore.Core.Game.TimeTicks;
					patience += newPatience;
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

			return base.FireEvent(E);
		}
	}
}