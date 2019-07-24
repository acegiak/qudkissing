using System;
using XRL.Core;
using XRL.UI;
using System.Collections.Generic;

namespace XRL.World.Parts
{
    public class acegiak_KissingPreferenceResult{
        public string explanation;
        public float amount;
        public string reactPath;
        public Dictionary<string, string> reactVars;

        public acegiak_KissingPreferenceResult(float amount,string explanation,
            string                     reactPath,
            Dictionary<string, string> reactVars = null){
            this.amount = amount;
            this.explanation = explanation;
            this.reactPath = reactPath;
            this.reactVars = reactVars;
        }
    }

	public interface acegiak_KissingPreference
	{
        acegiak_KissingPreferenceResult attractionAmount(GameObject kissee, GameObject kisser);

    }
}