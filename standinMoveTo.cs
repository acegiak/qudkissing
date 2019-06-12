using System;
using XRL.World.AI.Pathfinding;
using XRL.World.Parts;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.AI.GoalHandlers
{
	internal class acegiak_MoveTo : GoalHandler
	{
		public string dZone;

		public int dCx;

		public int dCy;

		public int MaxTurns = -1;

		public bool careful;

		public bool overridesCombat;

		public bool oneShort;

		public bool wandering;

		public acegiak_MoveTo(GameObject go)
		{
			dZone = go.pPhysics.CurrentCell.ParentZone.ZoneID;
			dCx = go.pPhysics.CurrentCell.X;
			dCy = go.pPhysics.CurrentCell.Y;
		}

		public acegiak_MoveTo(GameObject go, bool oneShort)
			: this(go)
		{
			this.oneShort = oneShort;
		}

		public acegiak_MoveTo(Cell C)
		{
			dZone = C.ParentZone.ZoneID;
			dCx = C.X;
			dCy = C.Y;
		}

		public acegiak_MoveTo(Cell C, bool oneShort)
			: this(C)
		{
			this.oneShort = oneShort;
		}

		public acegiak_MoveTo(string ZoneID, int Cx, int Cy, bool careful = false, bool overridesCombat = false, bool oneShort = false, bool wandering = false)
		{
			dZone = ZoneID;
			dCx = Cx;
			dCy = Cy;
			this.careful = careful;
			this.overridesCombat = overridesCombat;
			this.oneShort = oneShort;
			this.wandering = wandering;
		}

		public acegiak_MoveTo(string ZoneID, int Cx, int Cy, int MaxTurns, bool careful = false, bool overridesCombat = false, bool oneShort = false, bool wandering = false)
			: this(ZoneID, Cx, Cy, careful, overridesCombat, oneShort, wandering)
		{
			this.MaxTurns = MaxTurns;
		}

		public override bool CanFight()
		{
			return !overridesCombat;
		}

		public override bool Finished()
		{
			if (!ParentBrain.isMobile())
			{
				return true;
			}
			Cell currentCell = base.ParentObject.CurrentCell;
			if (currentCell != null)
			{
				if (currentCell.X == dCx && currentCell.Y == dCy)
				{
					return true;
				}
				if (oneShort && currentCell.DistanceTo(dCx, dCy) <= 1)
				{
					return true;
				}
			}
			return false;
		}

		public override void TakeAction()
		{
			if (!ParentBrain.isMobile())
			{
				FailToParent();
				return;
			}
			if (ParentBrain.pPhysics.CurrentCell.ParentZone.ZoneID == null)
			{
				Pop();
				return;
			}
			if (dZone == null)
			{
				Pop();
				return;
			}
			Cell currentCell = base.ParentObject.CurrentCell;
			FindPath findPath = new FindPath(currentCell.ParentZone.ZoneID, currentCell.X, currentCell.Y, dZone, dCx, dCy, ParentBrain.limitToAquatic(),  false, ParentBrain.ParentObject, careful);
			ParentBrain.ParentObject.UseEnergy(1000);
			if (findPath.bFound)
			{
				findPath.Directions.Reverse();
				int num = findPath.Directions.Count;
				if (oneShort)
				{
					num--;
				}
				if (MaxTurns > -1)
				{
					Pop();
					if (num > MaxTurns)
					{
						num = MaxTurns;
					}
				}
				for (int i = 0; i < num; i++)
				{
					PushGoal(new acegiak_Step(findPath.Directions[i], careful, overridesCombat, wandering));
				}
			}
			else
			{
				FailToParent();
			}
		}
	}

	[Serializable]
	internal class acegiak_Step : GoalHandler
	{
		public string Dir;

		public bool careful;

		public bool overridesCombat;

		public bool wandering;

		public GameObject Toward;

		public acegiak_Step(string Direction, bool careful = false, bool overridesCombat = false, bool wandering = false, GameObject Toward = null)
		{
			Dir = Direction;
			this.careful = careful;
			this.overridesCombat = overridesCombat;
			this.wandering = wandering;
			this.Toward = Toward;
		}

		public override bool Finished()
		{
			return false;
		}

		public override bool CanFight()
		{
			return !overridesCombat;
		}

		
		private GameObject CellHasFriendly(Cell TargetCell)
		{
			if (TargetCell == null)
			{
				return null;
			}
			bool flag = false;
			foreach (GameObject item in TargetCell.LoopObjectsWithPart("Combat"))
			{
				if (ParentBrain != null && item != ParentBrain.Target)
				{
					flag = true;
					if ((item.IsPlayer() || item.Blueprint != ParentBrain.ParentObject.Blueprint) && !ParentBrain.IsHostileTowards(item))
					{
						flag = false;
					}
					if (flag)
					{
						return item;
					}
				}
			}
			return null;
		}

		private bool CellHasHostile(Cell TargetCell)
		{
			if (TargetCell == null)
			{
				return false;
			}
			bool result = false;
			foreach (GameObject item in TargetCell.LoopObjectsWithPart("Combat"))
			{
				if ((item.IsPlayer() || item.Blueprint != ParentBrain.ParentObject.Blueprint) && ParentBrain.IsHostileTowards(item) && ParentBrain.ParentObject.PhaseAndFlightMatches(item))
				{
					result = true;
				}
			}
			return result;
		}

		private void MoveDirection(string Direction)
		{
			Cell cellFromDirection = ParentBrain.pPhysics.CurrentCell.GetCellFromDirection(Direction, bBuiltOnly: false);
			if (cellFromDirection != null && CellHasFriendly(cellFromDirection) == null)
			{
				try
				{
					ParentBrain.ParentObject.FireEvent(Event.New("CommandMove", "Direction", Direction));
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Step::MoveDirection", x);
				}
			}
		}

		public override void TakeAction()
		{
			if (!ParentBrain.isMobile())
			{
				Pop();
				return;
			}
			if (Toward != null && !Toward.IsValid())
			{
				Toward = null;
				Think("I was trying to go toward someone, but they're gone!");
				FailToParent();
				return;
			}
			Cell cellFromDirection = ParentBrain.pPhysics.CurrentCell.GetCellFromDirection(Dir);
			if (cellFromDirection == null)
			{
				Think("I can't move there!");
				FailToParent();
				return;
			}
			if (wandering && cellFromDirection.HasObjectWithTagOrProperty("WanderStopper"))
			{
				Think("I shouldn't wander there!");
				FailToParent();
				return;
			}
			if (careful && cellFromDirection.NavigationWeight(base.ParentObject, bSmart: true) > 0)
			{
				Think("That's too dangerous!");
				FailToParent();
				return;
			}
			GameObject gameObject = CellHasFriendly(cellFromDirection);
			if (gameObject != null && gameObject.HasPart("Brain"))
			{
				Brain brain = gameObject.GetPart("Brain") as Brain;
				if (gameObject.IsPlayer() || (brain.Target != null && brain.Target == ParentBrain.Target))
				{
					return;
				}
			}
			if (CellHasHostile(cellFromDirection))
			{
				Think("There's something in my way!");
				FailToParent();
			}
			else
			{
				MoveDirection(Dir);
				Pop();
			}
		}
	}

    [Serializable]
	internal class acegiak_WaitWith : GoalHandler
	{
		private int TicksLeft;
		private int StartTicks;
		private GameObject With;

		public acegiak_WaitWith(int Duration,GameObject with)
		{
			TicksLeft = Duration;
			StartTicks = Duration;
			With = with;
		}

		public override void Create()
		{
			Think("I'll wait " + TicksLeft.ToString() + " ticks.");
		}

		public override bool Finished()
		{
			if (TicksLeft <= 0)
			{
				if(ParentBrain.ParentObject.CurrentCell.DistanceTo(With.CurrentCell) <= 2){
					TicksLeft = StartTicks;
					return false;
				}
				return true;
			}
			return false;
		}

		public override void TakeAction()
		{
			ParentBrain.ParentObject.UseEnergy(1000);
			TicksLeft--;
			if (TicksLeft <= 0)
			{
				if(ParentBrain.ParentObject.CurrentCell.DistanceTo(With.CurrentCell) <= 2){
					TicksLeft = StartTicks;
				}else{
					Pop();
				}
			}
		}
	}


    [Serializable]
	internal class acegiak_DateAssess : GoalHandler
	{
		private GameObject DateObject;
		private GameObject Date;
        private bool announced = false;

		public acegiak_DateAssess(GameObject date, GameObject dateobject)
		{
			this.DateObject = dateobject;
			this.Date = date;
		}

		public override void Create()
		{
			Think("I'll assess this date.");
		}

		public override bool Finished()
		{
			
			return announced;
		}

		public override void TakeAction()
		{
			ParentBrain.ParentObject.UseEnergy(1000);
            ParentBrain.ParentObject.GetPart<acegiak_Romancable>().AssessDate(this.Date,this.DateObject);
			announced = true;
			Pop();
			
		}
	}

}


