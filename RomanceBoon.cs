using System;
using XRL.Core;
using XRL.UI;
using XRL.World;

namespace XRL.World.Parts
{
    // public class acegiak_RomancePreferenceResult{
    //     public string explanation;
    //     public float amount;
    //     public acegiak_RomancePreferenceResult(float amount,string explanation){
    //         this.amount = amount;
    //         this.explanation = explanation;
    //     }
    // }

	[Serializable]
	public abstract class acegiak_RomanceBoon
	{
        [NonSerialized]
        public acegiak_Romancable Romancable = null;
        public abstract bool BoonReady(GameObject player);
        public abstract bool BoonPossible(GameObject talker);
        public abstract acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node);
        public void setRomancable(acegiak_Romancable romancable){
            this.Romancable = romancable;
        }
        
        public virtual void Save(SerializationWriter Writer){
            Writer.Write(this.GetType().FullName);
        }
        
        public static void Read(SerializationReader Reader, acegiak_Romancable romancable){
            string classname = Reader.ReadString();
            Type type = Type.GetType(classname);
            acegiak_RomanceBoon boon = (acegiak_RomanceBoon)Activator.CreateInstance(type,romancable);
            boon.setRomancable(romancable);
            romancable.boons.Add(boon);             
        }

    }
}