# 🎮 BOM! - Multiplayer Word Matching Game

## 📋 Overview

**BOM!** is a fast-paced, local or online multiplayer word-matching party game where players pass paper slips with words to each other, aiming to avoid ending up with the wrong combination. Designed with a school notebook aesthetic and featuring pixel art visuals, BOM! is a light-hearted, social experience playable with 3 to 6 players.

---

## 🧠 Game Objective

Each player receives a word. During their turn, players pass a word to the next player in the circle. The goal is to collect a **unique** set of words and avoid duplication or specific trigger words depending on the mode.

Rounds proceed until a special condition ends the game (in some modes) or players complete a set of turns.

---

## 📘 Game Modes

### 1. **Classic**

- Each player passes one word per turn.
- Players cannot see what words others have until the end.
- Objective: Be the only one with unique words.

### 2. **Fast Mode** _(Planned)_

- Similar to Classic, but time-limited turns.
- Adds urgency and quick thinking.

### 3. **BOMB! Mode** _(Planned)_

- A "bomb word" is randomly added.
- Whoever ends the game holding the bomb loses.
- Adds bluffing and deduction elements.

> _Note: Only Classic Mode is implemented in the current version._

---

## 🎮 How to Play (Flow)

1. **Create a Room**

   - Choose number of players (3–6)
   - Set a game mode (currently defaults to Classic)
   - Customize word list (default words: Word 1, Word 2, …)

2. **Join a Room**

   - Enter player name and room code
   - Connect via Unity Relay system

3. **Lobby**

   - Players see each other’s names
   - Host starts the game when all are ready

4. **Gameplay (In Development)**
   - Players pass paper slips (words)
   - Each round shows updated word set
   - Eventually, winner/loser is declared (depends on mode)

---

## 🏗️ Project Architecture

The project is built using **Unity 2022+**, **Netcode for GameObjects**, and **Unity Relay** for online connectivity.

### 🔁 Scenes

- `MainMenuScene` – Logo and buttons to Join or Create Room
- `CreateRoomScene` – Configure room, player count, word list
- `JoinRoomScene` – Input name and room code to join
- `LobbyScene` – Show current players, start/back buttons
- `GameScene` – Game logic and UI (WIP)

## ☁️ Multiplayer Backend

- Unity Relay handles peer-to-peer matchmaking using join codes.
- Unity Authentication is used for anonymous login.
- Network data such as player names and word lists is synced via `ServerRpc` and `NetworkVariable`.

---

## 🛠️ Features Implemented

- ✅ Dynamic word slot UI based on player count
- ✅ Join code generation and entry
- ✅ Host/client synchronization
- ✅ Lobby with name list
- ✅ Scene switching for host and clients
- ✅ Relay-based network transport setup
- ✅ NetworkManager bootstrap system

---

## 🚧 Features In Progress

- 🟡 Player turn system
- 🟡 Word passing logic
- 🟡 Game end conditions
- 🟡 Classic and Bomb mode implementation

---

## 📦 Dependencies

- [Unity Netcode for GameObjects](https://docs-multiplayer.unity3d.com/)
- [Unity Relay](https://unity.com/relay)
- [Unity Authentication](https://unity.com/authentication)
- TextMeshPro

---

## 📱 Platform Support

- Built with mobile (portrait) layout in mind
- Currently being tested on Android builds
- Also supports desktop testing with multiple instances

---
