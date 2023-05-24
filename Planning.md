# Game Idea
It is a 3d Third person tower defense game, you are a character and your goal is to run arround and destroy the enemys that come in waves each round, you will build up and reload your towers during the game with different ammo types and you will have your own weapon with infinite ammo to make sure that you can never soft lock yourself.

# Towers
Towers are the main level of defense we will have three types of towers.

Towers will require money to place and can be placed on any tile, there will be a main enemy class that will handle the different behaviours and the other classes will inherit off chaning the behaviour. This inherits from gameobject
### Properties
* Ammo Count
* Level
* Fire Speed
* Accuracy
* Turn Speed

## Sniper tower
Slow to shoot, but very accurate and powerful.

## Machine Gun
Shoots extremely fast with minimal accuracy.

## Turret Gun
This is a happy medium but it probably wont be good against higher level or a lot of enemies so its best to use multiple types.

# World Generation
The world generation is completely procedurally generated we split the world into chunks. Of like 12 by 12 we would choose the side we need to exit and an entrance point and randomly generate the world between. we would get a new tile into the chunk every wave once each chunk is full we get a new chunk. This helps keep the game semi even because it takes longer for enemies to get to the tower while still improving gameplay.

# Enemies
Enemies are spawned based off the wave level we will use a formula to determine the ammount of enemies to spawn and spawn a variety of enemies based off that.
