using System;

public static class RhythmTiming
{
    public static double BeatDuration(double bpm)
    {
        return 60.0d / bpm;
    }

    public static double BeatToTime(double beat, double bpm)
    {
        return beat * BeatDuration(bpm);
    }

    public static double TimeToBeat(double time, double bpm)
    {
        return time / BeatDuration(bpm);
    }

    // division: 1=quarter, 2=eighth, 4=sixteenth
    public static double SnapTimeToBeatGrid(double time, double bpm, int division)
    {
        if (division < 1)
            division = 1;

        double beat = TimeToBeat(time, bpm);
        double snappedBeat = Math.Round(beat * division) / division;
        return BeatToTime(snappedBeat, bpm);
    }
}