# KarambaIDEAtest
KarambaIDEA is Grasshopper plug-in that imports output from Karamba3D to IDEA Statica Connection. The plug-in KarambaIDEA is developed as part of the research project SMARTconnection. More information about the research project can be found at: https://www.bouwenmetstaal.nl/themas/parametrisch-ontwerpen/smartconnection/

This repository contains the source-code of the plug-in KarambaIDEA. The plug-in can be downloaded at: https://www.food4rhino.com/app/karambaidea

KarambaIDEA is not official software of either Karamba3D or IDEA Statica Connection.

## Installation
The source-code can be opened in Visual Studio. The following software packages are needed in order to run. KarambaIDEA makes use of DLL-files. Therefore IDEA Statica Connection and Rhino 6 should be installed in the folder specified below.

Needed software:
* IDEA Statica Connection version 9.1     (installed in folder C:\Program Files\IDEAStatiCa\StatiCa9)
* Rhino 6                                 (installed in folder C:\Program Files\Rhino 6)
* Karamba3D
* .NET Framework 4.7 (to built solution)
  

In most cases the following references are not being found, when opening the project folder for the first time. Update the following references.

References (to update)
*GH_IO                              path C:\Program Files\Rhino 6\Plug-ins\Grasshopper\GH_IO.dll
*Grasshopper                        path C:\Program Files\Rhino 6\Plug-ins\Grasshopper\Grasshopper.dll
*RhinoCommon                        path C:\Program Files\Rhino 6\System\RhinoCommon.dll
*IdeaStatiCa.ConnectionBasicTypes   path C:\Program Files\IDEAStatiCa\StatiCa9\IdeaStatiCa.ConnectionBasicTypes.dll


## Usage

This repository contains two folders: KarambaIDEA and Tester.

The Tester folder, can be used to test the link between IDEA Statica Connection and raw data.
The KarambaIDEA folder, is the folder that builts the plug-in. When building this folder, Rhino 6 will start.

The plug-in can only be found if
GrasshopperDeveloperSettings

https://developer.rhino3d.com/guides/grasshopper/your-first-component-windows/

```c#
TODO: include usage explaination/ link to tutorial videos
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[Apache License 2.0](https://choosealicense.com/licenses/apache-2.0/#)

