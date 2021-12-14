using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.Language;
using XRL.World.Effects;
using XRL.World.AI.GoalHandlers;


namespace XRL.World.Parts.Skill
{
	[Serializable]
	public class acegiak_Interrupt : BaseSkill
	{
		public Guid ActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry Ability = null;
        
		public acegiak_Interrupt()
		{
			DisplayName = "Interrupt";
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}
		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "CommandAcegiakInterrupt");
			base.Register(Object);
		}



        public bool Interrupt(Cell C, bool flag)
        {
            //IPart.AddPlayerMessage("Interruptmin");

			if (C != null)
			{
				foreach (GameObject target in C.GetObjectsInCell()){

					if (target.IsInvalid() || target.GetPart<Brain>() == null || !target.GetPart<Brain>().IsHostileTowards(ParentObject)){
						continue;
					}

					int check = 18;
					acegiak_Kissable kissable = target.GetPart<acegiak_Kissable>();
					if(kissable != null){
						check += ((int)(kissable.attractionAmount(ParentObject)*3f));
					}
					if(!target.MakeSave("Willpower",check,ParentObject,null,"Hesitation")){


							
							target.ApplyEffect(new acegiak_EffectHesitation());
							CooldownMyActivatedAbility(ActivatedAbilityID, 50);
							acegiak_Romancable romancable = target.GetPart<acegiak_Romancable>();
							if(romancable != null){
								romancable.patience = 10;
							}

            				IPart.AddPlayerMessage(target.DisplayName+" hesitates.");
							target.ParticleText("?");
							return true;
		
						
					}
				}
			}
            return false;

        }
	


		public override bool FireEvent(Event E)
		{
			if (E.ID == "CommandAcegiakInterrupt")
			{
				bool flag = ParentObject.IsMissingTongue();
				if (flag && !ParentObject.HasPart("Telepathy"))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowBlock("You cannot Interrupt without a tongue.");
					}
					return true;
				}
				if (!ParentObject.CheckFrozen(Telepathic: true))
				{
					return true;
				}
				Cell cell = PickDestinationCell(80, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, PickTarget.PickStyle.EmptyCell, null, Snap: true);
				if (cell == null)
				{
					return true;
				}
				if (ParentObject.DistanceTo(cell) > 8 && !ParentObject.HasPart("Telepathy"))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.Show("That is out of range! (" + 8 + " squares)");
					}
					return true;
				}
				if (cell != null)
				{
					int intProperty = ParentObject.GetIntProperty("Horrifying");
					int turns = Math.Max(50 - intProperty * 10, 10);
					CooldownMyActivatedAbility(ActivatedAbilityID, turns);
					Interrupt(cell, flag);
					UseEnergy(1000, "Skill Persuasion Interrupt");
				}
			}
			return base.FireEvent(E);
		}

		public override bool AddSkill(GameObject GO)
		{
			ActivatedAbilityID = AddMyActivatedAbility("Interrupt", "CommandAcegiakInterrupt", "Skill", null, "\u0014");
			return true;
		}

		public override bool RemoveSkill(GameObject GO)
		{
			RemoveMyActivatedAbility(ref ActivatedAbilityID);
			return true;
		}
	}
}
