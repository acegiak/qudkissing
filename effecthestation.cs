using XRL.UI;
using XRL.Rules;
using System.Collections.Generic;
using XRL.World.Parts;

using XRL.Core;

namespace XRL.World.Effects
{
	public class acegiak_EffectHesitation : BasicCookingEffect
	{
		public int storedXp;
		public int storedRep;

		public bool target = false;

		public acegiak_EffectHesitation()
		{
			base.DisplayName = "&Mhesitation";
			base.Duration = 5;
		}

	

		public override string GetDetails()
		{
			return "Momentarily non-hostile.";
		}

		public override void ApplyEffect(GameObject ParentObject)
		{
			storedRep = ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body);
			storedXp = ParentObject.Statistics["XPValue"].BaseValue;
			// if (ParentObject.pBrain.Target == XRLCore.Core.Game.Player.Body)
			// {
			// 	ParentObject.pBrain.Goals.Clear();
			// 	target = true;
			// }
			ParentObject.pBrain.StopFighting(XRLCore.Core.Game.Player.Body,true);
			ParentObject.Statistics["XPValue"].BaseValue = 0;
			ParentObject.pBrain.SetFeeling(XRLCore.Core.Game.Player.Body, 0);
		}

		public override void RemoveEffect(GameObject ParentObject)
		{
			ParentObject.Statistics["XPValue"].BaseValue = storedXp;
			int current = ParentObject.pBrain.GetFeeling(XRLCore.Core.Game.Player.Body);
			ParentObject.pBrain.SetFeeling(XRLCore.Core.Game.Player.Body, current+storedRep);
			if(target){
				ParentObject.pBrain.Goals.Clear();
				ParentObject.pBrain.Target = XRLCore.Core.Game.Player.Body;
			}
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == BeginTakeActionEvent.ID;
			}
			return true;
		}


		public override bool HandleEvent(BeginTakeActionEvent E)
		{
			
				base.Duration--;
			
			return base.HandleEvent(E);
		}

	}
}
