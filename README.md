# Moonshine Run Game

***

### "Moonshine Run" is the working title for my current solo game development endeavour.
It will be a simulation / tycoon game about starting and growing an illegal moonshine empire.

***

### Grid-based building system

There will be locations for the player to unlock where they will be able to place down objects, components of their business such as workstations, water barrels, distillers, etc.

The first thing I built has been this grid-based building system, it can be given two Vector2 positions of the bottom-left and top-right corner of the desired build zone.
When the player enters 'build mode' using the B key, the build zone will light up with a checker pattern to indicate it's position, and a cursor object will snap to the currently hovered-over grid square.

![alt text](images/build-system-demo-1.png, "Grid based building system")

The build zone will automatically align an object to its correct position (centre of the object as close to the cursor as possible) based on the width and height in tiles of the object. It can also handle the rotation of an object with the R key.
The ghost object will highlight green or red, depending on if the object can be placed at its current location. 
If a large enough location is near by, the build zone will automatically snap the object to it, with some unwanted exceptions I am working on fixing.

![alt text](images/build-system-demo-2.png, "Building system collision and snap to available grid")

I also created a basic inventory as seen in the top-left corner. The design is temporary but it functions quite well for my application.

***

### Working on UI and Merchant NPC

Currently playing around with basic UI designs, The next thing I want to add is a merchant that sells some items to the player, such as water barrels and ingredients.
The merchant is going to get an interface where the player can filter by category, select an item, view it's details and enter a quantity and purchase.
I will need to add a currency system.

![alt text](images/wip-merchant-ui.png, "Current merchant UI design")
