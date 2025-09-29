## PDF Eater

A PDF reader program specifically designed for use for annotating with a stylus.

### Features 

Current supports the following:

- Stylus drawing
- Stylus erasing
- Mouse drawing
- Touch screen scroll
- Ruling straight lines
- Ruling 'wavy' lines
- Thickness customisation
- Colors!! (user defined colors too!)

### Usage notes

Currently the ENTIRE PDF is rendered when you load it, this is in a background thread, so you can still interact with the PDF while it's rendering, 
but if you open a 200 page PDF, you won't be able to edit page 200 until the progress bar in the top left indicates it's finished.

Designed to be fairly 'foolproof' so it'll prompt you if you leave without saving, also shows warning boxes for things (no edgecases found... yet...). 
Also does things like show a progress bar while it's saving your work, etc.

Currently all annotations are *baked into the PDF* that you choose to save to.

### Compile

Currently no releases available, but you can compile it with the following command (if you have .NET installed):
```nu-script
dotnet publish
```
The resulting binary will end up here: `./bin/Release/net9.0-windows/win-x64/publish/PDFEater.exe`!
Also seems to work smoothly on Linux using Wine!

