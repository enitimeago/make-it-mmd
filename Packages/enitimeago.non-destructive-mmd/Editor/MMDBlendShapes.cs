using System.Collections.Generic;

namespace enitimeago.NonDestructiveMMD
{
    internal class MMDBlendShapes
    {
        public static IEnumerable<(string name, int i)> JapaneseNames()
        {
            for (int i = 2; i < Names.Length; i += 3)
            {
                yield return (Names[i], i);
            }
        }

        // https://github.com/anatawa12/AvatarOptimizer/blob/083142dc37538d6812f0a5d9a49b4e278c47c164/Editor/AnimatorParsers/AnimatorParser.cs#L600
        public static readonly string[] Names = new[] {
            // New EN by Yi MMD World
            //  https://docs.google.com/spreadsheets/d/1mfE8s48pUfjP_rBIPN90_nNkAIBUNcqwIxAdVzPBJ-Q/edit?usp=sharing
            // Old EN by Xoriu
            //  https://booth.pm/ja/items/3341221
            //  https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/i/0b7b5e4b-c62e-41f7-8ced-1f3e58c4f5bf/d5nbmvp-5779f5ac-d476-426c-8ee6-2111eff8e76c.png
            // Old EN, New EN, JA,

            // ===== Mouth =====
            "a",            "Ah",               "あ",
            "i",            "Ch",               "い",
            "u",            "U",                "う",
            "e",            "E",                "え",
            "o",            "Oh",               "お",
            "Niyari",       "Grin",             "にやり",
            "Mouse_2",      "∧",                "∧",
            "Wa",           "Wa",               "ワ",
            "Omega",        "ω",                "ω",
            "Mouse_1",      "▲",                "▲",
            "MouseUP",      "Mouth Horn Raise", "口角上げ",
            "MouseDW",      "Mouth Horn Lower", "口角下げ",
            "MouseWD",      "Mouth Side Widen", "口横広げ",
            "n",            null,               "ん",
            "Niyari2",      null,               "にやり２",
            // by Xoriu only
            "a 2",          null,               "あ２",
            "□",            null,               "□",
            "ω□",           null,               "ω□",
            "Smile",        null,               "にっこり",
            "Pero",         null,               "ぺろっ",
            "Bero-tehe",    null,               "てへぺろ",
            "Bero-tehe2",   null,               "てへぺろ２",

            // ===== Eyes =====
            "Blink",        "Blink",            "まばたき",
            "Smile",        "Blink Happy",      "笑い",
            "> <",          "Close><",          "はぅ",
            "EyeSmall",     "Pupil",            "瞳小",
            "Wink-c",       "Wink 2 Right",     "ｳｨﾝｸ２右",
            "Wink-b",       "Wink 2",           "ウィンク２",
            "Wink",         "Wink",             "ウィンク",
            "Wink-a",       "Wink Right",       "ウィンク右",
            "Howawa",       "Calm",             "なごみ",
            "Jito-eye",     "Stare",            "じと目",
            "Ha!!!",        "Surprised",        "びっくり",
            "Kiri-eye",     "Slant",            "ｷﾘｯ",
            "EyeHeart",     "Heart",            "はぁと",
            "EyeStar",      "Star Eye",         "星目",
            "EyeFunky",     null,               "恐ろしい子！",
            // by Xoriu only
            "O O",          null,               "はちゅ目",
            "EyeSmall-v",   null,               "瞳縦潰れ",
            "EyeUnderli",   null,               "光下",
            "EyHi-Off",     null,               "ハイライト消",
            "EyeRef-off",   null,               "映り込み消",

            // ===== Eyebrow =====
            "Smily",        "Cheerful",         "にこり",
            "Up",           "Upper",            "上",
            "Down",         "Lower",            "下",
            "Serious",      "Serious",          "真面目",
            "Trouble",      "Sadness",          "困る",
            "Get angry",    "Anger",            "怒り",
            null,           "Front",            "前",
            
            // ===== Eyes + Eyebrow Feeling =====
            // by Xoriu only
            "Joy",          null,               "喜び",
            "Wao!?",        null,               "わぉ!?",
            "Howawa ω",     null,               "なごみω",
            "Wail",         null,               "悲しむ",
            "Hostility",    null,               "敵意",

            // ===== Other ======
            null,           "Blush",            "照れ",
            "ToothAnon",    null,               "歯無し下",
            "ToothBnon",    null,               "歯無し上",
            null,           null,               "涙",

            // others

            // https://gist.github.com/lilxyzw/80608d9b16bf3458c61dec6b090805c5
            null, null, "しいたけ",

            // https://site.nicovideo.jp/ch/userblomaga_thanks/archive/ar1471249
            null, null, "なぬ！",
            null, null, "はんっ！",
            null, null, "えー",
            null, null, "睨み",
            null, null, "睨む",
            null, null, "白目",
            null, null, "瞳大",
            null, null, "頬染め",
            null, null, "青ざめ",
        };
    }
}
