# RAXY Sequence

A lightweight, decoupled sequence architecture and execution runner for Unity.

## Features
- `ISequenceable`: Unified interface for sequence actions (`Label`, `OverrideNextActionTime`, `NextActionTime`, `ExecuteAsync`).
- `SequenceableBase`: Abstract base class with Odin Inspector-friendly serialized scheduling fields for delay and overlap execution.
- `SequenceRunner`: Standardized async scheduler that manages both sequential execution and start-relative delay/overlap execution, ensuring all started background tasks are properly awaited.
