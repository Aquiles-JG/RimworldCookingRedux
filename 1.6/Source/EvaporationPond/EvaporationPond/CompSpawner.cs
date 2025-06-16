using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EvaporationPond;

public class CompSpawner : ThingComp
{
	private int ticksUntilSpawn;

	public CompProperties_Spawner PropsSpawner => (CompProperties_Spawner)props;

	private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!respawningAfterLoad)
		{
			ResetCountdown();
		}
	}

	public override void CompTick()
	{
		TickInterval(1);
	}

	public override void CompTickRare()
	{
		TickInterval(250);
	}

	private void TickInterval(int interval)
	{
		if (!parent.Spawned)
		{
			return;
		}
		CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
		if (comp != null)
		{
			if (!comp.Awake)
			{
				return;
			}
		}
		else if (parent.Position.Fogged(parent.Map))
		{
			return;
		}
		if (!PropsSpawner.requiresPower || PowerOn)
		{
			ticksUntilSpawn -= interval;
			CheckShouldSpawn();
		}
	}

	private void CheckShouldSpawn()
	{
		if (ticksUntilSpawn <= 0)
		{
			ResetCountdown();
			TryDoSpawn();
		}
	}

	public bool TryDoSpawn()
	{
		if (!parent.Spawned)
		{
			return false;
		}
		if (PropsSpawner.spawnMaxAdjacent >= 0)
		{
			int num = 0;
			for (int i = 0; i < 9; i++)
			{
				IntVec3 c = parent.Position + GenAdj.AdjacentCellsAndInside[i];
				if (!c.InBounds(parent.Map))
				{
					continue;
				}
				List<Thing> thingList = c.GetThingList(parent.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (thingList[j].def == PropsSpawner.thingToSpawn)
					{
						num += thingList[j].stackCount;
						if (num >= PropsSpawner.spawnMaxAdjacent)
						{
							return false;
						}
					}
				}
			}
		}
		if (TryFindSpawnCell(parent, PropsSpawner.thingToSpawn, PropsSpawner.spawnCount, out var result))
		{
			Thing thing = ThingMaker.MakeThing(PropsSpawner.thingToSpawn);
			thing.stackCount = PropsSpawner.spawnCount;
			if (thing == null)
			{
				Log.Error("Could not spawn anything for " + parent);
			}
			if (PropsSpawner.inheritFaction && thing.Faction != parent.Faction)
			{
				thing.SetFaction(parent.Faction);
			}
			Thing thing2 = default(Thing);
			GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Direct, out thing2, (Action<Thing, int>)null, (Predicate<IntVec3>)null, default(Rot4));
			if (PropsSpawner.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
			{
				Messages.Message("MessageCompSpawnerSpawnedItem".Translate(PropsSpawner.thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent);
			}
			return true;
		}
		return false;
	}

	public static bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
	{
		foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(parent).InRandomOrder())
		{
			parent.Map.debugDrawer.FlashCell(item);
			if (!item.Walkable(parent.Map))
			{
				continue;
			}
			Building edifice = item.GetEdifice(parent.Map);
			if ((edifice != null && thingToSpawn.IsEdifice()) || edifice is Building_Door { FreePassage: false })
			{
				continue;
			}
			bool flag = false;
			List<Thing> thingList = item.GetThingList(parent.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def.category == ThingCategory.Item && (thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			result = item;
			return true;
		}
		result = IntVec3.Invalid;
		return false;
	}

	private void ResetCountdown()
	{
		ticksUntilSpawn = PropsSpawner.spawnIntervalRange.RandomInRange;
	}

	public override void PostExposeData()
	{
		string text = (PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (PropsSpawner.saveKeysPrefix + "_"));
		Scribe_Values.Look(ref ticksUntilSpawn, text + "ticksUntilSpawn", 0);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (Prefs.DevMode)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEBUG: Spawn " + PropsSpawner.thingToSpawn.label,
				icon = (Texture)(object)TexCommand.DesirePower,
				action = delegate
				{
					ResetCountdown();
					TryDoSpawn();
				}
			};
		}
	}

	public override string CompInspectStringExtra()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (PropsSpawner.writeTimeLeftToSpawn && (!PropsSpawner.requiresPower || PowerOn))
		{
			return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(PropsSpawner.thingToSpawn, null, PropsSpawner.spawnCount)).Resolve() + ": " + ticksUntilSpawn.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
		}
		return null;
	}
}
