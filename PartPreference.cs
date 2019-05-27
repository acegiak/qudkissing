using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using Mono.CSharp;
using System.Linq;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_PartPreference : acegiak_RomancePreference
	{
        string BodyPart = "face";
        float Amount = 0.1f;

        public acegiak_PartPreference(GameObject GO){
            if(GO == null){
                return;
            }
            Body body = GO.GetPart<Body>();
            if(body == null){
                return;
            }
            BodyPart bp = body.GetBody();
            if(bp == null){
                return;
            }
            Random random = new Random();
            List<BodyPart> parts = bp.GetParts().Where(x => !x.Abstract && !x.Extrinsic).ToList();
            BodyPart part = parts[random.Next(parts.Count-1)];


            this.BodyPart = part.Name;
            this.Amount =  (float)((random.NextDouble()*2)-1);
        }
        public acegiak_PartPreference(string partname, float amount){
            this.BodyPart = partname;
            this.Amount = amount;
        }

        public acegiak_RomancePreferenceResult attractionAmount(GameObject GO){
            bool has = false;
            Body part = GO.GetPart<Body>();

            //IPart.AddPlayerMessage("They "+(Amount>0?"likes ":"dislikes ")+this.BodyPart);


            if (part == null)
            {
                //IPart.AddPlayerMessage("You have no body.");

				has = false;
            }else{
               // IPart.AddPlayerMessage("You have a body.");

                BodyPart bpart = part.GetPartByName(this.BodyPart);
                has = bpart != null;

                //IPart.AddPlayerMessage("You "+(has?"have ":"have no ")+this.BodyPart);
            }

            float result = Amount * (has?1:-1);
            string explain = ((result>0)?"is attracted to":"is &rnot attracted to")+" your "+((has)?"":"&rlack of ")+BodyPart;
            return new acegiak_RomancePreferenceResult(result,explain);
        }

    }
}