using System;
using System.Media;
using System.IO;
using System.Windows.Media;

namespace CheckersApp
{
    public static class SoundManager
    {
        private static readonly MediaPlayer _movePlayer = new MediaPlayer();
        private static readonly MediaPlayer _capturePlayer = new MediaPlayer();
        private static readonly MediaPlayer _victoryPlayer = new MediaPlayer();

        private static bool _soundsLoaded = false;

        static SoundManager()
        {
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string soundsPath = Path.Combine(basePath, "Sounds");

                if (!Directory.Exists(soundsPath))
                {
                    soundsPath = Path.Combine(basePath, "..\\..\\Sounds");
                }

                if (Directory.Exists(soundsPath))
                {
                    LoadSound(_movePlayer, Path.Combine(soundsPath, "move.wav"));
                    LoadSound(_capturePlayer, Path.Combine(soundsPath, "capture.wav"));
                    LoadSound(_victoryPlayer, Path.Combine(soundsPath, "victory.wav"));
                    _soundsLoaded = true;
                }
                else
                {
                    UseSystemSounds();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации звуков: {ex.Message}");
                UseSystemSounds();
            }
        }

        private static void LoadSound(MediaPlayer player, string filePath)
        {
            if (File.Exists(filePath))
            {
                player.Open(new Uri(filePath, UriKind.RelativeOrAbsolute));
                player.MediaEnded += (s, e) => player.Stop();
            }
        }

        private static void UseSystemSounds()
        {
            _soundsLoaded = false;
            System.Diagnostics.Debug.WriteLine("Используются системные звуки");
        }

        private static void PlaySound(MediaPlayer player)
        {
            try
            {
                if (_soundsLoaded)
                {
                    player.Stop();
                    player.Position = TimeSpan.Zero;
                    player.Play();
                }
                else
                {
                    // Запасной вариант - системные звуки
                    PlaySystemSound();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения: {ex.Message}");
                PlaySystemSound();
            }
        }

        private static void PlaySystemSound()
        {
            SystemSounds.Beep.Play();
        }

        public static void PlayMoveSound()
        {
            PlaySound(_movePlayer);
        }

        public static void PlayCaptureSound()
        {
            if (!_soundsLoaded)
            {
                SystemSounds.Hand.Play();
                return;
            }
            PlaySound(_capturePlayer);
        }

        public static void PlayVictorySound()
        {
            if (!_soundsLoaded)
            {
                SystemSounds.Exclamation.Play();
                return;
            }
            PlaySound(_victoryPlayer);
        }
    }
}