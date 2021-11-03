This command line application converts a Mass Effect 3 (Legendary Edition; Original is untested) Headmorph file exported from the save editor in the .ron format into a fomrat that can be used by the Appearance Modification Menu. 

Simply run the program on the command line with the following arguments:
Path to input ron file
Path to output file (optional, defaults to input with extension changed to txt)
gender in the format of --gender=Male or --gender=Female


It will run and output the converted text file.
This program may require that you install .Net 5.

To add your converted headmorph into the Appearance Modification Menu, add the following text to your BioUI file in your DLC mod:
<Section name="sfxgamecontent.sfxguidata_appearance_presets">
	<Property name="appearanceitemarray">
		<!-- your text here -->
	</Property>
</Section>


Be sure to update the string reference that appears on the first line to a short description of your headmorph, which willl appear in the store.


This program makes use of code from the ME3Tweaks Mod Manager repository for parsing ron files, and I am grateful that I can build on their work.
