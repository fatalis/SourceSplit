SourceSplit
===========

SourceSplit is a [LiveSplit] component for Source engine games. It aims to support every game and engine version, but some features are only available on games where support has been added.

Features
--------
  * Keeps track of Game Time to get rid of loading times. Emulates demo timing perfectly.
  * Splits when the map changes. Configurable with map whitelists.
  * Auto start/stop/reset the timer. (supported games only)

Install
-------
Starting with LiveSplit 1.4, you can download and install SourceSplit automatically from within the Splits Editor with just one click. Just type in the name of one of the fully supported games (see below) and click Activate. This downloads SourceSplit to the Components folder.

Configure
---------
Due to bugs in LiveSplit 1.4.0, it's recommended to deactivate SourceSplit in the Splits Editor after activating (downloading) it the first time. Instead, you should activate it in the Layout Editor, under the "Control" category.

Double-click "SourceSplit" after adding it to your layout to bring up the settings.

#### Auto Split
The default settings are fine unless you feel it's splitting too often. In that case you can use the whitelist and add maps you think it should split on completion. Note to those unfamiliar with LiveSplit: you must add a split in the Splits Editor for every map it's going to auto-split on.

#### Game Processes
You don't need to mess with this unless your game's process isn't listed. All of the fully supported games (and a few others) are added by default.

#### Alternate Timing Method Time
This shows makes it show Real Time when comparing against Game Time, and vice versa. Doesn't work when SourceSplit is activated in the Splits Editor.

Fully Supported Games
---------------------
  * Half-Life 2
  * Half-Life 2: Episode One
  * Half-Life 2: Episode Two
  * Portal
  * Portal 2
  * Aperture Tag
  * (more soon)

Technical Information
---------------------
#### How It Works
Reading game memory and [signature scanning]. This method is used in order to try to support every game and engine version, including ones I've never tested at all. The code would be a lot simpler if it were targetting just one game. A ton of reverse engineering was required to do this. Tools used: IDA Pro, OllyDbg, Source SDK.

#### Timing Accuracy
Note: 1 tick is 15 or 16.6666 milliseconds. You can think of it as around the same as one frame at 60FPS. The timing emulates what the engine does when recording a demo.

Many hours were put into making sure the timer is as accurate as possible. For games with auto-start/end support, timing has tick-perfect accuracy >99% of the time. Otherwise timing accuracy depends on the user (start, end), just like RTA.

There are some situations where the timing can be off by a few ticks:

  * If your frame rate is lower than the tick rate, a few ticks can be lost on auto-start/end. 
  * Due to the nature of how SourceSplit reads game memory on a time interval, a few ticks can be lost if it misses the interval. This should almost never happen, unless you have a truly ancient computer.

However, these cases are rare. While I was making sure the timing is accurate, I actually discovered bugs (much more severe relative to these rare cases) in several other timers and demo tools. If you notice that the timing is different from another timer, it's most likely because the other one isn't as accurate as SourceSplit.

tl;dr I'm confident with the timing and you can and should use it for ~official~ purposes.

Contact
-------
  * [@fatalis_](https://twitter.com/fatalis_)
  * [fatalis.twitch@gmail.com](mailto:fatalis.twitch@gmail.com)
  * [twitch.tv/fatalis_](http://www.twitch.tv/fatalis_)

[PayPal Donate](http://fatalis.hive.ai/donate)

[LiveSplit]:http://livesplit.org/
[signature scanning]:https://wiki.alliedmods.net/Signature_scanning
