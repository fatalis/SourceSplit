SourceSplit is a LiveSplit (livesplit.org) component that helps out Source
engine speedrunning. It can automatically split for you when a map changes, and
it keeps track of in-game time so you don't have to record demos. It works by
reading the game's memory, and it should work on every Source engine game.

Install:

Extract LiveSplit.SourceSplit.dll to your LiveSplit\Components folder. Restart
LiveSplit. Version 1.3 or higher is required.

Configure:

Add SourceSplit in LiveSplit's Layout Editor. Double click 'SourceSplit' to go
to the settings. Enable AutoSplit if you want, it's disabled by default. Add
your game's executable if it isn't present in the Game Processes list. Keep in
mind a lot of source games use hl2.exe and you probably won't have to add
anything. Use the Whitelist if you only want to auto-split on maps you choose.

Map Times List:

You can view the individual level times by right-clicking LiveSplit and going
to "Control->SourceSplit: Map Times". This window will automatically update
when you finish a map, and you can leave it open.

FAQ:

How accurate is the timer?

Only the first and last maps will be inaccurate, because you have to split for
those manually. Every map in between should be 100% accurate, down to the tick.
SourceSplit doesn't do start and end splits because there's no way to do that
accurately for every Source engine game. In a future version I might add
start/end support for some of the popular games.

Can I get VAC banned for using this?

No. If there was even the slightest chance of that happening, I wouldn't have
made SourceSplit. It's known that VAC only bans if you write memory. SourceSplit
only reads it. As an extra precaution in case VAC changes how it works in the
future, SourceSplit refuses to touch VAC protected games, even if you tell it to.
See: http://forums.steampowered.com/forums/showthread.php?t=2465755

It isn't working.

Contact me if it isn't working and I'll figure it out. You'll probably just need
to send me the game's engine.dll. Remember, SourceSplit doesn't support VAC
protected games as stated above.

Will you make something like this for a different game/engine?

My goal is to make removing load times manually no longer a thing for every
popular PC speedgame. I don't know which game I'm going to do next, so give me
some ideas.

Is it open source?

https://github.com/fatalis/sourcesplit

Changelog:

1.2.1
Hotfix for autosplit breakage introduced in 1.2

1.2
Fixed HL2 Steampipe May 29 2014 update
Added an option to disable Game Time
Now obeys drop shadow settings

1.1
Fixed Portal 2 support not working.
Hopefully fixed auto-split not working all of the time.
Show total time in the map times list when run is ended.
Fixed several small bugs.

1.0
First release. Don't consider the timer to be accurate until it's been tested
thoroughly by the community. There are going to be bugs.


Testers: xcd, rewrite, pallidus, S, Colfra
Source engine help: DeathByNukes

@fatalis_
twitch.tv/fatalis_
Fatalis @ irc2.speedrunslive.com IRC
fatalis.twitch@gmail.com
