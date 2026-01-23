# Nemesis Euchre Roadmap

## Initial Project Structure 0.1

1. ~~Create Console Application~~
2. ~~Add Command Library~~
3. ~~Add Dependency Injection~~
4. ~~Add Unit Testing~~
5. ~~Add Versioning~~
7. ~~Add Linting and Analyzers~~
6. ~~Set up Claude w/ basic skills and instructions files~~

## Euchre Game Engine 0.2

1. ~~Build GameModels~~
2. ~~Optimize GameModels for Machine Learning~~
   * ~~Convert Suits to Relative Suits (Trump vs Not Trump) for reduced number of unique states~~
   * ~~Convert Positions to Relative Positions (Self, Partner, Left Hand Opponent, Right Hand Opponent)~~
3. ~~Create GameOrchestrator~~
4. ~~Create IPlayerActor to provide methods for all player decisions that need to be made during a game~~
   * ~~Calling Trump~~
   * ~~Discarding when Ordered Up~~
   * ~~Playing Cards~~
5. ~~Implement IPlayerActor in simple bots for data generation in order to seed machine learning~~
6. ~~Shuffle Cards and create Deals~~
7. ~~Create DealOrchestrator~~
8. ~~Update Score and return GameModel once complete~~
9. ~~Orchestrate Selecting Trump by looping through IPlayerActors~~
10. Orchestrate Tricks (Playing Cards) by looping through IPlayerActors
11. Calculate Trick Winner
12. Update Deal with result once complete
13. Update Console App to Play a game when called

## Store Game Results 0.3

1. Add CallTrumpDecisions to GameModel with relevant Machine Learning data
2. Add DiscardCardDecisions to GameModel with relevant Machine Learning data
3. Add PlayCardDecisions to GameModel with relevant Machine Learning data
4. Stand up Database (SQL?, Mongo?, HomeServer?, Azure?)
5. Create DataModels (Entity Framework?)
6. Create Tables?
7. Create Indexes?
8. Store completed games in the Database
9. Update Console App to run multiple games simultaneously in parallel

## Machine Learning 0.4

1. Use Console App to Generate TONS of Euchre Game data using ChaosBots
2. Use Machine Learning? to analyze data in order to create a new bot (Gen1) that can play better than ChaosBot (Use R?, Stored Procedures?)
3. Implement the new (Gen1) Bot
4. Use Console App to Generate TONS of Euchre Game data using Gen1Bots
5. etc.

## User Players 1.0

1. Add a PlayGame command to allow interactive play
2. Implement IPlayerActor with methods in NemesisEuchre.Console for handling player actions
3. Create Display for Cards in terminal ASCII art
4. Add interactivity for selecting an option or card from the terminal
5. Create Display for current state of Deal
6. Create Display for current state of Trick
7. Implement Call Trump action in the terminal
8. Implement Discard Card action in the terminal
9. Implement Play Card action in the terminal
10. Add animations? to simulate bot actions and allow for human reactions
11. CONSIDER Adding hooks into the GameEngine to allow players to get GameState updates in more real time (Would allow for multiple players in the future)
12. Play games against ChaosBot to record win percentage
13. Play games against GenX bots to record win percentage and prove they are improving