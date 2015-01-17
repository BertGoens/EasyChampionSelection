EasyChampionSelection
=====================

**Downloads are in the description below, not the Download ZIP button! That's if you want the source code.**

A small program to help relieve your stress in champion select.  
Ever had it occuring to you that in the heat of the moment you forgot your champion pool?  
ECS will pop up a window for you so you'll never have to remember them again.
It's smart enough to only show up when it's your turn to pick or ban.

Youtube tutorial: 

#Guide
Install EasyChampionSelection and open it.
(It requires .Net 4.5)

##Riot API key
@Main:Button "Settings"
Go to the settings to get your api key
Once you've obtained it, copy and paste it in the field.

##Configure league client
Start up league of legends, and create a custom game. 
(A box should now appear saying "your lolclient.exe has just been updated")
Now we want to configure our client overlay
@Settings:Button "Configure Client Overlay"

Create a custom game and press Get Current Client Image  
@Configure Client Overlay: Button "Get Current Client Image"
![Button: Get Current Client Image](http://i.imgur.com/1axnVZV.jpg)
Your client should be drawn onto my window.
If that doesn't happen try move the client up a little and retry.
IMPORTANT: After we have our image quickly quit your game in the client.

Now we must set up 3 'checkpoints' so ECS knows when you are in champion select.
These are: the Champion Searchbar filter, the Team Chatbar, the ECS client overlay (where should ECS appear)
For the bars: please insert the red square onto the white part only.
My ECS Client Overlay preference is next to the skins tab as it looks really smooth.  
![Configuring Client Overlay](http://imgur.com/Ft2BziH.jpg)

To set a checkbpoint just select one of the radiobuttons and drag the newly spawned red square to it's position. 
You can adjust the width and height accordingly.
It's better to make them a little smaller than trying to contain the bars.

If your ready configuring just close the window.

Look at the Settings window. The Champion Searchbar, Team Chatbar and Client Overlay should now have diffrent values than 0.
You can close it now.

##Adding Groups
Add all the groups you want, this is how my groups look: 
Top, Jungle, Mid, Adc, Support, Should Test, Ban, Ban King Poro, ...  
![Adding a group](http://imgur.com/70Zw7Je.jpg)

You can move groups up, down, rename and delete them.
Now it's time to get some champions in them.
Just select the group you want and either select a whole buch off them or just add them one by one.
(Tip for fast adding and deleting champions: you can double click them)  
![Adding champions](http://imgur.com/9NX3xPr.jpg)

##Last step
![Get in champion selection](http://imgur.com/IbQLfGS.jpg?1)
Your Easy Champion Selection should be assisting you now.
Have fun!

##Tip:
![Right Click TrayIcon](http://imgur.com/o1mX374.jpg)  
Hiding the main window this way will also hide it from your task bar.
You can set it up to always hide this way in the settings.

#Download
[Dropbox Beta download](https://dl.dropboxusercontent.com/u/105633860/Easy%20Champion%20Selection.msi)
