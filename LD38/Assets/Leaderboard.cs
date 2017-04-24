using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour {

	public Image[] playerColours;
	public Text[] playerUnits;
	public Text[] playerCountries;

	public void UpdateLeaderboard (Map map) {
		Dictionary<Player, int> playerUnitCount = new Dictionary<Player, int> ();
		Dictionary<Player, int> playerCountryCount = new Dictionary<Player, int> ();

		Dictionary<Player, int> playerLeader = new Dictionary<Player, int> ();

		foreach (Player player in GameObject.FindObjectsOfType<Player> ()) {
			playerCountryCount [player] = 0;
			playerUnitCount [player] = 0;
			playerLeader [player] = 0;
			foreach (Country country in map.colour2country.Values) {
				if (country.owner == player) {
					playerCountryCount [player] += 1;
					playerUnitCount [player] += country.units;
					playerLeader [player] = playerCountryCount [player] * 10 + playerUnitCount [player];
				}
			}
		}

		var leaders = playerLeader.ToList();

		leaders.Sort((pair1,pair2) => pair1.Value.CompareTo(pair2.Value));
		leaders.Reverse ();

		int index = 0;
		foreach (KeyValuePair<Player, int> pair in leaders) {
			playerColours[index].color = pair.Key.playerColour;
			playerUnits [index].text = playerUnitCount [pair.Key].ToString ();
			playerCountries [index].text = playerCountryCount [pair.Key].ToString ();
			if (++index > playerColours.Length)
				break;
		}
	}
}
