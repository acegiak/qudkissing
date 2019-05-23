using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using Mono.CSharp;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_AmorousPreference : acegiak_RomancePreference
	{
        float Amount = 0.1f;

        public acegiak_AmorousPreference(GameObject GO){
            Random random = new Random();

            this.Amount =  (float)((random.NextDouble()*1.5)-1);
        }


        public Tuple<float,string> attractionAmount(GameObject GO){
            string explain = ((Amount>0)?"generally amorous":"is &rnot very amorous");
            return new Tuple<float,string>(Amount,explain);
        }

    }
}