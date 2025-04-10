# 🎮 Match-3 Dots Grid Game (Unity)

A flexible and extendable match-3 style puzzle game created in Unity, featuring multiple dot types, grid logic, connection handling, and bomb mechanics — built with clean code, modular architecture, and SOLID principles.

---

## Code structure

### 🧩 Dependency Injection with Zenject

This project uses **[Zenject](https://github.com/modesttree/Zenject)** to manage dependencies and enforce a loosely-coupled architecture.

One key usage is injecting the `GameManager` into the `InputHandler`:
- Prevents inputs from being handled during invalid grid states (e.g., during explosions or refill).
- Ensures clean separation of responsibilities.

### 📡 Observer Pattern via EventManager (Pub-Sub)
The game uses a centralized EventManager to implement a Publisher-Subscriber (Observer) pattern for in-game communication.

Use Cases:
- Sending drag events from `InputHandler` to `DragController`.
- Broadcasting when normal bombs `BombDot` explode.
- Notifying state changes like `IDLE`, `DOTS_SOLVED`, and `GRID_LOADING` from `GridManager` to `GameManager`

### ⚡ Async Operations with UniTask

The project uses [`UniTask`](https://github.com/Cysharp/UniTask) to handle asynchronous operations efficiently without blocking the main Unity thread. This is especially useful for:

- **Grid Refill Logic:**  
  Await until the grid is updated and do the refill after it.


### 🎞️ Animations with LeanTween

The project leverages [`LeanTween`](https://assetstore.unity.com/packages/tools/animation/leantween-3595) for lightweight and flexible animations. Key usages include:

- **Dot Movement:**  
  Smooth transitions of dots falling into place using `LeanTween.move`.
- **Dot Clearing:**  
  Clear a dot using `LeanTween.scale`.
- **Explosion Feedback:**  
  Visual feedback when a bomb explodes or a dot is cleared.

---

## 🧠 Assumptions & Design Choices

- Dots are created via **factory pattern** using prefabs and instatiated by `DotsManager`.
- **Zenject** is used for dependency injection and better testability.
- Interface-based design (`IConnectable`, `IDot`) for extensibility.
- Custom **event system** allows loose coupling between input and grid logic.
- Uses **flood fill** to check connected dots and potential matches.
- Dots can be shuffled using a utility function when no valid matches remain.
- Colored bomb dot cannot be a starting dot and only be spawned in exact 9-dot line.
- Normal bomb dot canno

---

## ✅ Features Implemented

### 🔷 Core Gameplay
- Grid is fixed-size (e.g., 7x7) but still can be customized.
- Dynamic and randomly grid creation with `DotTile` system
- Dot spawning, collapsing, and refill mechanics
- Drag-to-connect line system with visual feedback
- Connect at least 3 dots to create a line
- LineRenderer that updates with pointer during drag
- Dot has 5 different possible colors and can be customized

### ⚙️ Grid Refill System
- The grid automatically refills when a line of dots is cleared or a bomb is triggered.
- Refill logic is coordinated between the `GridManager` and `DotsManager` to ensure dots fall into empty spaces and new ones spawn at the top.
- Uses a state machine (`GridState`) to manage transitions and prevent overlap between input and refill operations.


### 🧪 Debugging Tools
Several developer tools are provided to assist in testing and debugging:

- **Change Dot Color:**  
  Right-click on any dot in play mode to change its color.  
  > *Handled by:* `DotColorDebugger`

- **Highlight Possible Matches:**  
  Press the `Space` key to highlight any valid chains or connectable paths.  
  > *Handled by:* `GridManager` & `GridSolver`

- **Shuffle Grid:**  
  Click the "Shuffle" button to randomly rearrange the grid while preserving dot colors.  
  > *Handled by:* `GridManager` & `GridShuffler`

### 🔷 Dot Types & Special Tiles
Various dot types are implemented to introduce special behaviors and game mechanics:

- **`NormalDot`**  
  Standard match-3 dot. Can be connected in chains with others of the same color.

- **`BombDot`**  
  Triggers an area explosion (3x3) when clicked, removing surrounding dots. Spawned in the end of a 6-dot line

- **`ColoredBombDot`**  
  Acts as a wildcard that connects with any dot of the same color. Useful for forming extended chains. Spawned in the end of a 9-dot line.
