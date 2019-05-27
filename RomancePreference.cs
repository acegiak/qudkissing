using System;
using XRL.Core;
using XRL.UI;
using Mono.CSharp;

namespace XRL.World.Parts
{
    public class acegiak_RomancePreferenceResult{
        public string explanation;
        public float amount;
        public acegiak_RomancePreferenceResult(float amount,string explanation){
            this.amount = amount;
            this.explanation = explanation;
        }
    }

	public interface acegiak_RomancePreference
	{
        acegiak_RomancePreferenceResult attractionAmount(GameObject GO);

    }
}