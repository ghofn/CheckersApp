using System;
using System.Media;
using System.IO;
using System.Windows.Media;
using System.Threading.Tasks;

namespace CheckersApp
{
    public static class SoundManager
    {
        private static MediaPlayer _backgroundPlayer;
        private static MediaPlayer _whiteVictoryPlayer;
        private static MediaPlayer _blackVictoryPlayer;

        private static SoundPlayer _moveSound;
        private static SoundPlayer _captureSound;
        private static SoundPlayer _kingSound;

        private static bool _soundsEnabled = true;
        private static bool _musicEnabled = true;
        private static bool _isVictoryPlaying = false;

        static SoundManager()
        {
            InitializeSounds();
        }

        private static void InitializeSounds()
        {
            try
            {
                string soundsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds");

                if (!Directory.Exists(soundsPath))
                {
                    Directory.CreateDirectory(soundsPath);
                    Console.WriteLine("✅ Создана папка Sounds");
                }

                // Короткие звуки
                LoadSound(ref _moveSound, Path.Combine(soundsPath, "move.wav"));
                LoadSound(ref _captureSound, Path.Combine(soundsPath, "capture.wav"));
                LoadSound(ref _kingSound, Path.Combine(soundsPath, "king.wav"));

                // Фоновая музыка
                string bgMusicPath = Path.Combine(soundsPath, "background_music.wav");
                if (File.Exists(bgMusicPath))
                {
                    _backgroundPlayer = new MediaPlayer();
                    _backgroundPlayer.Open(new Uri(bgMusicPath));
                    _backgroundPlayer.MediaEnded += (s, e) =>
                    {
                        if (_musicEnabled && !_isVictoryPlaying)
                        {
                            _backgroundPlayer.Position = TimeSpan.Zero;
                            _backgroundPlayer.Play();
                        }
                    };
                    _backgroundPlayer.Volume = 0.3;
                }

                // Победа белых
                string whiteVictoryPath = Path.Combine(soundsPath, "white_victory.wav");
                if (File.Exists(whiteVictoryPath))
                {
                    _whiteVictoryPlayer = new MediaPlayer();
                    _whiteVictoryPlayer.Open(new Uri(whiteVictoryPath));
                    _whiteVictoryPlayer.MediaEnded += (s, e) =>
                    {
                        _isVictoryPlaying = false;
                        if (_musicEnabled)
                        {
                            _backgroundPlayer?.Play();
                        }
                    };
                }

                // Победа черных
                string blackVictoryPath = Path.Combine(soundsPath, "black_victory.wav");
                if (File.Exists(blackVictoryPath))
                {
                    _blackVictoryPlayer = new MediaPlayer();
                    _blackVictoryPlayer.Open(new Uri(blackVictoryPath));
                    _blackVictoryPlayer.MediaEnded += (s, e) =>
                    {
                        _isVictoryPlaying = false;
                        if (_musicEnabled)
                        {
                            _backgroundPlayer?.Play();
                        }
                    };
                }

                Console.WriteLine("✅ Звуковая система готова");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка инициализации звуков: {ex.Message}");
            }
        }

        private static void LoadSound(ref SoundPlayer player, string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    player = new SoundPlayer(filePath);
                    player.LoadAsync();
                }
            }
            catch { }
        }

        public static void PlayMoveSound()
        {
            if (!_soundsEnabled) return;
            if (_moveSound != null)
                _moveSound.Play();
            else
                SystemSounds.Asterisk.Play();
        }

        public static void PlayCaptureSound()
        {
            if (!_soundsEnabled) return;
            if (_captureSound != null)
                _captureSound.Play();
            else
                SystemSounds.Exclamation.Play();
        }

        public static void PlayKingSound()
        {
            if (!_soundsEnabled) return;
            if (_kingSound != null)
                _kingSound.Play();
            else
                SystemSounds.Beep.Play();
        }

        public static void PlayWhiteVictorySound()
        {
            if (!_soundsEnabled) return;

            _backgroundPlayer?.Pause();
            _isVictoryPlaying = true;

            if (_whiteVictoryPlayer != null)
            {
                _whiteVictoryPlayer.Position = TimeSpan.Zero;
                _whiteVictoryPlayer.Play();
            }
            else
            {
                SystemSounds.Hand.Play();
                Task.Delay(3000).ContinueWith(_ =>
                {
                    _isVictoryPlaying = false;
                    if (_musicEnabled)
                        _backgroundPlayer?.Play();
                });
            }
        }

        public static void PlayBlackVictorySound()
        {
            if (!_soundsEnabled) return;

            _backgroundPlayer?.Pause();
            _isVictoryPlaying = true;

            if (_blackVictoryPlayer != null)
            {
                _blackVictoryPlayer.Position = TimeSpan.Zero;
                _blackVictoryPlayer.Play();
            }
            else
            {
                SystemSounds.Question.Play();
                Task.Delay(3000).ContinueWith(_ =>
                {
                    _isVictoryPlaying = false;
                    if (_musicEnabled)
                        _backgroundPlayer?.Play();
                });
            }
        }

        public static void StartBackgroundMusic()
        {
            if (_musicEnabled && !_isVictoryPlaying && _backgroundPlayer != null)
            {
                _backgroundPlayer.Play();
            }
        }

        public static void StopBackgroundMusic()
        {
            _backgroundPlayer?.Pause();
        }

        public static void ResumeBackgroundMusic()
        {
            _isVictoryPlaying = false;
            if (_musicEnabled && _backgroundPlayer != null)
            {
                _backgroundPlayer.Play();
            }
        }

        public static void ToggleSounds()
        {
            _soundsEnabled = !_soundsEnabled;
        }

        public static void ToggleMusic()
        {
            _musicEnabled = !_musicEnabled;
            if (_musicEnabled && !_isVictoryPlaying)
                StartBackgroundMusic();
            else
                StopBackgroundMusic();
        }

        public static bool SoundsEnabled => _soundsEnabled;
        public static bool MusicEnabled => _musicEnabled;
    }
}