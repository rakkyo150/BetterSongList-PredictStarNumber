using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BetterSongList.Util
{
	static class PredictStarNumberUtil
	{
		public static async Task<string> PredictStarNumberApi(string hash, SongDetailsCache.Structs.MapDifficulty mapDifficulty)
		{
			string endpoint = $"https://predictstarnumber.herokuapp.com/api/hash/{hash}";

			HttpClient client = new HttpClient();
			var response = await client.GetAsync(endpoint);
			string jsonString = await response.Content.ReadAsStringAsync();

			dynamic jsonDynamic = JsonConvert.DeserializeObject<dynamic>(jsonString);

			string difficulty="";
			
			if(mapDifficulty == SongDetailsCache.Structs.MapDifficulty.Easy)
			{
				difficulty = "Easy";
			}
			else if(mapDifficulty == SongDetailsCache.Structs.MapDifficulty.Normal)
			{
				difficulty = "Normal";
			}
			else if(mapDifficulty== SongDetailsCache.Structs.MapDifficulty.Hard)
			{
				difficulty = "Hard";
			}
			else if(mapDifficulty== SongDetailsCache.Structs.MapDifficulty.Expert)
			{
				difficulty = "Expert";
			}
			else if(mapDifficulty== SongDetailsCache.Structs.MapDifficulty.ExpertPlus)
			{
				difficulty = "ExpertPlus";
			}

			string rank = JsonConvert.SerializeObject(jsonDynamic[difficulty]);

			return rank;
		}
	}
}
