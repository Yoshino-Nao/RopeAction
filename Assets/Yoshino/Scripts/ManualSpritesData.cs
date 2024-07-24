using System;
using UnityEngine;
[CreateAssetMenu]
public class ManualSpritesData : ScriptableObject
{
    [Serializable]
    public struct ButtonSpritesDualSense
    {
        public Sprite Move;
        public Sprite Jump;
        public Sprite RopeAttach;
        public Sprite RopeControl;
    }
    public ButtonSpritesDualSense m_dualSense;
    [Serializable]
    public struct ButtonSpritesKeyBoardAndMouse
    {
        public Sprite Move;
        public Sprite Jump;
        public Sprite RopeAttach;
        public Sprite RopeControl;
    }
    public ButtonSpritesKeyBoardAndMouse m_keyboardAndMouse;
}
