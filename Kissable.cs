using System;
using XRL.Core;
using XRL.UI;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Mono.CSharp;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_Kissable : IPart
	{
		public string useFactionForFeelingFloor;

		public bool kissableIfPositiveFeeling;

		private bool bOnlyAllowIfLiked = true;

		private List<acegiak_RomancePreference> preferences = null;



		public acegiak_Kissable()
		{
			base.Name = "acegiak_Kissable";
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
			Object.RegisterPartEvent(this, "InvCommandKiss");
			base.Register(Object);
		}

		public bool Kiss(GameObject who, bool FromDialog)
		{
            
                if (!hasPart(ParentObject,"face")){
					if (who.IsPlayer())
					{
						Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&y doesn't have a face.");
					}
					return true;
                }
				if (!hasPart(who,"face")){
					if (who.IsPlayer())
					{
						Popup.Show("You don't have a face.");
					}
					return true;
                }

                
			if (kissableIfPositiveFeeling && ParentObject.pBrain.GetFeeling(who) < 0)
			{
				if (who.IsPlayer())
				{
					Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&y" + ParentObject.GetVerb("shy") + " away from you.");
				}
				return true;
			}
			if (useFactionForFeelingFloor == null)
			{
				if (bOnlyAllowIfLiked && who != null && ParentObject.pBrain.GetFeeling(who) < 50)
				{
					if (who.IsPlayer())
					{
						Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&y" + ParentObject.GetVerb("shy") + " away from you.");
					}
					return true;
				}
			}
			else if (!kissableIfPositiveFeeling && (ParentObject.pBrain.GetFeeling(who) < 0 || (who.IsPlayer() && Math.Max(XRLCore.Core.Game.PlayerReputation.getFeeling(useFactionForFeelingFloor), ParentObject.pBrain.GetFeeling(who)) < 50)))
			{
				if (who.IsPlayer())
				{
					Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&Y shies away from you.");
				}
				return true;
			}
			if(!isAttractedTo(who)){
				if (who.IsPlayer())
				{
					Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&Y isn't attracted to you.");
				}
				return true;
            }

            
			string verb = "kiss";
			GameObject parentObject = ParentObject;
			bool fromDialog = FromDialog;
			IPart.XDidYToZ(who, verb, parentObject, null, null, fromDialog);
			if (who.IsPlayer())
			{
				if (ParentObject.HasPropertyOrTag("SpecialKissResponse"))
				{
					Popup.Show(ParentObject.GetTag("SpecialKissResponse"));
				}
				else
				{
					Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&y " + ParentObject.GetPropertyOrTag("KissResponse", "&Mkisses you back") + ".");
				}
			}
			ParentObject.Heartspray();
			who.UseEnergy(1000, "Kissing");
			ParentObject.FireEvent(Event.New("ObjectKissed", "Object", ParentObject, "Kisser", who));
			return true;
		}

		public void havePreference(){
			if(preferences == null){
				preferences = new List<acegiak_RomancePreference>();
				Random random = new Random();
				int count = random.Next(5);
				for(int i = 0; i<count;i++){
					switch (random.Next(3)){
					case 0:
						preferences.Add(new acegiak_PartPreference(ParentObject));
						break;
					case 1:
						preferences.Add(new acegiak_AmorousPreference(ParentObject));
						break;
					case 2:
						preferences.Add(new acegiak_StatPreference(ParentObject));
						break;
					}
				}
			}
		}


        public bool isAttractedTo(GameObject kisser){
			havePreference();
			float sum = 0; 
			IPart.AddPlayerMessage("" + ParentObject.the + ParentObject.DisplayNameOnly + "&c examines you.");
			List<string> many = new List<string>();
			foreach(acegiak_RomancePreference pref in preferences){
				Tuple<float,string> output = pref.attractionAmount(kisser);
				sum += output.Item1;
				if(kisser.IsPlayer() && !many.Contains(output.Item2)){
					IPart.AddPlayerMessage("" + ParentObject.the + ParentObject.DisplayNameOnly + " "+output.Item2+".");
				}
				many.Add(output.Item2);

			}
            if(sum > 0){
                return true;
            }
            return false;
        }

        private bool hasPart(GameObject GO, string partname){
            Body part = GO.GetPart<Body>();
            if (part == null)
            {
				return false;
            }

			BodyPart bpart = part.GetBody();

			return bpart != null;
        }

		private bool partHasPart(XRL.World.BodyPart part, string partname){
			if(part.Type == partname){
				return true;
			}
			foreach(XRL.World.BodyPart subpart in part.Parts){
				if(subpart != part && partHasPart(subpart,partname)){
					return true;
				}
			}
			return false;
		}


		public override bool FireEvent(Event E){
            if (E.ID == "GetInventoryActions")
			{
				E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("Kiss", 'k',  false, "&Wk&yiss", "InvCommandKiss", 10);
			}
			if (E.ID == "InvCommandKiss" && Kiss(E.GetGameObjectParameter("Owner"), FromDialog: true))
			{
				E.RequestInterfaceExit();
			}
			return base.FireEvent(E);
		}
	}
}
