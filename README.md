# Sharp Visualiser
Real time music visualiser written in C#

![visual](/images/visual.gif?raw=true)

Uses DSP algorithms such as the Fourier Transform to generate visualisations of the 
music data in real time.

Currently only supports 16bit Signed Big Endian PCM WAV formats, and a single
visualisation which displays the noise levels in decibels against frequencies.


# Building/Running
## Windows (Visual Studio 2012+)
- Open the solution and press the `Start` button in the menu toolbar. (You may have to right click the `SharpVisualiser.UI` Project and select the `Set as StartUp Project` option first.) 

# Linux And OSX (MonoDevelop 4.0+)
- Open the solution and right click the `SharpVisualiser.UI` project and select `Run Item`


#TODO
- Migrate Unit Tests to NUnit from Visual Studio Test so they can be run on OSX and Linux
- Fix timing issues so that the visualiser automatically adjusts to playing
  in real time based on the settings of the media. Currently plays a little
  bit slow.
- Add support for more formats/codecs (Other WAV formats, MP3, OGG)
- Add more Visualisations (Waves, different colors, patterns, etc.)
- Add customisation options 
- Proper UI controls for Start/Stop/Play instead of menu options, as well
  as an indicator of current time.
- Possibly add the ability to play the actual media with the
  visualisation.

