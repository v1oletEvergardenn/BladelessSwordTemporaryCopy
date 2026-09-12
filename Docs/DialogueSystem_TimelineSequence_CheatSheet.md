# Dialogue System Timeline Sequence Commands Cheat Sheet

This project supports custom timeline commands for Dialogue System sequences.

---

## Quick Start

### Recommended cutscene pattern
1. Start cutscene timeline: `PlayTimeLine(CutsceneDirectorName)`
2. Pause when needed: `PauseTimeline()`
3. Continue from paused time: `ResumeTimeLine()`

---

## Command Reference

## 1) `PlayTimeLine(...)`
Plays a `PlayableDirector` and tracks it as current in `TimelineManager`.

### Signature
`PlayTimeLine(timelineName)`

### Parameters
- `timelineName` (required):
  - scene `GameObject` name containing a `PlayableDirector`, or
  - director name, or
  - playable asset name (fallback match)

### Examples
- `PlayTimeLine(IntroCutsceneDirector)`
- `PlayTimeLine(BossPhase1Timeline)`

---

## 2) `PauseTimeline()`
Pauses the current tracked timeline at its current time.

### Signature
`PauseTimeline()`

### Behavior
- Uses `TimelineManager.CurrentTimeline`.
- Calls `PlayableDirector.Pause()`.
- Keeps timeline time for resume.

### Example
- `PauseTimeline()`

---

## 3) `ResumeTimeLine()`
Resumes the current tracked timeline from paused time.

### Signature
`ResumeTimeLine()`

### Behavior
- Calls `PlayableDirector.Resume()`.
- If no timeline is tracked, no-op.

### Example
- `ResumeTimeLine()`

---

## Sequence Examples

### Basic timeline control
`PlayTimeLine(IntroCutsceneDirector);Delay(1.5);PauseTimeline();Delay(0.5);ResumeTimeLine()`

### Message-driven control
`PlayTimeLine(BossIntro)->Message(TLStarted);PauseTimeline()@Message(PauseTL)->Message(TLPaused);ResumeTimeLine()@Message(ResumeTL)`

---

## DialogueCommandTrack / DialogueCommandClip Setup

To use these from `DialogueCommandTrack` clip UI:

1. In `DialogueCommandClip`, add a command item.
2. Set `commandType` to `Timeline`.
3. Set `timelineCommandType`:
   - `PauseTimeline`
   - `ResumeTimeLine`
   - `PlayTimeLine`
4. If `PlayTimeLine`, set `timelineName`.

This generates sequence commands:
- `PauseTimeline()`
- `ResumeTimeLine()`
- `PlayTimeLine(TimelineName)`

---

## Notes

- A `TimelineManager` must exist in scene.
- Best practice: start timelines through `TimelineManager.PlayTimeline(...)` so current timeline tracking stays correct.
- Command names are shown in project style and should be used exactly as written above.