using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EvaporationPond;

public class PlaceWorker_EvaporationPond : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		List<IntVec3> waterCells;
		List<IntVec3> list = GroundCells(checkingDef, loc, out waterCells);
		if (waterCells == null || waterCells.Any((IntVec3 x) => !IsShallowOceanWater(x, map)))
		{
			return new AcceptanceReport("EP.MustBeOnShallowOceanWater".Translate());
		}
		if (list == null || list.Any((IntVec3 x) => IsWater(x, Find.CurrentMap)))
		{
			return new AcceptanceReport("EP.MustBeOnNonWaterGround".Translate());
		}
		return true;
	}

	private bool IsWater(IntVec3 loc, Map map)
	{
		return map.terrainGrid.TerrainAt(loc).IsWater;
	}

	private bool IsShallowOceanWater(IntVec3 loc, Map map)
	{
		return map.terrainGrid.TerrainAt(loc) == TerrainDefOf.WaterOceanShallow;
	}

	public List<IntVec3> GroundCells(BuildableDef checkingDef, IntVec3 loc, out List<IntVec3> waterCells)
	{
		CellRect cellRect = GenAdj.OccupiedRect(loc, checkingDef.defaultPlacingRot, checkingDef.Size);
		foreach (Rot4 item in RotationsToUse())
		{
			bool flag = true;
			waterCells = cellRect.GetEdgeCells(item).ToList();
			if (waterCells.Any((IntVec3 x) => !IsShallowOceanWater(x, Find.CurrentMap)))
			{
				flag = false;
			}
			if (flag)
			{
				List<IntVec3> copy = waterCells.ListFullCopy();
				IEnumerable<IntVec3> source = cellRect.Cells.Where((IntVec3 x) => !copy.Contains(x));
				return source.ToList();
			}
		}
		waterCells = null;
		return null;
		static IEnumerable<Rot4> RotationsToUse()
		{
			yield return new Rot4(0);
			yield return new Rot4(1);
			yield return new Rot4(2);
			yield return new Rot4(3);
		}
	}

	public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		List<IntVec3> waterCells;
		List<IntVec3> list = GroundCells(def, loc, out waterCells);
		if (list != null)
		{
			GenDraw.DrawFieldEdges(list.ToList(), Designator_Place.CanPlaceColor.ToOpaque(), (float?)null);
			GenDraw.DrawFieldEdges(waterCells, Designator_Place.CanPlaceColor.ToOpaque(), (float?)null);
		}
	}
}
