# AstroColibri

## 1.1.0.1
- Fixes:
	- Correct calculation of "Never visible"

- New Features:
	- New AstroColibri Loop Condition  
	  The condition triggers, if a visible event has been detected by the trigger. 
	  The sequence continues with whatever is defined after the loop.

	- New AstroColibri Instruction  
	  If this instruction is placed after a loop containing an AstroColibri condition and an AstroColibri trigger the DSO sequnce Template defined in the instruction is inserted into the sequence after this instruction, the sequence is restarted and continues with this DSO sequence template.

	- New Plugin Parameters  
		- Default DSO Template  
		This is the default DSO Sequnece Template which is loaded into the AstroColibri instruction when the sequence is loaded.
		- A Test switch  
		When this switch is active, each exposure triggers a simulated request to Astro-Colibri, which returns a visible, an invisible and a never visible target and the AstroColibri Condtion triggers immediately.  
		  Note that in test mode the links diplayed in the dockable in the image tab do not correspond to the simulated response. These are links to some arbitrary but real events from the past.
	
		
		
## 1.0.0.2
- Fixed Links in AssemblyInfo

## 1.0.0.1
- Initial release