
namespace XRL.World.AI{
	public class acegiak_FlirtOpinion : IOpinionSubject
	{
		public override int BaseValue => 1;

		public override string GetText(GameObject Actor)
		{
			return "Attempted to woo me.";
		}
	}
}