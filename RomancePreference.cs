using System;
using XRL.Core;
using XRL.UI;
using Mono.CSharp;

namespace XRL.World.Parts
{

	public interface acegiak_RomancePreference
	{
        Tuple<float,string> attractionAmount(GameObject GO);

    }
}