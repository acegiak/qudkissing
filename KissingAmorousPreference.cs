using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_AmorousPreference : acegiak_KissingPreference
	{
        float Amount = 0.1f;

        public acegiak_AmorousPreference(GameObject GO){
            Random random = new Random();

            this.Amount =  (float)((random.NextDouble()*1.5)-1);
        }


        public acegiak_KissingPreferenceResult attractionAmount(GameObject kissee, GameObject GO){
            string explain = ((Amount>0)?"is generally amorous":"is &rnot very amorous");
            return new acegiak_KissingPreferenceResult(Amount,explain);
        }

    }
}