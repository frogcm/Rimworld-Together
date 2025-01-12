﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using Verse;

namespace RimworldTogether.GameClient.Managers
{
    public static class RimworldManager
    {
        public enum Location { Caravan, Settlement }

        public static bool CheckForAnySocialPawn(Location location)
        {
            if (location == Location.Caravan)
            {
                Caravan caravan = ClientValues.chosenCaravan;

                Pawn playerNegotiator = caravan.PawnsListForReading.Find(fetch => fetch.IsColonist && !fetch.skills.skills[10].PermanentlyDisabled);
                if (playerNegotiator != null) return true;
            }

            else if (location == Location.Settlement)
            {
                Map map = Find.AnyPlayerHomeMap;

                Pawn playerNegotiator = map.mapPawns.AllPawns.Find(fetch => fetch.IsColonist && !fetch.skills.skills[10].PermanentlyDisabled);
                if (playerNegotiator != null) return true;
            }

            return false;
        }

        public static bool CheckIfPlayerHasMap()
        {
            Map map = Find.AnyPlayerHomeMap;
            if (map != null) return true;
            else return false;
        }

        public static bool CheckIfPlayerHasCommsConsole()
        {
            Map[] playerMaps = Find.Maps.FindAll(x => x.ParentFaction == RimWorld.Faction.OfPlayer).ToArray();

            foreach(Map map in playerMaps)
            {
                Thing[] mapThings = map.listerThings.AllThings.ToArray();
                foreach(Thing thing in mapThings)
                {
                    if (thing.def.defName == "CommsConsole") return true;
                }
            }

            return false;
        }

        public static bool CheckIfHasEnoughSilverInCaravan(int requiredQuantity)
        {
            if (requiredQuantity == 0) return true;

            List<Thing> caravanSilver = CaravanInventoryUtility.AllInventoryItems(ClientValues.chosenCaravan)
                .FindAll(x => x.def == ThingDefOf.Silver);

            int silverInCaravan = 0;
            foreach (Thing silverStack in caravanSilver) silverInCaravan += silverStack.stackCount;

            if (silverInCaravan > requiredQuantity) return true;
            else return false;
        }

        public static void RemoveThingFromCaravan(ThingDef thingDef, int requiredQuantity)
        {
            if (requiredQuantity == 0) return;

            List<Thing> caravanQuantity = CaravanInventoryUtility.AllInventoryItems(ClientValues.chosenCaravan)
                .FindAll(x => x.def == thingDef);

            int takenQuantity = 0;
            foreach (Thing unit in caravanQuantity)
            {
                if (takenQuantity + unit.stackCount >= requiredQuantity)
                {
                    unit.holdingOwner.Take(unit, requiredQuantity - takenQuantity);
                    break;
                }

                else if (takenQuantity + unit.stackCount < requiredQuantity)
                {
                    unit.holdingOwner.Take(unit, unit.stackCount);
                    takenQuantity += unit.stackCount;
                }
            }
        }

        public static Map[] GetMapsWithCommsConsole()
        {
            Map[] playerMaps = Find.Maps.FindAll(x => x.ParentFaction == RimWorld.Faction.OfPlayer).ToArray();

            List<Map> mapsWithComms = new List<Map>();

            foreach (Map map in playerMaps)
            {
                Thing[] mapThings = map.listerThings.AllThings.ToArray();
                foreach (Thing thing in mapThings)
                {
                    if (thing.def.defName == "CommsConsole") mapsWithComms.Add(map);
                }
            }

            return mapsWithComms.ToArray();
        }

        public static string CompressMapToString(Map map, bool includeItems, bool includeHumans, bool includeAnimals, bool includeMods)
        {
            MapDetailsJSON mapDetailsJSON = DeepScribeManager.TransformMapToString(map, includeItems, includeHumans, includeAnimals);

            if (includeMods) mapDetailsJSON.mapMods = ModManager.GetRunningModList().ToList();

            string toConvert = Serializer.SerializeToString(mapDetailsJSON);
            byte[] bytes = Encoding.UTF8.GetBytes(toConvert);
            return GZip.Compress(bytes);
        }

        public static MapDetailsJSON DeCompressMapDetailsFromString(string deflatedDetails)
        {
            byte[] inflatedBytes = GZip.Decompress(deflatedDetails);
            string inflatedString = Encoding.UTF8.GetString(inflatedBytes);
            return Serializer.SerializeFromString<MapDetailsJSON>(inflatedString);
        }
    }
}
