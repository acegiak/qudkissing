using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using System.Linq;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_PartPreference : acegiak_KissingPreference
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


            this.BodyPart = part.TypeModel().Type;
            this.Amount =  (float)((random.NextDouble()*2)-1);
        }
        public acegiak_PartPreference(string partname, float amount){
            this.BodyPart = partname;
            this.Amount = amount;
        }

        public override acegiak_KissingPreferenceResult attractionAmount(GameObject kissee, GameObject GO){
            bool has = false;
            Body part = GO.GetPart<Body>();

            //IPart.AddPlayerMessage("They "+(Amount>0?"likes ":"dislikes ")+this.BodyPart);

            string reactPath = "part." + this.BodyPart;
            if (part == null)
            {
                //IPart.AddPlayerMessage("You have no body.");

				has = false;
                reactPath += ".none";
            }else if (part.GetDismemberedPartByType(this.BodyPart) != null){
                has = false;
                reactPath = ".missing";
            } else {
               // IPart.AddPlayerMessage("You have a body.");

                var parts = part.GetPart(this.BodyPart);
                //BodyPart bpart = (parts.Count() ? parts[Stat.Rnd2.Next(parts.Count())] : null);
                has = (parts.Count() != 0);
                reactPath += (has ? ".have" : ".none");

                //IPart.AddPlayerMessage("You "+(has?"have ":"have no ")+this.BodyPart);
            }

            float result = Amount * (has?1:-1);
            string explain = ((result>0)?"is attracted to":"is &rnot attracted to")+" your "+((has)?"":"&rlack of ")+BodyPart;
            var vars = new Dictionary<string, string>();
            vars["*part*"] = BodyPart.ToLower();
            vars["*Part*"] = BodyPart;
            return new acegiak_KissingPreferenceResult(result,explain,reactPath,vars);
        }


        public override void Save(SerializationWriter Writer){
            base.Save(Writer);
            Writer.Write(this.BodyPart);
            Writer.Write(Amount);
        }

        public override void Load(SerializationReader Reader){
            base.Load(Reader);
            this.BodyPart = Reader.ReadString();
            this.Amount = Reader.ReadSingle();
        }

    }
}