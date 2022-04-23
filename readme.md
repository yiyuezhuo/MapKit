# MapKit

Provides some helpers to make area movement mechanism for Godot mono.

* Map scrolling
* Selected area highlighting & event.
* A reference data backend using an [external tool](https://github.com/yiyuezhuo/PixelMapPreprocessor).

## Requirements 

* `Newtonsoft.Json`

Add following content to `*.csproj`

```xml
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
  </ItemGroup>
```

For example:

```xml
<Project Sdk="Godot.NET.Sdk/3.3.0">
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <RootNamespace>MapKit</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
  </ItemGroup>
</Project>
```

Or add it using `dotnet` command:

```shell
dotnet add package Newtonsoft.Json
```

## Usage

The addons provides a Japan map sample to show the usage.

### Detailed Pipeline

* Create a [paradox style](https://eu4.paradoxwikis.com/Map_modding) pixel color province map `base_map`:
<a href="addons/MapKit/Sample/sample_map.png"><img src="addons/MapKit/Sample/sample_map.png"></a>
* Use [pixel-map-preprocessor](https://github.com/yiyuezhuo/PixelMapPreprocessor) to create [remap](addons/MapKit/Sample/sample_map_remap.png) and related [json data](addons/MapKit/Sample/sample_map_data.json): `dotnet run your_image_path`.
* Create a `mapData.tres` resource, set the script to `addons/MapKit/Scripts/MapData.cs`, set `Base Texture` to the `base_map`, and set `Region Data Path` to generated json file.
* Create a `map_material.tres` of `ShaderMaterial` and set shader to `addons/MapKit/Shaders/map2.gds`, set `Base Texture` to `base_map`, and set `Remap Texture` to generated remap texture in "Shader Param".
* Create a new scene and instance `addons/MapKit/Sample/SampleMapView.tscn`, set `Ui State Data Res` of MapView node (root node) to `uiStateData.tres`. In Map node, set `Map Data Resource` to `mapData.tres`, texture to `base_map` and material to `map_material.tres`.
* Try to play the scene.
