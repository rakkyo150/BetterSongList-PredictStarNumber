﻿using BetterSongList.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.SortModels {
	public sealed class FolderDateSorter : ISorterWithLegend, ISorterPrimitive {
		public bool isReady => wipTask == null && songTimes != null;

		/*
		 * TODO: For now, I need to use LevelId : int because I have to cast Playlists in LevelCollectionTableSet
		 * once that is gone (Fixed BS Playlist Lib) I can go back to IPreviewBeatmapLevel : int
		 */
		static ConcurrentDictionary<string, int> songTimes = null;

		static TaskCompletionSource<bool> wipTask = null;
		static bool isLoading = false;
		public Task Prepare(CancellationToken cancelToken) => Prepare(false);
		Task Prepare(bool fullReload) {
			if(songTimes == null) {
				songTimes = new ConcurrentDictionary<string, int>();
				SongCore.Loader.SongsLoadedEvent += (_, _2) => Prepare(false);
			}

			wipTask ??= new TaskCompletionSource<bool>();

			if(!SongCore.Loader.AreSongsLoaded || SongCore.Loader.AreSongsLoading)
				return wipTask.Task;

			if(!isLoading) {
				isLoading = true;
				Task.Run(() => {
					var xy = new System.Diagnostics.Stopwatch();
					xy.Start();

					foreach(var song in 
						SongCore.Loader.BeatmapLevelsModelSO
						.allLoadedBeatmapLevelPackCollection.beatmapLevelPacks.Where(x => x is SongCore.OverrideClasses.SongCoreCustomBeatmapLevelPack)
						.SelectMany(x => x.beatmapLevelCollection.beatmapLevels)
						.Cast<CustomPreviewBeatmapLevel>()
					) {
						if(songTimes.ContainsKey(song.levelID) && !fullReload)
							continue;

						/*
						 * There isnt really any "good" setup - LastWriteTime is cloned when copying a file and retained when manually
						 * extracing from a zip, but the createtime is obviously "reset" when you copy files
						 */
						songTimes[song.levelID] = (int)File.GetCreationTimeUtc(song.customLevelPath + Path.DirectorySeparatorChar + "info.dat").ToUnixTime();
					}

					Plugin.Log.Debug(string.Format("Getting SongFolder dates took {0}ms", xy.ElapsedMilliseconds));
					wipTask.TrySetResult(true);
					wipTask = null;
					isLoading = false;
				});
			}

			return wipTask.Task;
		}

		public float? GetValueFor(IPreviewBeatmapLevel level) {
			if(songTimes.TryGetValue(level.levelID, out var oVal))
				return oVal;

			return null;
		}

		const float MONTH_SECS = 1f / (60 * 60 * 24 * 30.4f);
		public IEnumerable<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
			var curUtc = (int)DateTime.UtcNow.ToUnixTime();

			return SongListLegendBuilder.BuildFor(levels, (level) => {
				if(!songTimes.ContainsKey(level.levelID))
					return null;

				var months = (curUtc - songTimes[level.levelID]) * MONTH_SECS;

				if(months < 1)
					return "<1 M";

				return Math.Round(months) + " M";
			});
		}
	}
}
