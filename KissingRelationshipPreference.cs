using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_RelationshipPreference : acegiak_KissingPreference
	{
        float Amount = 0.1f;

        public acegiak_RelationshipPreference(GameObject GO){
            Random random = new Random();

            this.Amount =  (float)(((random.NextDouble()*2)-1)*0.1);
        }


        public override acegiak_KissingPreferenceResult attractionAmount(GameObject kissee, GameObject GO){
            float modifier = (kissee.pBrain.GetFeeling(GO) -10);
            string reactPath = ((modifier>0) ? "relationship.hi" : "relationship.lo");
            return new acegiak_KissingPreferenceResult(
                Amount*modifier,
                (modifier*this.Amount>0?"likes":"doesn't like")+" that you"+(modifier >0?"":" don't")+" know them well",
                reactPath);
        }

        public override void Save(SerializationWriter Writer){
            base.Save(Writer);
            Writer.Write(Amount);
        }

        public override void Load(SerializationReader Reader){
            base.Load(Reader);
            this.Amount = Reader.ReadSingle();
        }

    }
}