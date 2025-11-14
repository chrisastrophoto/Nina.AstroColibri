# AstroColibri

## 1.3.0.1

- New Features:
	- It is now possible to remove all or selected events from the dockable in the image tab by clicking the appropriate buttons.
	
	- The "Check for Transients"" button can now retrieve events of the last at max. 24 hours. There is a new setting to modify this period: "Manually check for new transients in previous xxxx minutes".
	
	- A button "Check for Transients" is added to the dockable in the imaging tab.

## 1.2.0.1

- New Features:
	- Events are shown in the dockable only, if they are above horizon and above minimum altitude after start of nautical dawn in the evening and before nautical down in the morning or if they are currently visible. 
	  The test cases behave as before. As a result the test case "Visble Target" may be shown or not, depending on the time when you execute the test run.
	
	- The visibility ("Visble now" or "Visible at night") is shown as a new line on the dockable in the image tab.

	- If the new switch "Save automatically" is set to ON, the sequence will be saved automatically, if the AstroColibri instruction is used and after the DSO template has been inserted and the target of the DSO template has been updated. 
	  In that case the original sequence is overwritten without notice! 
	  If no AstroColibri instruction is used or the DSO template is not found, the sequence will NOT be saved automatically.

## 1.1.0.1
- Fixes:
	- Correct calculation of "Never visible"

- New Features:
	- Include custom horizon in visibility calculation

	- New AstroColibri Loop Condition  
	  The condition triggers, if a visible event has been detected by the trigger. 
	  The sequence continues with whatever is defined after the loop.

	- New AstroColibri Instruction  
	  If this instruction is placed after a loop containing an AstroColibri condition and an AstroColibri trigger the DSO sequnce Template defined in the instruction is inserted into the sequence after this instruction, the sequence is restarted and continues with this DSO sequence template.

	- New Plugin Parameters  
		- Default DSO Template  
		This is the default DSO Sequnece Template which is loaded into the AstroColibri instruction when the sequence is loaded.
		- Test switch  
		When this switch is active, each exposure triggers a simulated request to Astro-Colibri, which returns a visible, an invisible and a never visible target and the AstroColibri Condtion triggers immediately.  
		Note that in test mode the links diplayed in the dockable in the image tab do all point to the [Astro-Colibri Web Interface](https://astro-colibri.com/). These are links to some arbitrary but real events from the past.
	
		
		
## 1.0.0.2
- Fixed Links in AssemblyInfo

## 1.0.0.1
- Initial release