using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace eni.NonDestructiveMMD
{
    [System.Serializable]
    public class MMDToAvatarBlendShape
    {
        public string mmdKey;
        public string avatarKey;

        public MMDToAvatarBlendShape(string mmdKey, string avatarKey)
        {
            this.mmdKey = mmdKey;
            this.avatarKey = avatarKey;
        }
    }

    [AddComponentMenu("Scripts/Non-Destructive MMD")]
    [DisallowMultipleComponent]
    public class NonDestructiveMMD : MonoBehaviour
    {
        public List<MMDToAvatarBlendShape> blendShapeMappings = new List<MMDToAvatarBlendShape>();
    }

    [CustomEditor(typeof(NonDestructiveMMD))]
    public class NonDestructiveMMDEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            NonDestructiveMMD data = (NonDestructiveMMD)target;

            if (GUILayout.Button("Open Editor"))
            {
                NonDestructiveMMDWindow.ShowWindow(data);
            }
        }
    }

    public class NonDestructiveMMDWindow : EditorWindow
    {
        private NonDestructiveMMD _dataSource = null;

        private int _currentMmdKey = -1;
        private Dictionary<int, string> _blendShapeForMmdKey = new Dictionary<int, string>(); // TODO: save back
        private Vector2 leftPaneScroll;
        private Vector2 rightPaneScroll;
        private string[] faceBlendShapes;

        private GUIStyle _defaultStyle;
        private GUIStyle _selectedStyle;
        private GUIStyle _hasValueStyle;
        private GUIStyle _selectedHasValueStyle;

        public static void ShowWindow(NonDestructiveMMD data)
        {
            NonDestructiveMMDWindow window = GetWindow<NonDestructiveMMDWindow>("Non-Destructive MMD");
            window._dataSource = data;
        }

        private void OnGUI()
        {
            _defaultStyle = new GUIStyle(GUI.skin.button);
            _selectedStyle = new GUIStyle(GUI.skin.button);
            _selectedStyle.normal.background = MakeBackgroundTexture(2, 2, new Color(0.5f, 0.5f, 0.5f, 1f));
            _hasValueStyle = new GUIStyle(GUI.skin.button);
            _hasValueStyle.normal.background = MakeBackgroundTexture(2, 2, new Color(0.0f, 0.5f, 1f, 1f));
            _selectedHasValueStyle = new GUIStyle(GUI.skin.button);
            _selectedHasValueStyle.normal.background = MakeBackgroundTexture(2, 2, new Color(0.5f, 0.75f, 1f, 1f));

            if (_dataSource.gameObject != null)
            {
                var smr = _dataSource.gameObject.GetComponent<SkinnedMeshRenderer>();
                if (smr)
                {
                    faceBlendShapes = new string[smr.sharedMesh.blendShapeCount];
                    for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                    {
                        faceBlendShapes[i] = smr.sharedMesh.GetBlendShapeName(i);
                    }
                }
                else
                {
                    faceBlendShapes = null;
                }
            }

            GUILayout.BeginHorizontal();

            DrawLeftPane();
            DrawRightPane();

            GUILayout.EndHorizontal();
        }

        private void DrawLeftPane()
        {
            GUILayout.BeginVertical("box", GUILayout.Width(150), GUILayout.ExpandHeight(true));

            leftPaneScroll = GUILayout.BeginScrollView(leftPaneScroll);

            for (int i = 2; i < MmdBlendShapeNames.Length; i += 3)
            {
                var buttonStyle = _blendShapeForMmdKey.ContainsKey(i) ? _hasValueStyle : _defaultStyle;
                if (i == _currentMmdKey)
                {
                    buttonStyle = _blendShapeForMmdKey.ContainsKey(i) ? _selectedHasValueStyle : _selectedStyle;
                }

                if (GUILayout.Button(MmdBlendShapeNames[i], buttonStyle))
                {
                    _currentMmdKey = i;
                }
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawRightPane()
        {
            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            rightPaneScroll = GUILayout.BeginScrollView(rightPaneScroll);

            if (_currentMmdKey >= 0 && faceBlendShapes != null)
            {
                GUILayout.Label("Select blendshape for " + MmdBlendShapeNames[_currentMmdKey]);

                string selectedBlendShape;
                _blendShapeForMmdKey.TryGetValue(_currentMmdKey, out selectedBlendShape);

                if (GUILayout.Button("None", string.IsNullOrEmpty(selectedBlendShape) ? _selectedStyle : _defaultStyle))
                {
                    Debug.Log("Unselected blendshape");
                    _blendShapeForMmdKey.Remove(_currentMmdKey);
                }

                foreach (var blendShapeName in faceBlendShapes)
                {
                    if (GUILayout.Button(blendShapeName, blendShapeName == selectedBlendShape ? _hasValueStyle : _defaultStyle))
                    {
                        Debug.Log("Selected blendshape: " + blendShapeName);
                        _blendShapeForMmdKey[_currentMmdKey] = blendShapeName;
                    }
                }
            }
            else
            {
                GUILayout.Label("Select a MMD shapekey");
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
        
        private Texture2D MakeBackgroundTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D backgroundTexture = new Texture2D(width, height);
            backgroundTexture.SetPixels(pix);
            backgroundTexture.Apply();
            return backgroundTexture;
        }

        // https://github.com/anatawa12/AvatarOptimizer/blob/083142dc37538d6812f0a5d9a49b4e278c47c164/Editor/AnimatorParsers/AnimatorParser.cs#L600
        private static readonly string[] MmdBlendShapeNames = new [] {
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
