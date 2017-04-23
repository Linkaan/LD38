using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn {

	public Country attackingCountry;
	public Country attackedCountry;

	public Player player;
	public Player attackedPlayer;
	public bool doMoveGeneral;
	public bool useOnlyHalfArmy;

	public int turnCount;

	public Turn (int turnCount, Player player, Country moveToCountry) {
		this.turnCount = turnCount;
		this.player = player;
		this.doMoveGeneral = true;
		this.attackedCountry = moveToCountry;
	}

	public Turn (int turnCount, Player player, Player attackedPlayer, Country attackingCountry, Country attackedCountry, bool useOnlyHalfArmy) {
		this.turnCount = turnCount;
		this.player = player;
		this.attackedPlayer = attackedPlayer;
		this.attackingCountry = attackingCountry;
		this.attackedCountry = attackedCountry;
		this.useOnlyHalfArmy = useOnlyHalfArmy;
	}

}
