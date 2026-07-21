# Wordle Engine

A test-driven Wordle game engine in C# (.NET 9, xUnit), built as a follow-up to a pairing session with Source Allies. Original kata requirements are in [this gist](https://gist.github.com/kylehoehns/7b3e7644f1c59f84113ee128522d438a).

## Running

```
make init      # build
make verify    # run tests
```

## Features

- **Guess scoring** — `G` correct position, `Y` wrong position, `-` not in word. Duplicate letters are handled with a two-pass tally: greens claim their letters first, then yellows spend what remains, left to right. (`GEESE`/`EMCEE` and `ELATE`/`EEEEE` in the test suite pin the tricky cases.)
- **Guess history** — every valid guess and its feedback, exposed read-only.
- **Input validation** — guesses must match the secret's length and be letters only. Invalid input never consumes an attempt.
- **Game lifecycle** — six attempts; the engine exposes `Status` (`InProgress` / `Won` / `Lost`) and reveals `Solution` only once the game is over. Guessing after the game ends throws.
- **Hard mode** — opt-in via constructor. Implemented per official Wordle rules: green letters are locked to their position, yellow letters must appear somewhere in every subsequent guess. Violations are rejected without consuming an attempt.
- **Random word selection** — a static factory picks the secret from a built-in word list. The `Random` is injectable, so tests stay deterministic.

## Design notes

The public `Guess` method is a thin orchestrator — validate, score, record, update state — with each concern in its own private method. Scoring itself is a pure function; game state (attempts, history, status) lives in the shell around it. Status is exposed as queryable state rather than encoded into the feedback string, leaving presentation decisions ("declare victory", "reveal the solution") to whatever front end consumes the engine.

Built strictly test-first; the commit history is the red-green-refactor log, including mid-kata re-engineering when triple-letter test cases broke the original scoring approach.

## Possible next steps

- Case normalization (`ToUpperInvariant`) for mixed-case input
- Dictionary validation of guesses against a word list
- Splitting the pure scorer into its own class for reuse (e.g., scoring hypothetical guesses)
- A `GuessResult` record or events for front ends that want push semantics