using UnityEngine;
using nn.hid;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class MPFT_NTD_MMControlSystem : MonoBehaviour
{
    public Text DebugText;
    public string DebugMes;

    /// <summary>
    /// ハンドヘルドプレイパッド
    /// </summary>
    [SerializeField]
    public struct NTD_SGGamePad
    {
        public string ModeName;
        public float L_Analog_X;
        public float L_Analog_Y;
        public float R_Analog_X;
        public float R_Analog_Y;
        public bool Up;
        public bool Down;
        public bool Right;
        public bool Left;
        public bool B;
        public bool X;
        public bool A;
        public bool Y;
        public bool Plus;
        public bool Minus;
        public bool MM_TL;
        public bool MM_TR;
        public bool MM_UTL;
        public bool MM_UTR;
    };
    [SerializeField]
    public NTD_SGGamePad SGGamePad = new NTD_SGGamePad();

    /// <summary>
    /// マルチプレイパッド（お裾分け）
    /// </summary>
    [SerializeField]
    public struct NTD_MMGamePad
    {
        public string ModeName;
        public float MM_Analog_X;
        public float MM_Analog_Y;
        public bool MM_Up_B;
        public bool MM_Down_X;
        public bool MM_Right_A;
        public bool MM_Left_Y;
        public bool MM_Plus_Minus;
        public bool MM_SL;
        public bool MM_SR;
        public bool MM_T;
        public bool MM_UT;
    };
    [SerializeField]
    public NTD_MMGamePad[] MMGamePad = new NTD_MMGamePad[4];

#if false
    // 4 players
    private NpadId[] npadIds ={ NpadId.Handheld, NpadId.No1, NpadId.No2, NpadId.No3, NpadId.No4 };
#else
    // 2 players
    private NpadId[] npadIds = { NpadId.Handheld, NpadId.No1, NpadId.No2 };
#endif

    /// <summary>
    /// パッドステータス格納
    /// </summary>
    public NpadState npadState = new NpadState();

    /// <summary>
    /// コントロール格納
    /// </summary>
    private ControllerSupportArg controllerSupportArg = new ControllerSupportArg();
    /// <summary>
    /// nnリクエスト
    /// </summary>
    private nn.Result result = new nn.Result();



    // Start is called before the first frame update
    void Start()
    {
        ///パッド初期化
        Npad.Initialize();
        ///パッドサポートタイプは、id群
        Npad.SetSupportedIdType(npadIds);
        ///パッドのタイプは【横向き型】
        NpadJoy.SetHoldType(NpadJoyHoldType.Horizontal);

        //パッドサポートタイプ
        Npad.SetSupportedStyleSet(
            NpadStyle.FullKey |     ///スイッチ合体型
            NpadStyle.Handheld |    ///ハンドヘルド
            NpadStyle.JoyDual |     ///別枠合体型コントローラー
            NpadStyle.JoyLeft |     ///お裾分け左パッド
            NpadStyle.JoyRight);    ///お裾分け右パッド

        ///全てのパッドをシングルモードに変更
        foreach (NpadId NP in npadIds)
        {
            NpadJoy.SetAssignmentModeSingle(NP);
        }
    }

    private void Update()
    {
        DebugMes = "Nintendo Switch Pad Control System Tester\n By MasterProjectFenrir [Katsuya.Inoue]\n\n\n";
        PadCheck();
        if (DebugText)
        {
            DebugText.text = DebugMes;
        }

        ///ボタンお裾分けコンフィグ画面
        NpadButton onButtons = 0;
        if ((onButtons & (NpadButton.Plus | NpadButton.Minus)) != 0)
        {
            ShowControllerSupport();
        }

    }
    void PadCheck()
    {
        for (int i = 0; i < npadIds.Length; i++)
        {
            ///パッドＩｄを取得
            NpadId npadId = npadIds[i];

            ///パッドの現在のスタイル（モード）を獲得
            NpadStyle npadStyle = Npad.GetStyleSet(npadId);

            ///パッドの現在のスタイル（モード）が無い場合は、そのパッドは存在していない
            if (npadStyle == NpadStyle.None)
            {
                continue;
            }

            ///パッドの状態を獲得
            Npad.GetState(ref npadState, npadId, npadStyle);

            ///パッドのスタイルによるボタン入力状況チェック
            switch (npadStyle)
            {
                ///左パッド
                case NpadStyle.JoyLeft:
                    JoyNullSet(i);
                    JoyLeftSet(npadState, i);
                    break;
                ///右パッド
                case NpadStyle.JoyRight:
                    JoyNullSet(i);
                    JoyRightSet(npadState, i);
                    break;
                case NpadStyle.FullKey:
                case NpadStyle.JoyDual:
                case NpadStyle.Handheld:
                    JoySGNullSet();
                    JoySingleSet(npadState);
                    break;
            }
        }
    }
    void JoySGNullSet()
    {
        SGGamePad.L_Analog_X = 0.0f;
        SGGamePad.L_Analog_Y = 0.0f;
        SGGamePad.R_Analog_X = 0.0f;
        SGGamePad.R_Analog_Y = 0.0f;

        SGGamePad.Up = false;
        SGGamePad.Down = false;
        SGGamePad.Right = false;
        SGGamePad.Left = false;
        SGGamePad.B = false;
        SGGamePad.X = false;
        SGGamePad.A = false;
        SGGamePad.Y = false;
        SGGamePad.Plus = false;
        SGGamePad.Minus = false;
        SGGamePad.MM_TL = false;
        SGGamePad.MM_TR = false;
        SGGamePad.MM_UTL = false;
        SGGamePad.MM_UTR = false;
    }
    void JoySingleSet(NpadState NS)
    {
        DebugMes += "ModeType:" + "[FullKey/JoyDual/Handheld]" + "\n";
        SGGamePad.L_Analog_X = NS.analogStickL.x;
        SGGamePad.L_Analog_X /= AnalogStickState.Max;
        DebugMes += "L_AnalogX:" + SGGamePad.L_Analog_X.ToString() + ",   ";

        SGGamePad.L_Analog_Y = NS.analogStickL.y;
        SGGamePad.L_Analog_Y /= AnalogStickState.Max;
        DebugMes += "L_AnalogY:" + SGGamePad.L_Analog_Y.ToString() + "\n";

        SGGamePad.R_Analog_X = NS.analogStickR.x;
        SGGamePad.R_Analog_X /= AnalogStickState.Max;
        DebugMes += "R_AnalogX:" + SGGamePad.R_Analog_X.ToString() + ",   ";

        SGGamePad.R_Analog_Y = NS.analogStickR.y;
        SGGamePad.R_Analog_Y /= AnalogStickState.Max;
        DebugMes += "R_AnalogY:" + SGGamePad.R_Analog_Y.ToString() + "\n";

        SGGamePad.Up = NS.GetButton(NpadButton.Up);
        if(SGGamePad.Up)    DebugMes += "[Up],";
        SGGamePad.Down = NS.GetButton(NpadButton.Down);
        if (SGGamePad.Down) DebugMes += "[Down],";
        SGGamePad.Right = NS.GetButton(NpadButton.Right);
        if (SGGamePad.Right) DebugMes += "[Right],";
        SGGamePad.Left = NS.GetButton(NpadButton.Left);
        if (SGGamePad.Left) DebugMes += "[Left],";

        DebugMes += "\n";
        SGGamePad.B = NS.GetButton(NpadButton.B);
        if (SGGamePad.B) DebugMes += "[B],";
        SGGamePad.X = NS.GetButton(NpadButton.X);
        if (SGGamePad.X) DebugMes += "[X],";
        SGGamePad.A = NS.GetButton(NpadButton.A);
        if (SGGamePad.A) DebugMes += "[A],";
        SGGamePad.Y = NS.GetButton(NpadButton.Y);
        if (SGGamePad.Y) DebugMes += "[Y],";

        DebugMes += "\n";
        SGGamePad.Plus = NS.GetButton(NpadButton.Plus);
        if (SGGamePad.Plus) DebugMes += "[+],";
        SGGamePad.Minus = NS.GetButton(NpadButton.Minus);
        if (SGGamePad.Minus) DebugMes += "[-],";
        SGGamePad.MM_TL = NS.GetButton(NpadButton.ZL);
        if (SGGamePad.MM_TL) DebugMes += "[ZL],";
        SGGamePad.MM_TR = NS.GetButton(NpadButton.ZR);
        if (SGGamePad.MM_TR) DebugMes += "[ZR],";
        SGGamePad.MM_UTL = NS.GetButton(NpadButton.L);
        if (SGGamePad.MM_UTL) DebugMes += "[L],";
        SGGamePad.MM_UTR = NS.GetButton(NpadButton.R);
        if (SGGamePad.MM_UTR) DebugMes += "[R],";

        DebugMes += "\n";
    }
    /// <summary>
    /// パッドを初期化
    /// </summary>
    /// <param name="No">　パッド番号</param>
    void JoyNullSet(int No)
    {
        MMGamePad[No].MM_Analog_X = 0.0f;
        MMGamePad[No].MM_Analog_Y = 0.0f;

        MMGamePad[No].MM_Up_B = false;
        MMGamePad[No].MM_Down_X = false;
        MMGamePad[No].MM_Left_Y = false;
        MMGamePad[No].MM_Right_A = false;

        MMGamePad[No].MM_Plus_Minus = false;
        MMGamePad[No].MM_SL = false;
        MMGamePad[No].MM_SR = false;
        MMGamePad[No].MM_T = false;
        MMGamePad[No].MM_UT = false;
    }
    /// <summary>
    /// 左パッドセット
    /// </summary>
    /// <param name="NS">ステータス</param>
    /// <param name="No">パッド番号</param>
    void JoyLeftSet(NpadState NS,int No)
    {
        DebugMes += "ModeType:" + "[Joy-Left]" + "\n";
        MMGamePad[No].MM_Analog_X = 0.0f;
        MMGamePad[No].MM_Analog_Y = 0.0f;

        MMGamePad[No].MM_Analog_X = -NS.analogStickL.y;
        MMGamePad[No].MM_Analog_X /= AnalogStickState.Max;
        DebugMes += "L_AnalogX:" + MMGamePad[No].MM_Analog_X.ToString()+",   ";

        MMGamePad[No].MM_Analog_Y = NS.analogStickL.x;
        MMGamePad[No].MM_Analog_Y /= AnalogStickState.Max;
        DebugMes += "L_AnalogY:" + MMGamePad[No].MM_Analog_X.ToString() + "\n";

        MMGamePad[No].MM_Up_B = NS.GetButton(NpadButton.Up);
        if (MMGamePad[No].MM_Up_B) DebugMes += "[Up],";
        MMGamePad[No].MM_Down_X = NS.GetButton(NpadButton.Down);
        if (MMGamePad[No].MM_Down_X) DebugMes += "[Down],";
        MMGamePad[No].MM_Left_Y = NS.GetButton(NpadButton.Left);
        if (MMGamePad[No].MM_Left_Y) DebugMes += "[left],";
        MMGamePad[No].MM_Right_A = NS.GetButton(NpadButton.Right);
        if (MMGamePad[No].MM_Right_A) DebugMes += "[Right],";

        DebugMes += "\n";

        MMGamePad[No].MM_Plus_Minus = NS.GetButton(NpadButton.Minus);
        if (MMGamePad[No].MM_Plus_Minus) DebugMes += "[-],";
        MMGamePad[No].MM_SL = NS.GetButton(NpadButton.LeftSL);
        if (MMGamePad[No].MM_SL) DebugMes += "[SL],";
        MMGamePad[No].MM_SR = NS.GetButton(NpadButton.LeftSR);
        if (MMGamePad[No].MM_SR) DebugMes += "[SR],";
        MMGamePad[No].MM_T = NS.GetButton(NpadButton.ZL);
        if (MMGamePad[No].MM_T) DebugMes += "[ZL],";
        MMGamePad[No].MM_UT = NS.GetButton(NpadButton.L);
        if (MMGamePad[No].MM_UT) DebugMes += "[L],";

        DebugMes += "\n";
    }
    /// <summary>
    /// 右パッドセット
    /// </summary>
    /// <param name="NS">ステータス</param>
    /// <param name="No">パッド番号</param>
    void JoyRightSet(NpadState NS,int No)
    {
        DebugMes += "ModdType:" + "[Joy-Right]" + "\n";
        MMGamePad[No].MM_Analog_X = 0.0f;
        MMGamePad[No].MM_Analog_Y = 0.0f;

        MMGamePad[No].MM_Analog_X = NS.analogStickR.y;
        MMGamePad[No].MM_Analog_X /= AnalogStickState.Max;
        DebugMes += "R_AnalogX:" + MMGamePad[No].MM_Analog_X.ToString() + ",   ";

        MMGamePad[No].MM_Analog_Y = -NS.analogStickR.x;
        MMGamePad[No].MM_Analog_Y /= AnalogStickState.Max;
        DebugMes += "R_AnalogY:" + MMGamePad[No].MM_Analog_Y.ToString() + "\n";

        MMGamePad[No].MM_Up_B = NS.GetButton(NpadButton.B);
        if (MMGamePad[No].MM_Up_B) DebugMes += "[B],";
        MMGamePad[No].MM_Down_X = NS.GetButton(NpadButton.X);
        if (MMGamePad[No].MM_Down_X) DebugMes += "[X],";
        MMGamePad[No].MM_Left_Y = NS.GetButton(NpadButton.Y);
        if (MMGamePad[No].MM_Left_Y) DebugMes += "[Y],";
        MMGamePad[No].MM_Right_A = NS.GetButton(NpadButton.A);
        if (MMGamePad[No].MM_Right_A) DebugMes += "[A],";

        DebugMes += "\n";

        MMGamePad[No].MM_Plus_Minus = NS.GetButton(NpadButton.Plus);
        if (MMGamePad[No].MM_Plus_Minus) DebugMes += "[+],";
        MMGamePad[No].MM_SL = NS.GetButton(NpadButton.RightSL);
        if (MMGamePad[No].MM_SL) DebugMes += "[RightSL],";
        MMGamePad[No].MM_SR = NS.GetButton(NpadButton.RightSR);
        if (MMGamePad[No].MM_SR) DebugMes += "[SR],";
        MMGamePad[No].MM_T = NS.GetButton(NpadButton.ZR);
        if (MMGamePad[No].MM_T) DebugMes += "[ZR],";
        MMGamePad[No].MM_UT = NS.GetButton(NpadButton.R);
        if (MMGamePad[No].MM_UT) DebugMes += "[R],";

        DebugMes += "\n";
    }

    void ShowControllerSupport()
    {
        controllerSupportArg.SetDefault();
        controllerSupportArg.playerCountMax = (byte)(npadIds.Length - 1);

        controllerSupportArg.enableIdentificationColor = true;
        nn.util.Color4u8 color = new nn.util.Color4u8();
        color.Set(255, 128, 128, 255);
        controllerSupportArg.identificationColor[0] = color;
        color.Set(128, 128, 255, 255);
        controllerSupportArg.identificationColor[1] = color;
        color.Set(128, 255, 128, 255);
        controllerSupportArg.identificationColor[2] = color;
        color.Set(224, 224, 128, 255);
        controllerSupportArg.identificationColor[3] = color;

        controllerSupportArg.enableExplainText = true;
        ControllerSupport.SetExplainText(ref controllerSupportArg, "Red", NpadId.No1);
        ControllerSupport.SetExplainText(ref controllerSupportArg, "Blue", NpadId.No2);
        ControllerSupport.SetExplainText(ref controllerSupportArg, "Green", NpadId.No3);
        ControllerSupport.SetExplainText(ref controllerSupportArg, "Yellow", NpadId.No4);

        Debug.Log(controllerSupportArg);
        result = ControllerSupport.Show(controllerSupportArg);
        if (!result.IsSuccess()) 
        { 
            Debug.Log(result); 
        }
    }

}
