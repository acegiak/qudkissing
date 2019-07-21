using System;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts
{
    public class acegiak_KissingPreferenceResult{
        public string explanation;
        public float amount;
        public string reactPath;

        public acegiak_KissingPreferenceResult(float amount,string explanation,string reactPath){
            this.amount = amount;
            this.explanation = explanation;
            this.reactPath = reactPath;
        }
    }

	public interface acegiak_KissingPreference
	{
        acegiak_KissingPreferenceResult attractionAmount(GameObject kissee, GameObject kisser);

    }
}