# Superposition Arcade

[Play the game online on Itch.io!](https://rsoiffer.itch.io/superposition-arcade)

Superposition Arcade is a mind-melting puzzle game built on an realistic simulation of quantum circuits. Your goal is simple: balls fall from the top of the screen, and you need to place pegs to bounce them into the right holes. Each row of pegs is a quantum logic gate, so your choices build up a complex quantum circuit. Gain an intuition for how all the gates work, solve levels based on real quantum circuit identities, and try to find the smallest circuits to solve real-world quantum computing problems!

How to play:
- Drag gates from the box in the lower-left to the board on the left
- Use the gates to bounce all the balls into the matching holes
- Press the Turbo button to speed up the game once you have a solution
- Mouse over a peg to see arrows showing where it sends the balls

## Installing

You can play [online on Itch.io](https://rsoiffer.itch.io/superposition-arcade).

To build the project from source, do the following:
- Install Unity 2022.3.10f1
- Clone this repository
- Open the project in Unity

## Technical Details

Superposition Arcade is built in the Unity game engine. All the code in this repository is my original work. Superposition Arcade does not use any external code libraries.

The quantum circuits in Superposition Arcade are simulated with a simple custom quantum simulator, mostly implemented in the `Assets/Scripts/QData.cs` file. They include a variety of unitary gates, as well as some non-unitary gates, including measurement and noise. The simulator currently does not support classical data, though this is more because of a UI limitation than a technical limitation in the simulator.

Superposition Arcade uses a number of public domain images and sounds. Many of the images and sounds are from [kenney.nl](https://kenney.nl/). The music is from [Juhani on OpenGameArt](https://opengameart.org/content/5-chiptunes-action). The UI, background, and VFX are original work.
