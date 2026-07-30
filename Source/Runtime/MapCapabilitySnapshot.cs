using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TechSenseFilters.Runtime
{
    internal sealed class MapCapabilitySnapshot
    {
        private readonly Dictionary<ThingDef, SourceAvailability> sources;
        private readonly IReadOnlyList<Pawn> capablePawns;

        private MapCapabilitySnapshot(
            Dictionary<ThingDef, SourceAvailability> sources,
            IReadOnlyList<Pawn> capablePawns,
            int gameTick)
        {
            this.sources = sources;
            this.capablePawns = capablePawns;
            GameTick = gameTick;
        }

        internal int GameTick { get; }

        internal bool HasSource(ThingDef sourceDef)
        {
            return sourceDef != null && sources.ContainsKey(sourceDef);
        }

        internal bool HasUsableSource(ThingDef sourceDef)
        {
            return sourceDef != null &&
                sources.TryGetValue(
                    sourceDef,
                    out SourceAvailability availability) &&
                availability.Usable;
        }

        internal bool HasCapablePawn(RecipeDef recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            bool hasWorkRequirement = recipe.requiredGiverWorkType != null;
            bool hasSkillRequirement =
                recipe.skillRequirements != null &&
                recipe.skillRequirements.Count > 0;
            if (!hasWorkRequirement &&
                !hasSkillRequirement &&
                !recipe.mechanitorOnlyRecipe)
            {
                return true;
            }

            for (int i = 0; i < capablePawns.Count; i++)
            {
                Pawn pawn = capablePawns[i];
                if (recipe.mechanitorOnlyRecipe &&
                    !MechanitorUtility.IsMechanitor(pawn))
                {
                    continue;
                }

                if (recipe.requiredGiverWorkType != null &&
                    pawn.WorkTypeIsDisabled(recipe.requiredGiverWorkType))
                {
                    continue;
                }

                bool skillsSatisfied = true;
                if (recipe.skillRequirements != null)
                {
                    for (int requirementIndex = 0;
                        requirementIndex < recipe.skillRequirements.Count;
                        requirementIndex++)
                    {
                        if (!recipe.skillRequirements[requirementIndex]
                            .PawnSatisfies(pawn))
                        {
                            skillsSatisfied = false;
                            break;
                        }
                    }
                }

                if (skillsSatisfied)
                {
                    return true;
                }
            }

            return false;
        }

        internal static MapCapabilitySnapshot Capture(
            Map map,
            DefinitionProductionIndex index,
            int gameTick)
        {
            var sources = new Dictionary<ThingDef, SourceAvailability>();
            var pawns = new List<Pawn>();
            if (map == null)
            {
                return new MapCapabilitySnapshot(sources, pawns, gameTick);
            }

            List<Building> buildings =
                map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                Building building = buildings[i];
                if (building == null ||
                    !building.Spawned ||
                    !index.IsProductionSource(building.def))
                {
                    continue;
                }

                bool usable = !(building is IBillGiver billGiver) ||
                    billGiver.CurrentlyUsableForBills();
                if (sources.TryGetValue(
                    building.def,
                    out SourceAvailability existing))
                {
                    existing.Usable |= usable;
                    sources[building.def] = existing;
                }
                else
                {
                    sources.Add(
                        building.def,
                        new SourceAvailability { Usable = usable });
                }
            }

            IReadOnlyList<Pawn> spawnedPawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < spawnedPawns.Count; i++)
            {
                Pawn pawn = spawnedPawns[i];
                if (pawn != null &&
                    pawn.Faction == Faction.OfPlayer &&
                    !pawn.Dead &&
                    !pawn.Downed &&
                    !pawn.InMentalState &&
                    (pawn.IsColonist || pawn.IsColonyMech))
                {
                    pawns.Add(pawn);
                }
            }

            return new MapCapabilitySnapshot(sources, pawns, gameTick);
        }

        private struct SourceAvailability
        {
            internal bool Usable;
        }
    }
}
