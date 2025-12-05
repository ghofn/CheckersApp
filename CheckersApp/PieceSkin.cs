using System.Windows.Media;
using System.Collections.Generic;

namespace CheckersApp
{
    public class PieceSkin
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Color WhitePieceColor { get; set; }
        public Color WhiteBorderColor { get; set; }
        public Color BlackPieceColor { get; set; }
        public Color BlackBorderColor { get; set; }
        public Color KingColor { get; set; }
        public string KingSymbol { get; set; }
        public string PreviewEmoji { get; set; }

        public static readonly PieceSkin Classic = new PieceSkin
        {
            Name = "Классика",
            Description = "Традиционные черные и белые шашки",
            WhitePieceColor = Colors.White,
            WhiteBorderColor = Colors.Gray,
            BlackPieceColor = Colors.Black,
            BlackBorderColor = Colors.DarkSlateGray,
            KingColor = Colors.Gold,
            KingSymbol = "♕",
            PreviewEmoji = "🎯"
        };

        public static readonly PieceSkin Wooden = new PieceSkin
        {
            Name = "Дерево",
            Description = "Натуральное дерево",
            WhitePieceColor = Color.FromRgb(244, 209, 166),
            WhiteBorderColor = Color.FromRgb(139, 90, 43),
            BlackPieceColor = Color.FromRgb(101, 67, 33),
            BlackBorderColor = Color.FromRgb(61, 43, 31),
            KingColor = Color.FromRgb(218, 165, 32),
            KingSymbol = "♛",
            PreviewEmoji = "🌳"
        };

        public static readonly PieceSkin Marble = new PieceSkin
        {
            Name = "Мрамор",
            Description = "Роскошный итальянский мрамор",
            WhitePieceColor = Color.FromRgb(230, 230, 230),
            WhiteBorderColor = Colors.LightGray,
            BlackPieceColor = Color.FromRgb(100, 100, 100),
            BlackBorderColor = Colors.DarkGray,
            KingColor = Colors.Silver,
            KingSymbol = "★",
            PreviewEmoji = "💎"
        };

        public static readonly PieceSkin Neon = new PieceSkin
        {
            Name = "Неон",
            Description = "Яркие неоновые цвета",
            WhitePieceColor = Color.FromRgb(100, 200, 255),
            WhiteBorderColor = Colors.Cyan,
            BlackPieceColor = Color.FromRgb(255, 100, 200),
            BlackBorderColor = Colors.Magenta,
            KingColor = Colors.Yellow,
            KingSymbol = "⚡",
            PreviewEmoji = "🌌"
        };

        public static readonly PieceSkin Gold = new PieceSkin
        {
            Name = "Золото",
            Description = "Роскошные золотые шашки",
            WhitePieceColor = Color.FromRgb(255, 215, 0),
            WhiteBorderColor = Color.FromRgb(218, 165, 32),
            BlackPieceColor = Color.FromRgb(184, 134, 11),
            BlackBorderColor = Color.FromRgb(139, 101, 8),
            KingColor = Color.FromRgb(255, 255, 255),
            KingSymbol = "👑",
            PreviewEmoji = "💰"
        };

        public static readonly PieceSkin Crystal = new PieceSkin
        {
            Name = "Кристаллы",
            Description = "Прозрачные кристаллические шашки",
            WhitePieceColor = Color.FromArgb(180, 255, 255, 255),
            WhiteBorderColor = Color.FromRgb(200, 230, 255),
            BlackPieceColor = Color.FromArgb(180, 0, 0, 0),
            BlackBorderColor = Color.FromRgb(100, 100, 150),
            KingColor = Color.FromArgb(220, 100, 200, 255),
            KingSymbol = "✨",
            PreviewEmoji = "🔮"
        };

        public static readonly PieceSkin Fire = new PieceSkin
        {
            Name = "Огонь",
            Description = "Горящие огненные шашки",
            WhitePieceColor = Color.FromRgb(255, 100, 0),
            WhiteBorderColor = Color.FromRgb(255, 50, 0),
            BlackPieceColor = Color.FromRgb(139, 0, 0),
            BlackBorderColor = Color.FromRgb(100, 0, 0),
            KingColor = Color.FromRgb(255, 255, 0),
            KingSymbol = "🔥",
            PreviewEmoji = "🔥"
        };

        public static readonly PieceSkin Space = new PieceSkin
        {
            Name = "Космос",
            Description = "Темная материя и звезды",
            WhitePieceColor = Color.FromRgb(100, 100, 255),
            WhiteBorderColor = Color.FromRgb(70, 70, 200),
            BlackPieceColor = Color.FromRgb(20, 20, 40),
            BlackBorderColor = Color.FromRgb(10, 10, 20),
            KingColor = Color.FromRgb(255, 255, 100),
            KingSymbol = "🌠",
            PreviewEmoji = "🚀"
        };

        public static List<PieceSkin> GetAllSkins()
        {
            return new List<PieceSkin>
            {
                Classic,
                Wooden,
                Marble,
                Neon,
                Gold,
                Crystal,
                Fire,
                Space
            };
        }
    }

    public static class SkinManager
    {
        private static PieceSkin _currentSkin = PieceSkin.Classic;

        public static PieceSkin CurrentSkin
        {
            get => _currentSkin;
            set
            {
                _currentSkin = value;
                OnSkinChanged?.Invoke();
            }
        }

        public static event System.Action OnSkinChanged;

        public static Color GetPieceColor(Color originalColor)
        {
            if (originalColor == Colors.White)
                return CurrentSkin.WhitePieceColor;
            else if (originalColor == Colors.Black)
                return CurrentSkin.BlackPieceColor;

            return originalColor;
        }

        public static Color GetPieceBorderColor(Color originalColor)
        {
            if (originalColor == Colors.Gray || originalColor == Colors.White)
                return CurrentSkin.WhiteBorderColor;
            else if (originalColor == Colors.DarkSlateGray || originalColor == Colors.Black)
                return CurrentSkin.BlackBorderColor;

            return originalColor;
        }

        public static Color GetKingColor()
        {
            return CurrentSkin.KingColor;
        }

        public static string GetKingSymbol()
        {
            return CurrentSkin.KingSymbol;
        }
    }
}