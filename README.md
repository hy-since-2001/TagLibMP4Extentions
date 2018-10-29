# TagLibMP4Extentions
Set/Get Meta Data Tag to MP4

# Edit Area 
<img alt="Readme" src="https://github.com/hy-since-2001/TagLibMP4Extentions/blob/master/Readme_image1.png" />

# Prerequisites
~~~~
.NET Framework 4.5 ～
taglib 2.1.0 
 nuget PM> Install-Package taglib -Version 2.1.0
~~~~

# Usage
~~~~
string mp4Path = Path.Combine(Environment.CurrentDirectory, "Test.mp4");
//Set
await TagLibMP4Extentions.SetMetaTagAsync(mp4Path, new string[] { "test1", "test2", "test3"} );
//Get
var getTag = await TagLibMP4Extentions.GetMetaTagAsync(mp4Path);
~~~~

# Licence
[LGPL-2.1 License](https://opensource.org/licenses/lgpl-2.1.php)

