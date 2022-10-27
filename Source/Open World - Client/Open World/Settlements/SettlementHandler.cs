﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace OpenWorld
{
    public static class SettlementHandler
    {
		public static void AddSettlementInWorld(string data)
		{
			int tileID = int.Parse(data.Split('│')[2]);
			string name = data.Split('│')[3] + "'s Settlement";

			Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
			settlement.SetFaction(Main._ParametersCache.onlineNeutralFaction);
			settlement.Tile = tileID;
			settlement.Name = name;
			Find.WorldObjects.Add(settlement);

			Main._ParametersCache.onlineNeutralSettlements.Add(tileID, new List<string>() { name });

			Main._ParametersCache.allSettlements.Add(tileID, new List<string>() { name });
		}

		public static void RemoveSettlementInWorld(string data)
		{
			if (string.IsNullOrWhiteSpace(data)) return;

			int settlementTile = int.Parse(data.Split('│')[2]);

			List<Settlement> settlementList = Find.WorldObjects.Settlements;

			Settlement settlementToDestroy = settlementList.Find(item => item.Tile == settlementTile);

			if (settlementToDestroy == null) return;
			else
            {
				if (settlementToDestroy.Faction == Main._ParametersCache.onlineNeutralFaction) 
					Main._ParametersCache.onlineNeutralSettlements.Remove(settlementTile);

				else if (settlementToDestroy.Faction == Main._ParametersCache.onlineAllyFaction) 
					Main._ParametersCache.onlineAllySettlements.Remove(settlementTile);

				else if (settlementToDestroy.Faction == Main._ParametersCache.onlineEnemyFaction) 
					Main._ParametersCache.onlineEnemySettlements.Remove(settlementTile);

				Main._ParametersCache.allSettlements.Remove(settlementTile);

				Find.WorldObjects.Remove(settlementToDestroy);
			}
		}

		public static void GetSettlementsFromServer(string data)
		{
			data = data.Remove(0, 12);

			Main._ParametersCache.allSettlements.Clear();
			Main._ParametersCache.onlineNeutralSettlements.Clear();
			Main._ParametersCache.onlineAllySettlements.Clear();
			Main._ParametersCache.onlineEnemySettlements.Clear();

			string[] settlementsToLoad = data.Split('│');

			foreach (string str in settlementsToLoad)
			{
				if (string.IsNullOrWhiteSpace(str)) continue;

				int settlementTile = int.Parse(str.Split(':')[0]);
				string settlementName = str.Split(':')[1];
				int settlementFactionValue = int.Parse(str.Split(':')[2]);

				List<string> settlementDetails = new List<string>()
				{
					settlementName,
					settlementFactionValue.ToString()
				};

				if (settlementFactionValue == 0) Main._ParametersCache.onlineNeutralSettlements.Add(settlementTile, settlementDetails);
				else if (settlementFactionValue == 1) Main._ParametersCache.onlineAllySettlements.Add(settlementTile, settlementDetails);
				else if (settlementFactionValue == 2) Main._ParametersCache.onlineEnemySettlements.Add(settlementTile, settlementDetails);
			}

			Main._ParametersCache.allSettlements.AddRange(Main._ParametersCache.onlineNeutralSettlements);
			Main._ParametersCache.allSettlements.AddRange(Main._ParametersCache.onlineAllySettlements);
			Main._ParametersCache.allSettlements.AddRange(Main._ParametersCache.onlineEnemySettlements);
		}

		public static void ManageSettlementsInWorld()
		{
			List<Settlement> existingSettlements = new List<Settlement>();
			List<Settlement> serverSettlements = new List<Settlement>();

			//Get existing settlements
			foreach (Settlement st in Find.WorldObjects.Settlements)
			{
				if (st.Faction == Main._ParametersCache.onlineNeutralFaction ||
					st.Faction == Main._ParametersCache.onlineAllyFaction ||
					st.Faction == Main._ParametersCache.onlineEnemyFaction)
				{
					existingSettlements.Add(st);
				}
			}

			//Get server settlements
			foreach (KeyValuePair<int, List<string>> pair in Main._ParametersCache.onlineNeutralSettlements)
			{
				Settlement neutralSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
				neutralSettlement.Name = pair.Value[0];
				neutralSettlement.Tile = pair.Key;
				neutralSettlement.SetFaction(Main._ParametersCache.onlineNeutralFaction);

				serverSettlements.Add(neutralSettlement);
			}

			foreach (KeyValuePair<int, List<string>> pair in Main._ParametersCache.onlineAllySettlements)
			{
				Settlement allySettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
				allySettlement.Name = pair.Value[0];
				allySettlement.Tile = pair.Key;
				allySettlement.SetFaction(Main._ParametersCache.onlineAllyFaction);

				serverSettlements.Add(allySettlement);
			}

			foreach (KeyValuePair<int, List<string>> pair in Main._ParametersCache.onlineEnemySettlements)
			{
				Settlement enemySettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
				enemySettlement.Name = pair.Value[0];
				enemySettlement.Tile = pair.Key;
				enemySettlement.SetFaction(Main._ParametersCache.onlineEnemyFaction);

				serverSettlements.Add(enemySettlement);
			}

			foreach (Settlement settlement in existingSettlements)
			{
				Settlement settlementToRemove = Find.WorldObjects.Settlements.Find(fetch => fetch.Tile == settlement.Tile);
				Find.WorldObjects.Remove(settlementToRemove);
			}

			foreach (Settlement settlement in serverSettlements)
			{
				Find.WorldObjects.Add(settlement);
			}
		}
	}
}
