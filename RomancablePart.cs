using System;
using XRL.Core;
using XRL.UI;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Mono.CSharp;
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


		private static Random rng = new Random();  
		public acegiak_Romancable()
		{
			base.Name = "acegiak_Romancable";
			//DisplayName = "Kissable";
			
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
			base.Register(Object);
		}

        public bool Gift(GameObject who, bool FromDialog){

			haveFavoriteThings();
            //    Popup.Show(FavoriteThings.Keys.Aggregate(  "",   (current, next) => current + ", " + next));


            Inventory part2 = XRLCore.Core.Game.Player.Body.GetPart<Inventory>();
            List<XRL.World.GameObject> ObjectChoices = new List<XRL.World.GameObject>();
            List<string> ChoiceList = new List<string>();
            List<char> HotkeyList = new List<char>();
            char ch = 'a';
            part2.ForeachObject(delegate(XRL.World.GameObject GO)
            {
                // if (GO.HasPart("Examiner") && GO.HasPart("TinkerItem") && GO.GetPart<Examiner>().Complexity > 0)
                // {
                    ObjectChoices.Add(GO);
                    HotkeyList.Add(ch);
                    ChoiceList.Add(GO.DisplayName);
                    ch = (char)(ch + 1);
                // }
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
			int result = assessGift(ObjectChoices[num12]);

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

        public int assessGift(GameObject GO){
			haveFavoriteThings();
            int value = (rng.Next(1,6) -3);
			if(FavoriteThings.ContainsKey(GO.pRender.DisplayName)){
				value += 2*FavoriteThings[GO.pRender.DisplayName];
				FavoriteThings[GO.pRender.DisplayName] -= 1;
			}else{
				value -= 1;
			}
            return value*10;
        }


		public void haveFavoriteThings(){
			if(FavoriteThings != null){
				return;
			}
			FavoriteThings = new Dictionary<string, int>();
			int howmany = rng.Next(1,10);
			for(int i= 0;i<howmany;i++){
				GameObject gameObject = GameObjectFactory.Factory.CreateSampleObject(EncountersAPI.GetARandomDescendentOf("Item"));
				int amount = rng.Next(1,3);
				FavoriteThings[gameObject.pRender.DisplayName] = amount;
			}

		}
		

		public string DescribePreference(GameObject who){
			

			string preference = ParentObject.pBrain.GetFeeling(who) > 50 ?"Oh yes ok! ":"Hmmm... ";
			int max = (int)Math.Max(1,Math.Floor((double)(ParentObject.pBrain.GetFeeling(who)/10)));
			foreach(string thing in FavoriteThings.Keys.ToList()){
				if(max <= 0){
					break;
				}
				string[] list2 = new string[]
						{
							"Once, I saw a =item=! ",
							"I wonder what the perfect =item= would be like? ",
							"Have you ever seen a =item=? ",
							"You should get a =item=. ",
							"Would you like a =item=? ",
							"Have you got a =item=? ",
							"I think I'd like a =item=. ",
							"Perhaps I could make a =item=. ",
							"What does a =item= look like? ",
							"I don't think you know what a =item= is. ",
							"I had a dream about a =item=. "
						};
						preference += "&y"+list2.GetRandomElement().Replace("=item=",thing+"&y");
						max--;
			}
			return preference;
		}

		public override bool FireEvent(Event E){
            if (E.ID == "GetInventoryActions")
			{
				E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("Gift", 'G',  false, "&Wg&yift", "InvCommandGift", 10);
			}
			if (E.ID == "InvCommandGift" && Gift(E.GetGameObjectParameter("Owner"), FromDialog: true))
			{
				E.RequestInterfaceExit();
			}
			if (E.ID == "PlayerBeginConversation")
			{
				if(E.GetParameter<Conversation>("Conversation").NodesByID != null
				&& E.GetParameter<Conversation>("Conversation").NodesByID.Count >0
				&& !E.GetParameter<Conversation>("Conversation").NodesByID.ContainsKey("acegiak_aboutme")
				&& E.GetParameter<GameObject>("Speaker") != null
				&& E.GetParameter<GameObject>("Speaker").GetPart<acegiak_Romancable>() != null){

					string StartID = E.GetParameter<Conversation>("Conversation").NodesByID.Keys.ToArray()[0];
					if(E.GetParameter<Conversation>("Conversation").NodesByID.ContainsKey("Start")){
						StartID = "Start";
					}
					E.GetParameter<GameObject>("Speaker").GetPart<acegiak_Romancable>().haveFavoriteThings();

					ConversationNode aboutme = new ConversationNode();
					aboutme.ID = "acegiak_aboutme";
					aboutme.Text = E.GetParameter<GameObject>("Speaker").GetPart<acegiak_Romancable>().DescribePreference(ParentObject);


					ConversationChoice returntostart = new ConversationChoice();
					returntostart.Text = "Ok.";
					returntostart.GotoID = "End";
					returntostart.ParentNode = aboutme;

					aboutme.Choices.Add(returntostart);

					ConversationChoice romanticEnquiry = new ConversationChoice();
					romanticEnquiry.ParentNode = E.GetParameter<Conversation>("Conversation").NodesByID[StartID];
					romanticEnquiry.ID = "acegiak_askaboutme";
					romanticEnquiry.Text = "Tell me a little about yourself.";
					romanticEnquiry.GotoID = "acegiak_aboutme";
					
					
					E.GetParameter<Conversation>("Conversation").AddNode(aboutme);
					foreach(ConversationNode node in E.GetParameter<Conversation>("Conversation").StartNodes){
						node.Choices.Add(romanticEnquiry);
					}
					//E.GetParameter<Conversation>("Conversation").NodesByID[StartID].Choices.Add(romanticEnquiry);
				}
			}

			return base.FireEvent(E);
		}
	}
}