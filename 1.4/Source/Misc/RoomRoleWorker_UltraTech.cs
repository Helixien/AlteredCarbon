using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{

    public class RoomRoleWorker_UltraTech : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			int count = 0;
			List<Thing> allContainedThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < allContainedThings.Count; i++)
			{
				Thing th = allContainedThings[i];
				if (th.IsUltraTech())
				{
					count++;
				}
			}
			return 100001f * (float)count;
		}
	}
}

