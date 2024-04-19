/*--------------------------------------------------------------------------------*
  Copyright (C)Nintendo All rights reserved.

これらのコード化された命令、ステートメント、およびコンピュータプログラムには、独自の
任天堂および/またはそのライセンスを受けた開発者の情報であり、以下によって保護されています
国内および国際著作権法。第三者に開示することはできません
当事者またはコピーまたは複製、全体または一部、任天堂の書面による事前の同意。

ここに記載されている内容は機密性が高く、それに応じて取り扱われる必要があります。
 *--------------------------------------------------------------------------------*/

// Unityエンジン用の基本ライブラリをインポート
using UnityEngine;
// Nintendo Switchのハードウェア入力を扱うためのライブラリをインポート
using nn.hid;

public class HidMultiPlayer : MonoBehaviour
{
    // UIのテキスト表示を管理するコンポーネント
    private UnityEngine.UI.Text m_TextComponent;
    // 文字列の組み立てを効率的に行うためのStringBuilderオブジェクト
    private System.Text.StringBuilder m_StringBuilder 
        = new System.Text.StringBuilder();

#if true
    // 4人プレイの設定
    private NpadId[] m_NpadIds ={ 
        NpadId.Handheld,    //ハンドヘルド
        NpadId.No1,         //ジョイコン1
        NpadId.No2,         //ジョイコン2
        NpadId.No3,         //ジョイコン3
        NpadId.No4 };       //ジョイコン4
#else
    // 2人プレイの設定
    private NpadId[] m_NpadIds = { 
        NpadId.Handheld,    //ハンドヘルド
        NpadId.No1,         //ジョイコン1
        NpadId.No2 };       //ジョイコン2
#endif

    // コントローラの状態を保持するオブジェクト
    private NpadState npadState = new NpadState();
    // 以前のボタン入力を記憶するための配列
    private long[] preButtons;
    // コントローラサポートの設定を管理するオブジェクト
    private ControllerSupportArg controllerSupportArg 
        = new ControllerSupportArg();
    // nnライブラリの結果
    private nn.Result Result = new nn.Result();

    void Start()
    {
        // UIのテキストコンポーネントを取得。
        m_TextComponent = 
            GameObject.Find("/Canvas/Text").GetComponent<UnityEngine.UI.Text>();

        // コントローラーの初期化。
        Npad.Initialize();
        // 使用するコントローラーIDの設定。
        Npad.SetSupportedIdType(m_NpadIds);
        // コントローラーのホールドタイプを水平に設定。
        NpadJoy.SetHoldType(NpadJoyHoldType.Horizontal);

#if true
        // 横持ちのシングルJoy-Con設定。
        Npad.SetSupportedStyleSet(
            NpadStyle.FullKey |         //全ジョイコンを合体状態でのFullキーモード
            NpadStyle.Handheld |        //ハンドヘルド形態のモード
            NpadStyle.JoyDual |         //左右それぞれにジョイコンを握っているモード
            NpadStyle.JoyLeft |         //左ジョイコンのみ使用しているモード
            NpadStyle.JoyRight);        //右ジョイコンのみ使用しているモード

        // 全てのジョイコンをシングルモードで設定。
        for (int i = 1; i < m_NpadIds.Length; i++)
            NpadJoy.SetAssignmentModeSingle(m_NpadIds[i]);

#else
        // 全ジョイコンを合体状態での使用設定。
        Npad.SetSupportedStyleSet(
            NpadStyle.FullKey |         //全ジョイコンを合体状態でのFullキーモード
            NpadStyle.Handheld |        //ハンドヘルド形態のモード
            NpadStyle.JoyDual);         //左右それぞれにジョイコンを握っているモード
#endif
        // コントローラーのボタン状態を格納する配列の初期化。
        preButtons = new long[m_NpadIds.Length];
    }

