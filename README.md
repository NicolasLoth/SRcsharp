# SRcsharp: Spatial Reasoner in C#

> _A flexible 3D Spatial Reasoning library for Microsoft / Windows Ecosystem

## Features

The SRcharsp library supports the following operations of the [__Spatial Reasoner Syntax__](https://github.com/metason/SpatialReasoner#syntax-of-spatial-inference-pipeline) to specify a spatial inference pipeline.

- [x] __adjust__: optional setup to adjust nearby, sector, and max deviation settings
- [x] __deduce__: optional setup to specify relation categories to be deduced
- [x] __filter__: filter objects by matching spatial attributes
- [x] __pick__: pick objects along their spatial relations
- [x] __select__: select objects having spatial relations with others
- [x] __sort__: sort objects by metric attributes or by spatial relations
- [x] __slice__: choose a subsection of spatial objects 
- [x] __calc__: calculate global variables in fact base
- [x] __map__: calculate values of object attributes
- [ ] __produce__: create new spatial objects driven by their relations (_partially implemented_)
- [x] __reload__: reload all spatial objects of fact base as new input
- [x] __log__: log the current status of the inference pipeline


## Getting Started

### Documentation

See [__Docu on SpatialReasoner__](https://github.com/metason/SpatialReasoner) in separate repository.

### Building and Integrating

Use Visual Studio (Code) to import the SRcsharp library

### Usage

var st = new SpatialTerms();
var obj1 = new SpatialObject(id: "1", position: new SCNVector3(x: -1.5f, y: 1.2f, z: 0), width: 0.1f, height: 1.0f, depth: 0.1f);
var obj2 = new SpatialObject(id: "2", position: new SCNVector3(x: 0, y: 0, z: 0), width: 0.8f, height: 1.0f, depth: 0.6f);
var obj3 = new SpatialObject(id: "3", position: new SCNVector3(x: 0, y: 0, z: 1.6f), width: 0.8f, height: 0.8f, depth: 0.8f);
obj3.Angle = (float)(Math.PI / 2.0);

//Full pipeline input
var sr = new SpatialReasoner();
sr.Load(objs);
var pipeline = "filter(volume > 0.4) | pick(Left OR Above) | log()";
if (sr.Run(pipeline))
{
    Console.WriteLine(string.Join('\n', sr.Result));
}

//Queued methods
SpatialReasoner.Create().Load(objs).Filter("volume>0.4").Pick("Left OR Above").CLog();

//Extensions Methods on IEnumerable
Console.WriteLine(string.Join('\n', SpatialReasoner.Create().Load(objs).Where(so => so.Volume > 0.4).ToList()));

## Tests

TODO: Comments on Tests and Visualizations

## License

Released under the [Creative Commons CC0 License](LICENSE).
