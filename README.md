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
Once events are recorded, data and visibilty charts for these events are shown in a dockable in the "Image" tab named "Altitude Charts for Astro-COLIBRI Events" as long as the target is currently visible or might become visible after start of nautical dawn in the evening and before nautical down in the morning. 
Each event has a button on the left side next to the event source name, which allows to send the coordinates of the event source to the framing assistant.
New subsequent events are inserted on the top of the "Altitude Charts for Astro-COLIBRI Events".
The list of events is cleared, when you quit NINA.

- Optionlly add an "AstroColibri Condition" into an existing loop. It will trigger as soon as the "AstroColibri Trigger" has detected a visible Event. The user is free to define whatever should happen, after leaving the loop.  
IMPORTANT: Add an additional Loop Condition, otherwise the instructions in the loop will eventually repeated forever ... until an event is received.
- Optionally create a DSO Sequence Template which you want to be executed as soon as a visible event is received.  
IMPORTANT: Do not forget to insert a "Slew & Center" instruction into this template ... otherwise you will not be happy with the result ... 
- Add an "AstroColibri Instruction" directly after a loop containing a "AstroColibri Trigger" and an "AstroColibri Condition". Specify the DSO Sequence Template in this instruction, which you have created above.  
On loading of the sequence the "Default DSO Template" specified in the plugin options will be inserted. You are free to overwrite the template to be loaded here.  
Once the "AstroColibri Condition" triggers, the DSO Sequence Template is appended to the current sequence after the "AstroColibri Instruction", the target is inserted into this sequence template and finally the sequence is stopped and re-launched, so that it continues with the inserted DSO Sequence Template. 
If the switch "Save automatically" is set to ON, the sequence will be saved automatically after the DSO template has been inserted and the target of the DSO template has been updated. 
In that case the original sequence is overwritten without notice! 
If no AstroColibri instruction is used or the DSO template is not found, the sequence will NOT be saved automatically.

- We have added a "Test" switch, which simulates events detected on each exposure.  
In test mode, no internet access is required. Three events are simulated: a visible event, an invisible event and a never visible event. You can use this feature to test your sequence. The links in the dockable for the displayed events are all pointing to the [Astro-Colibri Web Interface](https://astro-colibri.com/)


Have fun and be one of the first to image T CrB ... or some new supernovae with help of this plugin. 

Christoph Nieswand, June 2025





