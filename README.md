# AstroColibri NINA Plugin

Astrocolibri is a plugin for N.I.N.A.

The plugin makes use of the Astro-COLIBRI platform [P. Reichherzer et al., 2021 ApJS 256 5: [ApJS](https://iopscience.iop.org/article/10.3847/1538-4365/ac1517), [ADS](https://ui.adsabs.harvard.edu/abs/2021ApJS..256....5R/abstract)].

[Astro-COLIBRI](https://astro-colibri.science/) is a platform for multi-messenger astrophysics.
It aggregates events from a large number of astronomical and astrophysical observatories around the world and makes them available to the public.

Astro-Colibri is available as [Web interface](https://astro-colibri.com/) and as app in [iOS-Appstore](https://apps.apple.com/us/app/astro-colibri/id1576668763) or [Google Play](https://play.google.com/store/apps/details?id=science.astro.colibri).

This plugin queries the public [API](https://astro-colibri.science/apidoc) of Astro-Colibri to retrieve the latest events provided by Astro-COLIBRI.
Currently it provides visibility charts for the events at the observtion location and allows to send the event location with a single click to the framing assistant.

Thus it provides means to take images from transient events in almost real time.
Note however, that there is always a delay between the actual event in the sky and the submission of the corresponding Astro-COLIBRI event.

## Requirements and usage

- The use of the plugin requires internet access during your imaging session.
- In order to use the API an user ID from Astro-COLIBRI required, which can be retrieved [here](https://astro-colibri.com/) by clicking on "Personalize" and creating an account. The user ID allows for calling the API 100 times per day. So if you want to run this plugin in more than one NINA sessions, you might need multiple accounts.
- Define in your Astro-Colibri account, for which type of events you want to receive notifications. This also determines the events you will receive via this plugin.
- Optionally define in your Astro-COLIBRI account your observation location as "Observatory". This will restricts to events which are visible at your observation location. (Note that they use the term "zenith limit" instead of "Altitude" to restrict events to location high enough in the sky.) If you do not specify an observatory here, the plugin will filter according to your observation location settings in NINA.
- Go to the plugin page and fill all required settings.
    + User ID from your Astro-COLIBRI account
    + The time in minutes the plugin waits until it performs a check for new events. Default and minimum value is 10, which is usually sufficient to not reach the request limit per user id and day.
	+ The path to the location where JSON files for the events are saved. Default path is %localappdata%\AstroColibri\Events\JSON\ and will be created on the first received data.
	+ The number of days to keep theses data files.
	+ A minimum altitude in degrees, which will provide an additional filter for evaluating an event as "visible".
- Add an "Astrocolibri Trigger" to your imaging sequence.
- Run the sequence ...
- The sequence will check for new event after each image recorded, but not more frequenly than specified by the according paramter above.

Once events are recorded, data and visibilty charts for these events are shown in a dockable in the "Image" tab named "Altitude Charts for Astro-COLIBRI Events".
Each event has a button on the left side next to the event source name, which allows to send the coordinates of the event source to the framing assistant.
New subsequent events are inserted on the top of the "Altitude Charts for Astro-COLIBRI Events".
The list of events is cleared, when you quit NINA.

Have fun and be one of the first to image T CrB ... or some new supernovae with help of this plugin. 

Christoph Nieswand, May 2025





