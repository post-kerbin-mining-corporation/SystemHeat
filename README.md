# System Heat

A mod for Kerbal Space Program, intended to proovide a better experience for heat management, particularly geared towards resource extraction, high energy engines, and nuclear reactors.

* [Features](#features)
* [Dependencies](#dependencies)
* [Installation](#installation)
* [Contributing](#contributing)
* [License](#licensing)

## Features

In a nutshell, this mod has as goals.
* Create a coherent vision for the system-level heat mechanics in KSP
* Design a fairly simple but deep system built around designing ships to manage heat
* Provide better tools and visualizations to help players with those designs

### Gameplay Concept

Besides building a system that is stable and extensible without the show-stopping bugs of Core Heat, I want to represent a couple of design challenges that come up in spacecraft design. Spacecraft usually have to deal with a number of different thermal systems for different purposes, due to the varying heat generation requirements of various things on them. Humans make heat, electricity makes heat, nuclear engines make heat... and they tend to do so at different rates and in different ways. You usually can't just put 50 Thermal Control System (large) parts on the ship and you're all happy. 

The SystemHeat concept will basically take this and run with it. Allocating your heat producing parts correctly between your radiators will be important, as well as making sure you don't do stuff like pipe your reactor exhaust into your life support system. Fun times!

### Heat Loops

In this mod, all parts that interact with the system are part of an abstract Heat Loop. This represents the flow of coolant through a set of systems that produce heat and consume heat. Producers might include reactors, life support processors, ISRUs, drills and that kind of thing. Radiators would be consumers (effectively the only one at this point).

Each part that produces heat does so with an Output Temperature and an Output Flux. The Output Temperature this is the temperature of the insides of the part (say, a nuclear reactor core) - any coolant coming out of the part will be at that temperature. The Output Flux is the rate heat is produced by the part. Parts have different temperatures and fluxes depending on their nature - a nuclear reactor has a pretty high Output Temperature and a high Output Flux, a drill might have a lower Output Flux and a lower Output Temperature.

Similarly, parts that consume heat have special properties. They specifically consume heat with an Input Flux - a bigger, better radiator is likely able to handle a higher input flux. 

When parts are connected into a Heat Loop , the members of the loop contribute their Input/Output Flux and Output Temperature into it. If all the fluxes going into the loop are positive, it heats up and increases its Loop Temperature. If the flux is negative, it cools down. If the Loop Temperature gets too high, systems in the loop will be negatively affected, in proportion to how different the Loop Temperature is to their Output Temperature.

Your job, as the spacecraft engineer, is to construct Heat Loops such that they are well-balanced and basically don't heat up and . There can be several different Heat Loops on a vessel with different parts assigned to them (though a part can currently only belong to one loop at a time).

## Installation

To install, place the GameData folder inside your Kerbal Space Program folder. If asked to overwrite files, please do so.

NOTE: Do NOT rename or move folders within the GameData folder - this mod uses absolute paths to assets and will break if this happens.

## Contributing

I certainly accept pull requests. Please target all such things to the `dev` branch though!

## Licensing

MIT license:

Copyright (c) 2019 Chris Adderley
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
