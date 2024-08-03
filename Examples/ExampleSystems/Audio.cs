namespace Pure.Examples.ExamplesSystems;

using Engine.Audio;
using Engine.Utilities;
using Engine.Window;

public static class Audio
{
    public static void Run()
    {
        Window.Title = "Pure - Audio Example";

        var jingleBells =
            ".5_E4_._E4_._E4_.2_E4_._E4_._E4_.2_E4_._G4_._C4_._D4_._E4~4_" +
            "F4_._F4_._F4_._F4_._F4_._E4_._E4_._E4_._E4_._" +
            "D4_._D4_._E4_._D4~2_._G4~2_._" +
            "E4_._E4_._E4~2_._E4_._E4_._E4~2_._E4_._G4_._C4_._D4_._E4~4_" +
            "F4_._F4_._F4_._F4_._F4_._E4_._E4_._E4_._E4_._E4_._" +
            "G4_._G4_._F4_._D4_._C4~3";
        var titanic =
            "F3~3_.2_F3_._F3~2_.2_F3~2_.2_E3~2_.2_F3~4_.2_F3~2_.2_E3~2_.2_F3~4_.2_G3~2_.2_A3~5_.2_G3~4_.3_" +
            "F3~3_.2_F3_._F3~2_.2_F3~2_.2_E3~2_.2_F3~4_.2_F3~2_.2_C3~6_.6_" +
            ".5_F3~3_.2_F3_._F3~2_.2_F3~2_.2_E3~2_.2_F3~4_.2_F3~2_.2_E3~2_.2_F3~4_.2_G3~2_.2_A3~5_.2_G3~4_.3_" +
            "F3~3_.2_F3_._F3~2_.2_F3~2_.2_E3~2_.2_F3~4_.2_F3~2_.2_C3~6_.6_" +
            "F3~6_.3_G3~6_.2_C3~2_.2_C4~4_.2_A#3~4_.2_A3_._G3~4_.4_" +
            "A3~3_.2_A#3~2_.2_A3~4_.2_G3~3_.2_F3_._E3~2_.2_F3~3_.3_E3~2_.2_D3~6_.3_C3~6_.4_" +
            "F3~5_.3_G3~5_.3_C3~2_.2_C4~4_.2_A#3~2_.2_A3~2_.2_G3~3_.4_" +
            "A3~2_.2_A#3~2_.2_A3~3_.3_G3~2_.2_F3~2_.2_E3~3_.2_F3~4_.2_E3~2_.2_E3~3_.2_F3~4_.2_G3~2_.2_A3~4_.2_G3~4_.2_F3~6";

        var track1 = new Notes(jingleBells, 0.2f, Wave.Square, (0.5f, 0.5f)) { Volume = 0.2f };
        var track2 = new Notes(titanic, 0.2f, Wave.Sine, (1f, 1f)) { Volume = 0.7f, Pitch = 1.2f };
        var playlist = new AudioPack(loop: true);
        playlist.AddTrack(null, track1, track2);
        playlist.Play();
        playlist.OnEnd(() => Console.WriteLine($"Playlist ended"));
        playlist.OnAnyTrackEnd(audio => Console.WriteLine($"Any Index {audio.index} ended"));
        playlist.OnIndexTrackEnd(0, audio => Console.WriteLine($"Index {audio.index} ended"));
        playlist.OnIndexTrackEnd(1, audio => Console.WriteLine($"Index {audio.index} ended"));

        while (Window.KeepOpen())
        {
            Time.Update();
            playlist.Update(Time.Delta);
        }
    }
}