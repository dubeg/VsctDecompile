# README
Decompile the `Menus.ctmenu` resource from dll/cto.

This merely repackages code from `vsct.exe`, `VSCTLibrary.dll` & `VSCTCompress.dll`.

## Purpose
You can either save it to a `.vsct` file, use its in-memory structures.

My goal is to use it to read the commands, buttons and icons defined in Visual Studio packages & generate a cache file or a source file (using a C# source generator) that QuickJump2022 will be able to use for its known commands.


## Related resources
- https://github.com/AmpScm/AnkhSVN/blob/dcc40c3328dba2b7a18f1241019f42a2750c1455/src/tools/Ankh.BitmapExtractor/Program.cs#L13
- https://stackoverflow.com/questions/21545910/can-i-add-my-add-in-into-analyze-section-of-visual-studio-menu
- https://marketplace.visualstudio.com/items?itemName=VisualStudioBlog.VSCTPowertoy
- https://www.dotnetportal.cz/blogy/15/Null-Reference-Exception/5194/Visual-Studio-Package
