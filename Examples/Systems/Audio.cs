using Pure.Engine.Audio;
using Pure.Engine.Utilities;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class Audio
{
    public static void Run()
    {
        Window.Title = "Pure - Audio Example";

        var jingleBells =
            ".5 E4 . E4 . E4 .2 E4 . E4 . E4 .2 E4 . G4 . C4 . D4 . E4~4 " +
            "F4 . F4 . F4 . F4 . F4 . E4 . E4 . E4 . E4 . " +
            "D4 . D4 . E4 . D4~2 . G4~2 . " +
            "E4 . E4 . E4~2 . E4 . E4 . E4~2 . E4 . G4 . C4 . D4 . E4~4 " +
            "F4 . F4 . F4 . F4 . F4 . E4 . E4 . E4 . E4 . E4 . " +
            "G4 . G4 . F4 . D4 . C4~3";
        var titanic =
            "F3~3 .2 F3 . F3~2 .2 F3~2 .2 E3~2 .2 F3~4 .2 F3~2 .2 E3~2 .2 F3~4 .2 G3~2 .2 A3~5 .2 G3~4 .3 " +
            "F3~3 .2 F3 . F3~2 .2 F3~2 .2 E3~2 .2 F3~4 .2 F3~2 .2 C3~6 .6 " +
            ".5 F3~3 .2 F3 . F3~2 .2 F3~2 .2 E3~2 .2 F3~4 .2 F3~2 .2 E3~2 .2 F3~4 .2 G3~2 .2 A3~5 .2 G3~4 .3 " +
            "F3~3 .2 F3 . F3~2 .2 F3~2 .2 E3~2 .2 F3~4 .2 F3~2 .2 C3~6 .6 " +
            "F3~6 .3 G3~6 .2 C3~2 .2 C4~4 .2 A#3~4 .2 A3 . G3~4 .4 " +
            "A3~3 .2 A#3~2 .2 A3~4 .2 G3~3 .2 F3 . E3~2 .2 F3~3 .3 E3~2 .2 D3~6 .3 C3~6 .4 " +
            "F3~5 .3 G3~5 .3 C3~2 .2 C4~4 .2 A#3~2 .2 A3~2 .2 G3~3 .4 " +
            "A3~2 .2 A#3~2 .2 A3~3 .3 G3~2 .2 F3~2 .2 E3~3 .2 F3~4 .2 E3~2 .2 E3~3 .2 F3~4 .2 G3~2 .2 A3~4 .2 G3~4 .2 F3~6";

        var track1 = new Notes(jingleBells, 0.2f, Wave.Square, (0.5f, 0.5f)) { Volume = 0.2f };
        var track2 = new Notes(titanic, 0.2f, Wave.Sine, (1f, 1f)) { Volume = 0.7f, Pitch = 1.2f };
        var playlist = new AudioPack(0.5f, true, track1, track2);
        playlist.Play();
        playlist.OnEnd(() => Console.WriteLine($"Playlist ended"));
        playlist.OnTrackEndAny(audio => Console.WriteLine($"Any Index {audio.index} ended"));
        playlist.OnTrackEndIndex(0, audio => Console.WriteLine($"Index {audio.index} ended"));
        playlist.OnTrackEndIndex(1, audio => Console.WriteLine($"Index {audio.index} ended"));

        while (Window.KeepOpen())
        {
            Time.Update();
            playlist.Update(Time.Delta);
        }
    }
}