    void Update()
    {
        // StringBuilderの内容をクリアして、新たなフレームの情報を記述する準備。
        m_StringBuilder.Length = 0;
        m_StringBuilder.Append(" +, -: コントローラサポートアプレット\n");
        // StringBuilderの内容をクリアして、新たなフレームの情報を記述する準備。
        NpadButton onButtons = 0;

        // 各コントローラーの状態をチェック。
        for (int i = 0; i < m_NpadIds.Length; i++)
        {
            // 現在のコントローラーのIDを取得。
            NpadId npadId = m_NpadIds[i];
            // 現在のコントローラースタイルを取得。
            NpadStyle npadStyle = Npad.GetStyleSet(npadId);
            // スタイルがNoneの場合、次のループへスキップ（このコントローラーは接続されていない）
            if (npadStyle == NpadStyle.None) { continue; }
            // コントローラの現在の状態を取得。
            Npad.GetState(ref npadState, npadId, npadStyle);
            // 特定のボタンが押されているかどうかをチェックするフラグ。
            bool isPushedEnterButton = false;
            // 左ジョイコンでのDownボタン、右ジョイコンでのXボタン、それ以外のスタイルでのAボタンが押されたか。
            if (npadStyle == NpadStyle.JoyLeft)
            {
                isPushedEnterButton = (npadState.buttons & NpadButton.Down) != 0;
            }
            else if (npadStyle == NpadStyle.JoyRight)
            {
                isPushedEnterButton = (npadState.buttons & NpadButton.X) != 0;
            }
            else if ((npadStyle == NpadStyle.FullKey) || (npadStyle == NpadStyle.JoyDual) || (npadStyle == NpadStyle.Handheld))
            {
                isPushedEnterButton = (npadState.buttons & NpadButton.A) != 0;
            }
            // 押されたボタンに基づいて、テキストの色を変更して表示。
            if (isPushedEnterButton)
            {
                m_StringBuilder.AppendFormat("<color=#ff8888ff>{0} {1} {2}</color>\n", npadId, npadStyle, npadState);
            }
            else
            {
                m_StringBuilder.AppendFormat("{0} {1} {2}\n", npadId, npadStyle, npadState);
            }
            // 新たに押されたボタンがあればその情報を記録。
            onButtons |= ((NpadButton)preButtons[i] ^ npadState.buttons) & npadState.buttons;
            // 現在のボタン状態を記録して次のフレームで比較に使用。
            preButtons[i] = (long)npadState.buttons;
        }
        // Plus または Minus ボタンが新たに押された場合、コントローラサポート画面を表示。
        if ((onButtons & (NpadButton.Plus | NpadButton.Minus)) != 0)
        {
            ShowControllerSupport();
        }
        // StringBuilderに保存されたテキストをUIのテキストコンポーネントに設定。
        m_StringBuilder.AppendFormat("Result: {0}\n", Result);

        m_TextComponent.text = m_StringBuilder.ToString();
    }

    // コントローラーサポートの詳細設定を表示するメソッド。
    void ShowControllerSupport()
    {
        // ControllerSupportArgのデフォルト設定を行います。
        controllerSupportArg.SetDefault();
        // サポートするプレイヤー数の最大値を設定します（npadIdsの要素数から1を引いた値
        controllerSupportArg.playerCountMax = (byte)(m_NpadIds.Length - 1);
        // コントローラーの識別色を有効にします。
        controllerSupportArg.enableIdentificationColor = true;
        // 各プレイヤーの識別色を設定します。これにより、ユーザーは自分のコントローラーを視覚的に識別できます。
        nn.util.Color4u8 color = new nn.util.Color4u8();
        color.Set(255, 128, 128, 255);
        controllerSupportArg.identificationColor[0] = color;
        color.Set(128, 128, 255, 255);
        controllerSupportArg.identificationColor[1] = color;
        color.Set(128, 255, 128, 255);
        controllerSupportArg.identificationColor[2] = color;
        color.Set(224, 224, 128, 255);
        controllerSupportArg.identificationColor[3] = color;
        // コントローラーに関する説明テキストを有効にします。
        controllerSupportArg.enableExplainText = true;
        // 各コントローラーの説明テキスト（カラーラベル）を設定します。
        ControllerSupport.SetExplainText(ref controllerSupportArg, "Red", NpadId.No1);
        ControllerSupport.SetExplainText(ref controllerSupportArg, "Blue", NpadId.No2);
        ControllerSupport.SetExplainText(ref controllerSupportArg, "Green", NpadId.No3);
        ControllerSupport.SetExplainText(ref controllerSupportArg, "Yellow", NpadId.No4);
        // コントローラーサポート画面を表示する前に設定された引数をログに出力します。
        Debug.Log(controllerSupportArg);
        // コントローラーサポート画面を表示します。結果はResultに保存されます。
        Result = ControllerSupport.Show(controllerSupportArg);
        // もし結果が失敗を示していた場合は、ログにエラーを出力します。
        if (!Result.IsSuccess()) { Debug.Log(Result); }
    }
}
