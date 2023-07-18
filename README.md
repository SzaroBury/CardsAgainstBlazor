# CardsAgainstMatey

A simple ASP.NET Core Blazor app that allows users to play Cards Against Humanity.
<p align="center">
  <img src="https://github.com/SzaroBury/CardsAgainstMatey/assets/37550354/4c0b9682-b26a-4fc4-87e8-b3cb32cb1eef"/>
</p>

![image](https://github.com/SzaroBury/CardsAgainstMatey/assets/37550354/5d22d235-561e-47a2-8b0f-b8d1d2fc82ca)

![image](https://github.com/SzaroBury/CardsAgainstMatey/assets/37550354/448653bc-43c8-47d8-a0b4-b8e54abc2957)


## Functionalities:
- Creating and adjusting game rooms
- Importing and exporting cards and sentences for a new game
- Playing the game!

## Technicalities:
- .NET 6.0
- ASP.NET Core Blazor (Blazor Server)
  - The server executes C# code and uses SignalR to update UI elements 
- Bootstrap v5.1.0
- Open Iconic v1.1.1


## To do:
- general tweaks for UI and gameplay
- adding tutorial on the home page
- adding private rooms - with required password to enter the room
- adding availbility to save sets of cards and sentences on the app server (adding db)
- adding chat and game log in game rooms
- reducing the number of requests sent between clients and the server
