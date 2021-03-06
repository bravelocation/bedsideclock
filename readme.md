# Code for Bedside Clock Windows Phone App #

This repository holds the code for [Bedside Clock Windows Phone App](http://www.bravelocation.com/bedsideclock).

This was originally a project just for me to learn about developing for Windows Phone, and to replace the better built-in clock I was missing from my Android phone. You can probably tell this with some of the slightly gratuitous features I added (phase of the moon calculations anyone?)

It's done reasonably well at the [App Store](http://www.windowsphone.com/en-gb/store/app/bedside-clock/2670018c-c233-e011-854c-00237de2db9e) with around 13,000 downloads as of February 2013. However, I'm getting rid of my Windows Phone so I thought I'd donate the code to the community in case anyone was interested in developing further.

## The Code ##

Hopefully the code is pretty self-explanatory, and should be able to be built out of the box using Visual Studio 2012 with the appropriate Windows Phone 8 SDKs installed.

There are some [xUnit.net](http://xunit.codeplex.com/) unit tests I added, so obviously to run those you'll need to install the appropriate test runner executables.

## Configuration ##

Somewhat painfully, it's not very easy to hold config values in a Windows Phone app (why no support for app.config?), so there is one setting hard-coded into the code that should bechanged if you adapt the code.

The value is the yahooApiKey used in YahooLocationServices.cs that does the address lookup. If reusing this code, please use your own API key from [http://developer.apps.yahoo.com/](http://developer.apps.yahoo.com/). Thanks!

## Screenshots and icons ##

The icons and images used on the app store are all in the /Screenshots directory. As you can see, I am definitely no artist, and the app icon is particularly ugly. Anyone who would like to improve this, feel free!

## Credits #

- Weather forecasts and locations are courtesy of [Yahoo! Developer Network](http://developer.yahoo.com/)
- The nice Weather icons courtesy of [Gavin Elliott](http://www.gavinelliott.co.uk/2010/04/free-weather-icons/).


