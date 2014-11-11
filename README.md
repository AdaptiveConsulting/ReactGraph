![Logo](https://raw.githubusercontent.com/AdaptiveConsulting/ReactGraph/master/icon/package_icon.png) ReactGraph
==========

ReactGraph is a library to make change propagation easy in .NET. It allows you to define formulas targeting properties and when any value used in that formula is changed it will be re-evaluated.

What makes ReactGraph useful is that it does not blindly react to changes, when any property changes it will perform a breadth-first topological sort to ensure each property is only recalculated once.

### A simple example
Given we have a simple dependency graph containing 4 properties. A, B, C and D.

What we want is
```
C = A + B
D = A + C
```

If we listen to changes on A B and C then when we change A, it will cause C and D to recalculate. When C recalculates D will be recalculated again, this is less than ideal.

With ReactGraph we understand that D relies on both A and C, so we will recalculate both those fields then recalculate D after. In react graph, this is how we define our formulas.

``` csharp
var sample = new Sample()
var engine = new DependencyEngine();
engine.Assign(() => foo.C).From(() => foo.A + foo.B, e => { });
engine.Assign(() => foo.D).From(() => foo.A + foo.C, e => { });
```

That is it, a simple delclarive way to specify dependencies between different Properties. When any value changes ReactGraph will use a breadth-first, topologically-sorted order to propogate and recalculate new values so each formula will be executed only once, just like excel does.

We can then visualise the dependencies in dot format (using GraphViz for example http://stamm-wilbrandt.de/GraphvizFiddle/)

For example:

Default:  
![image](https://cloud.githubusercontent.com/assets/453152/4478542/66b8150c-4986-11e4-9c89-bc3ebee38a87.png)

Include roots:  
![image](https://cloud.githubusercontent.com/assets/453152/4478556/7b6b1080-4986-11e4-9cd0-23e20cab3ead.png)

Exclude formulas:  
![image](https://cloud.githubusercontent.com/assets/453152/4478562/8e67d308-4986-11e4-801d-10f25714e5d4.png)

### Icon
Network by Murali Krishna from The Noun Project
