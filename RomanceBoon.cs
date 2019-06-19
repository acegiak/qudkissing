using System;
using XRL.Core;
using XRL.UI;

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

	public interface acegiak_RomanceBoon
	{
        bool BoonReady(GameObject player);
        bool BoonPossible(GameObject talker);
        acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node);

    }
}