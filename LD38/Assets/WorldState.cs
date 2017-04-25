using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldState {

	public List<Player> playersAlive;
	public Dictionary<Player, List<Country>> countriesOwnedByPlayer;
	public Dictionary<Country, int[]> countriesUnitCountAndProduction;
	public Dictionary<General, Country> generalPositions;

	public bool isTerminalNode;
	public Player whoWon;
	public Player whoLost;

	public Turn turn;

	public Map map;

	// creates new world state from current world
	public WorldState (Map map) {
		this.map = map;
		Player[] allPlayers = map.players;
		playersAlive = new List<Player> ();
		generalPositions = new Dictionary<General, Country> ();
		foreach (Player player in allPlayers) {
			if (player.isAlive) {
				playersAlive.Add (player);
				generalPositions [player.generalComponent] = player.generalHome;
			}
		}

		countriesOwnedByPlayer = new Dictionary<Player, List<Country>> ();
		countriesUnitCountAndProduction = new Dictionary<Country, int[]> ();
		foreach (Country country in map.colour2country.Values) {
			if (country.owner != null) {
				if (!countriesOwnedByPlayer.ContainsKey (country.owner)) {
					countriesOwnedByPlayer [country.owner] = new List<Country> ();
				}
				countriesOwnedByPlayer [country.owner].Add (country);
			}

			int[] unitValues = new int[2];
			unitValues [0] = country.units;
			unitValues [1] = country.unitGenerationFactor;

			countriesUnitCountAndProduction [country] = unitValues;
		}

		this.whoWon = null;
		this.whoLost = null;
	}

	public WorldState (Map map, List<Player> playersAlive, Dictionary<Player,
				       List<Country>> countriesOwnedByPlayer,
		 			   Dictionary<Country, int[]> countriesUnitCountAndProduction,
					   Dictionary<General, Country> generalPositions,
					   bool isTerminalNode, Player whoWon, Player whoLost, Turn turn) {
		this.map = map;
		this.playersAlive = playersAlive;
		this.countriesOwnedByPlayer = countriesOwnedByPlayer;
		this.countriesUnitCountAndProduction = countriesUnitCountAndProduction;
		this.generalPositions = generalPositions;
		this.isTerminalNode = isTerminalNode;
		this.whoWon = whoWon;
		this.whoLost = whoLost;
		this.turn = turn;
	}

	public WorldState getWorldStateAfterTurn (Turn turn) {
		bool isTerminalNode = false;
		Player whoWon = null;
		Player whoLost = null;

		List<Player> playersAlive = new List<Player> (this.playersAlive);
		Dictionary<Player, List<Country>> countriesOwnedByPlayer;

		countriesOwnedByPlayer = new Dictionary<Player, List<Country>> ();
		foreach (Player player in this.countriesOwnedByPlayer.Keys) {
			countriesOwnedByPlayer [player] = new List<Country> (this.countriesOwnedByPlayer [player]);
		}

		Dictionary<Country, int[]> countriesUnitCountAndProduction;

		countriesUnitCountAndProduction = new Dictionary<Country, int[]> ();
		foreach (Country country in this.countriesUnitCountAndProduction.Keys) {
			int[] newUnitValues = new int[2];
			newUnitValues [0] = this.countriesUnitCountAndProduction [country] [0];
			newUnitValues [1] = this.countriesUnitCountAndProduction [country] [1];
			countriesUnitCountAndProduction [country] = newUnitValues;
		}

		Dictionary<General, Country> generalPositions = new Dictionary<General, Country> (this.generalPositions);

		Player currentOwner = getPlayerThatOwnsCountry (turn.attackedCountry);
		if (currentOwner != null && generalPositions [currentOwner.generalComponent] == turn.attackedCountry) {
			countriesUnitCountAndProduction [turn.attackedCountry] [1] = 2;
		} else {
			countriesUnitCountAndProduction [turn.attackedCountry] [1] = 1;
		}

		if (turn.turnCount % map.turnCounter.unitSpawnRate == 0) {
			foreach (Country country in countriesUnitCountAndProduction.Keys) {
				countriesUnitCountAndProduction [country] [0] += countriesUnitCountAndProduction [country] [1];
			}
		}

		if (turn.doMoveGeneral) {
			generalPositions [turn.player.generalComponent] = turn.attackedCountry;
		} else {
			int[] outcome;
			bool doMove = false;

			if (countriesOwnedByPlayer [turn.player].Contains (turn.attackedCountry)) {
				doMove = true;
			}
			bool isNeutralCountry;
			int remainder;

			isNeutralCountry = !countriesUnitCountAndProduction.ContainsKey (turn.attackedCountry);

			outcome = simulateWar (countriesUnitCountAndProduction [turn.attackingCountry] [0],
				isNeutralCountry ? 0 : countriesUnitCountAndProduction [turn.attackedCountry] [0],
				doMove, turn.useOnlyHalfArmy, out remainder);

			if (isNeutralCountry) {
				int[] newUnitValues = new int[2];
				newUnitValues [0] = outcome [0];
				newUnitValues [1] = 1;
				countriesUnitCountAndProduction [turn.attackedCountry] = newUnitValues;
				countriesOwnedByPlayer [turn.player].Add (turn.attackedCountry);
			} else {
				countriesUnitCountAndProduction [turn.attackingCountry] [0] = outcome [0];
				if (outcome [1] <= 0) {
					if (turn.attackedPlayer != null)
						countriesOwnedByPlayer [turn.attackedPlayer].Remove (turn.attackedCountry);
					countriesOwnedByPlayer [turn.player].Add (turn.attackedCountry);
					countriesUnitCountAndProduction [turn.attackedCountry] [0] = remainder;
					//Debug.Log (turn.player.name + " might take over " + turn.attackedCountry);

					if (turn.attackedPlayer != null && turn.attackedCountry == generalPositions [turn.attackedPlayer.generalComponent]) {
						//Debug.Log ("someone might die!");
						// player died
						generalPositions.Remove (turn.attackedPlayer.generalComponent);
						playersAlive.Remove (turn.attackedPlayer);
						countriesOwnedByPlayer [turn.player].AddRange (countriesOwnedByPlayer [turn.attackedPlayer]);
						countriesOwnedByPlayer.Remove (turn.attackedPlayer);

						isTerminalNode = true;
						whoWon = turn.player;
						whoLost = turn.attackedPlayer;
					}
				} else {
					countriesUnitCountAndProduction [turn.attackedCountry] [0] = outcome [1];
				}
			}
		}

		return new WorldState (map, playersAlive, countriesOwnedByPlayer, countriesUnitCountAndProduction, generalPositions, isTerminalNode, whoWon, whoLost, turn);
	}

	int[] simulateWar (int A, int B, bool doMove, bool useOnlyHalfArmy, out int remainder) {
		int[] outcome = new int[2];

		remainder = 0;
		if (A > 2) {
			int unitsToAttackWith = useOnlyHalfArmy ? Mathf.CeilToInt (A / 2) : A - 1;
			A -= unitsToAttackWith;
			if (doMove) {
				B += unitsToAttackWith;		
			} else {
				remainder = unitsToAttackWith - B;
				B -= unitsToAttackWith;
			}
		}
		outcome [0] = A;
		outcome [1] = B;
		return outcome;
	}

	public int evaluateWorldState (Player forPlayer, Player againstPlayer) {
		int heuristic = 0;
		// take into account amount of units + countries
		// the fewer players the better evaluation for the current player
		// if captured enemy general this evaluation of the world state should be very high
		// if players general was captured this evaluation should be as low as possible

		if (!playersAlive.Contains (forPlayer))
			return int.MinValue;
		else if (againstPlayer != null && !playersAlive.Contains (againstPlayer)) {
			return 10000;
		}

		if (againstPlayer == null) {
			foreach (Country country in countriesOwnedByPlayer[forPlayer]) {
				heuristic += countriesUnitCountAndProduction [country] [0];
			}
			heuristic += countriesOwnedByPlayer [forPlayer].Count * 10;
			heuristic -= playersAlive.Count * 10;
		}

		foreach (Player p in playersAlive) {
			if (p == forPlayer)
				continue;
			foreach (Country country in countriesOwnedByPlayer[p]) {
				if (country.neighbours.Contains (generalPositions [forPlayer.generalComponent])) {
					heuristic -= 500;
				}
			}
		}

		if (isTerminalNode) {
			if (whoWon == forPlayer && (againstPlayer == null || whoLost == againstPlayer)) {
				heuristic += 10000;
			} else {
				heuristic = int.MinValue;
			}
		}

		return heuristic;
	}

	public List<WorldState> getPossibleWorldStates (Player player, int turnCount) {
		List<Turn> possibleTurns = getPossibleTurns (player, turnCount);

		List<WorldState> possibleWorldStates = new List<WorldState> ();
		foreach (Turn turn in possibleTurns) {
			possibleWorldStates.Add (this.getWorldStateAfterTurn (turn));
		}

		return possibleWorldStates;
	}

	List<Turn> getPossibleTurns (Player player, int turnCount) {
		List<Turn> possibleTurns = new List<Turn> ();

		if (!playersAlive.Contains (player))
			return possibleTurns;

		foreach (Country country in countriesOwnedByPlayer[player]) {
			foreach (Country neighbour in country.neighbours) {
				if (countriesOwnedByPlayer [player].Contains (neighbour) &&
					generalPositions[player.generalComponent].neighbours.Contains(neighbour)) {
					Turn moveGeneralTurn = new Turn (turnCount, player, neighbour);
					moveGeneralTurn.doMoveGeneral = true;
					possibleTurns.Add (moveGeneralTurn);

					if (countriesUnitCountAndProduction [country] [0] > 1) {
						possibleTurns.Add (new Turn (turnCount, player, player, country, neighbour, true));
						possibleTurns.Add (new Turn (turnCount, player, player, country, neighbour, false));
					}
				} else {
					if (countriesUnitCountAndProduction [country] [0] > 1) {
						Player otherPlayer = getPlayerThatOwnsCountry (neighbour);
						possibleTurns.Add (new Turn (turnCount, player, otherPlayer, country, neighbour, true));
						possibleTurns.Add (new Turn (turnCount, player, otherPlayer, country, neighbour, false));
					}
				}
			}
		}
		return possibleTurns;
	}

	Player getPlayerThatOwnsCountry (Country country) {
		foreach (Player player in playersAlive) {
			if (countriesOwnedByPlayer.ContainsKey (player) && countriesOwnedByPlayer [player].Contains (country))
				return player;
		}
		return null;
	}

}
