using XRL.UI;
using XRL.Rules;
using System.Collections.Generic;

namespace XRL.World.Parts.Effects
{
	public class acegiak_EffectGay : BasicCookingEffect
	{
		public int percent;
		public acegiak_EffectGay()
		{
			base.DisplayName = "&Mgay";
			base.Duration = 3600;
			this.percent=Stat.Rnd2.Next(10)+1;
		}

		string GayWord(){
			List<string> gaylist = new List<string>(new string[] { "Happy", "Glowing", "Joyous", "Merry","Amorous","Free","Blushing","Emotional" });
			return gaylist[Stat.Rnd2.Next(gaylist.Count)];
		}

		public override string GetDetails()
		{
			return GayWord()+", "+GayWord()+". +"+percent.ToString()+"% XP gained";
		}

		public override void ApplyEffect(GameObject Object)
		{
			Object.RegisterEffectEvent(this, "Gay");
		}

		public override void RemoveEffect(GameObject Object)
		{
			Object.UnregisterEffectEvent(this, "Gay");
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "Gay")
			{
				E.AddParameter("Amount", (int)((float)E.GetIntParameter("Amount") * ((percent/100)+1.0f)));
				return true;
			}
			return base.FireEvent(E);
		}
	}
}
