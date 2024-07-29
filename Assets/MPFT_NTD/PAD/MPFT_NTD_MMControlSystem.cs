using UnityEngine;
using nn.hid;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class MPFT_NTD_MMControlSystem : MonoBehaviour
{
    public static MPFT_NTD_MMControlSystem ms_instance;
    private void Awake()
    {
        if (ms_instance == null) 
        {
            ms_instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        if (ms_instance == null)
        {
            ms_instance = this;
        }
    }
    private void OnDisable()
    {
        ms_instance = null;
    }
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
