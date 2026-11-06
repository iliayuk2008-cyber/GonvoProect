using Plugin.Maui.Audio;

namespace GonvoProect.Services;

public class SoundService
{
    private IAudioPlayer? player;
    private Stream? stream;

    public async Task PlaySound(string fileName)
    {
        Stop();

        stream = await FileSystem.OpenAppPackageFileAsync(fileName);

        player = AudioManager.Current.CreatePlayer(stream);

        player.Volume = 0.7;

        player.PlaybackEnded += OnPlaybackEnded;

        player.Play();
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        DisposePlayer();
    }

    public void Stop()
    {
        if (player != null)
        {
            player.PlaybackEnded -= OnPlaybackEnded;
            player.Stop();
        }

        DisposePlayer();
    }

    private void DisposePlayer()
    {
        if (player != null)
        {
            player.PlaybackEnded -= OnPlaybackEnded;
            player.Dispose();
            player = null;
        }

        stream?.Dispose();
        stream = null;
    }
}