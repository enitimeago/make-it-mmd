using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace enitimeago.NonDestructiveMMD
{
    [Flags]
    public enum MmdBlendShapeCategory
    {
        Uncategorized = 0,
        Mouth = 1,
        Eye = 2,
        Eyebrow = 4
    }

    internal class MmdBlendShapeName
    {
        private string _name;
        private string _xoriuName;
        private string _iyEnglishName;
        private MmdBlendShapeCategory _category;

        public string Name => _name;
        public string XoriuName => _xoriuName;
        public string IyEnglishName => _iyEnglishName;
        public MmdBlendShapeCategory Category => _category;

        public MmdBlendShapeName(string name, string xoriuName, string iyEnglishName, MmdBlendShapeCategory category)
        {
            _name = name;
            _xoriuName = xoriuName;
            _iyEnglishName = iyEnglishName;
            _category = category;
        }
    }

    internal static class MmdBlendShapeNames
    {
        // List derived from the following sources:
        // - Xoriu https://www.deviantart.com/xoriu/art/MMD-Facial-Expressions-Chart-341504917
        // - iY MMD World https://docs.google.com/spreadsheets/d/1mfE8s48pUfjP_rBIPN90_nNkAIBUNcqwIxAdVzPBJ-Q/edit
        // - lilxyzw https://gist.github.com/lilxyzw/80608d9b16bf3458c61dec6b090805c5
        // - AvatarOptimizer https://github.com/anatawa12/AvatarOptimizer/blob/083142dc37538d6812f0a5d9a49b4e278c47c164/Editor/AnimatorParsers/AnimatorParser.cs#L600
        public static readonly ImmutableList<MmdBlendShapeName> All = ImmutableList.Create(
            // Mouth
            new MmdBlendShapeName("あ", "a", "Ah", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("い", "i", "Ch", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("う", "u", "U", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("え", "e", "E", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("お", "o", "Oh", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("にやり", "Niyari", "Grin", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("∧", "Mouse_2", "∧", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("ワ", "Wa", "Wa", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("ω", "Omega", "ω", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("▲", "Mouse_1", "▲", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("口角上げ", "MouseUP", "Mouth Horn Raise", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("口角下げ", "MouseDW", "Mouth Horn Lower", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("口横広げ", "MouseWD", "Mouth Side Widen", MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("にやり２", "Niyari2", null, MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("ん", "n", null, MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("あ２", "a 2", null, MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("□", "□", null, MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("ω□", "ω□", null, MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("にっこり", "Smile", null, MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("ぺろっ", "Pero", null, MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("てへぺろ", "Bero-tehe", null, MmdBlendShapeCategory.Mouth),
            new MmdBlendShapeName("てへぺろ２", "Bero-tehe2", null, MmdBlendShapeCategory.Mouth),
            // Eye
            new MmdBlendShapeName("まばたき", "Blink", "Blink", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("笑い", "Smile", "Blink Happy", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("はぅ", "> <", "Close><", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("瞳小", "EyeSmall", "Pupil", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("ｳｨﾝｸ２右", "Wink-c", "Wink 2 Right", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("ウィンク２", "Wink-b", "Wink 2", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("ウィンク", "Wink", "Wink", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("ウィンク右", "Wink-a", "Wink Right", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("なごみ", "Howawa", "Calm", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("じと目", "Jito-eye", "Stare", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("びっくり", "Ha!!!", "Surprised", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("ｷﾘｯ", "Kiri-eye", "Slant", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("はぁと", "EyeHeart", "Heart", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("星目", "EyeStar", "Star Eye", MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("恐ろしい子！", "EyeFunky", null, MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("はちゅ目", "O O", null, MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("瞳縦潰れ", "EyeSmall-v", null, MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("光下", "EyeUnderli", null, MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("ハイライト消", "EyHi-Off", null, MmdBlendShapeCategory.Eye),
            new MmdBlendShapeName("映り込み消", "EyeRef-off", null, MmdBlendShapeCategory.Eye),
            // Eyebrow
            new MmdBlendShapeName("にこり", "Smily", "Cheerful", MmdBlendShapeCategory.Eyebrow),
            new MmdBlendShapeName("上", "Up", "Upper", MmdBlendShapeCategory.Eyebrow),
            new MmdBlendShapeName("下", "Down", "Lower", MmdBlendShapeCategory.Eyebrow),
            new MmdBlendShapeName("真面目", "Serious", "Serious", MmdBlendShapeCategory.Eyebrow),
            new MmdBlendShapeName("困る", "Trouble", "Sadness", MmdBlendShapeCategory.Eyebrow),
            new MmdBlendShapeName("怒り", "Get angry", "Anger", MmdBlendShapeCategory.Eyebrow),
            new MmdBlendShapeName("前", null, "Front", MmdBlendShapeCategory.Eyebrow),
            // Eye + Eyebrow
            new MmdBlendShapeName("喜び", "Joy", null, MmdBlendShapeCategory.Eye | MmdBlendShapeCategory.Eyebrow),
            new MmdBlendShapeName("わぉ!?", "Wao!?", null, MmdBlendShapeCategory.Eye | MmdBlendShapeCategory.Eyebrow),
            new MmdBlendShapeName("なごみω", "Howawa ω", null, MmdBlendShapeCategory.Eye | MmdBlendShapeCategory.Eyebrow),
            new MmdBlendShapeName("悲しむ", "Wail", null, MmdBlendShapeCategory.Eye | MmdBlendShapeCategory.Eyebrow),
            new MmdBlendShapeName("敵意", "Hostility", null, MmdBlendShapeCategory.Eye | MmdBlendShapeCategory.Eyebrow),
            // Other
            new MmdBlendShapeName("照れ", null, "Blush", MmdBlendShapeCategory.Uncategorized),
            new MmdBlendShapeName("歯無し下", "ToothAnon", null, MmdBlendShapeCategory.Uncategorized),
            new MmdBlendShapeName("歯無し上", "ToothBnon", null, MmdBlendShapeCategory.Uncategorized),
            new MmdBlendShapeName("涙", null, null, MmdBlendShapeCategory.Uncategorized),
            new MmdBlendShapeName("しいたけ", null, null, MmdBlendShapeCategory.Uncategorized)
        );
    }
}
