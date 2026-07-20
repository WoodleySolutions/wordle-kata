# Wordle Engine Kata

Let's build an engine for the game **Wordle**. The engine should provide the core functionality to play the game.

---

## Core Requirement

**Feedback on Guesses**

The engine should accept a guess and a secret word, then provide feedback for each letter:
- `G` (Green): Letter is correct and in the correct position.
- `Y` (Yellow): Letter is in the word but in the wrong position.
- `-` (Gray): Letter is not in the word.

---

## Examples of Feedback

If the secret word is `PLANE`:

- Guessing `PLANE` returns: `GGGGG`
- Guessing `DITCH` returns: `-----`
- Guessing `WORLD` returns: `---Y-`
- Guessing `LEVER` returns: `YY---`
- Guessing `ELATE` returns: `-GG-G`

---

## Enhancements

1. **Tracking Guesses**
   - Track all previous guesses and their feedback so the user can review them.
2. **Input Validation**
   - Each guess must be exactly 5 letters long.
   - Invalid input should not count as an attempt.
3. **Guess Limiting**
   - The user has six attempts to guess the secret word.
   - If the user guesses correctly, declare victory.
   - If the user exhausts all attempts, reveal the solution.
4. **Hard Mode**
   - Players must use all `G` and `Y` letters from previous guesses in subsequent guesses.
5. **Randomized Target Word**
   - Select the secret word randomly from a pre-defined list.

---

*Rebuilt after the pairing session (2026-07-20) — original session work, recreated and completed with full TDD.*
