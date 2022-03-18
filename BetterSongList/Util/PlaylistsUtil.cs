﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetterSongList.Util {
	public static class PlaylistsUtil {
		public static bool hasPlaylistLib = false;
		public static bool requiresListCast = false;

		public static void Init() {
			var x = IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlaylistsLib");

			hasPlaylistLib = x != null;
			requiresListCast = hasPlaylistLib && x.HVersion <= new Hive.Versioning.Version("1.4.0");
		}

		public static Dictionary<string, IBeatmapLevelPack> builtinPacks = null;

		public static IBeatmapLevelPack GetPack(string packName) {
			if(packName == null)
				return null;

			if(packName == "Custom Levels") {
				return SongCore.Loader.CustomLevelsPack;
			} else if(packName == "WIP Levels") {
				return SongCore.Loader.WIPLevelsPack;
			}

			if(builtinPacks == null) {
				builtinPacks =
					SongCore.Loader.BeatmapLevelsModelSO.allLoadedBeatmapLevelWithoutCustomLevelPackCollection.beatmapLevelPacks
					// There shouldnt be any duplicate name basegame playlists... But better be safe
					.GroupBy(x => x.shortPackName)
					.Select(x => x.First())
					.ToDictionary(x => x.shortPackName, x => x);
			}

			if(builtinPacks.ContainsKey(packName)) {
				return builtinPacks[packName];
			} else {
				IBeatmapLevelPack wrapper() {
					foreach(var x in SongCore.Loader.BeatmapLevelsModelSO.customLevelPackCollection.beatmapLevelPacks) {
						if(x.packName == packName)
							return x;
					}
					return null;
				}
				return wrapper();
			}
		}

		public static bool IsCollection(IAnnotatedBeatmapLevelCollection levelCollection) {
			return levelCollection is BeatSaberPlaylistsLib.Legacy.LegacyPlaylist || levelCollection is BeatSaberPlaylistsLib.Blist.BlistPlaylist;
		}

		public static IPreviewBeatmapLevel[] GetLevelsForLevelCollection(IAnnotatedBeatmapLevelCollection levelCollection) {
			if(levelCollection is BeatSaberPlaylistsLib.Legacy.LegacyPlaylist legacyPlaylist)
				return legacyPlaylist.BeatmapLevels;
			if(levelCollection is BeatSaberPlaylistsLib.Blist.BlistPlaylist blistPlaylist)
				return blistPlaylist.BeatmapLevels;
			return null;
		}
	}
}
