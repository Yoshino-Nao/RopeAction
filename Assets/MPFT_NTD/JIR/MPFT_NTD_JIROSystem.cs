using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using nn.hid;

public class MPFT_NTD_JIROSystem : MonoBehaviour
{
    // コントローラーを格納するための配列
    public GameObject[] controllers = new GameObject[2];
    // テキスト表示用のUIオブジェクト
    public Text m_Text;
    // メッセージ用の文字列
    public string m_Mes;

    // パッドのID（初期値は無効）
    public NpadId npadId = NpadId.Invalid;
    // パッドのスタイル（初期値は無効）
    public NpadStyle npadStyle = NpadStyle.Invalid;
    // パッドの状態を保持するオブジェクト
    public NpadState npadState = new NpadState();

    // ジャイロセンサの情報を格納するためのリスト
    public List<Quaternion> m_JIROPoint;

    /// <summary>
    /// ６軸センサーハンドル
    /// </summary>
    public SixAxisSensorHandle[] handle = new SixAxisSensorHandle[2];
    /// <summary>
    /// ６軸センサーステート
    /// </summary>
    public SixAxisSensorState state = new SixAxisSensorState();
    /// <summary>
    /// ハンドルカウント
    /// </summary>
    public int handleCount = 0;

    /// <summary>
    /// 4Float
    /// </summary>
    public nn.util.Float4 npadQuaternion = new nn.util.Float4();
    /// <summary>
    /// クォータニオン
    /// </summary>
    public Quaternion quaternion = new Quaternion();

    // 初期化処理
    void Start()
    {
        // Npadの初期化
        Npad.Initialize();
        // サポートするNpadスタイルを設定
        Npad.SetSupportedStyleSet(
            NpadStyle.Handheld |
            NpadStyle.JoyDual |
            NpadStyle.FullKey);
        // サポートするNpadのIDを設定
        NpadId[] npadIds =
            {
            NpadId.Handheld,
            NpadId.No1
            };
        // サポートするNpadのIDタイプを設定
        Npad.SetSupportedIdType(npadIds);
    }

    // フレームごとの更新処理
    void Update()
    {
        // ジャイロセンサの情報を格納するリストをクリア
        m_JIROPoint.Clear();
        // ジャイロシステムを更新
        JIROSystem();

        // テキストオブジェクトが設定されている場合
        if (m_Text)
        {
            // ジャイロセンサの情報がない場合の表示
            if (m_JIROPoint.Count == 0)
            {
                m_Mes = "なし";
            }
            else
            {
                // ジャイロセンサの情報をテキストに表示
                m_Mes = "";
                int conters = 0;
                foreach (Quaternion Q in m_JIROPoint)
                {
                    m_Mes += "\n";
                    m_Mes += conters.ToString();
                    m_Mes += "\nQuaternion.x = ";
                    m_Mes += Q.x.ToString();
                    m_Mes += "\n Quaternion.y = ";
                    m_Mes += Q.y.ToString();
                    m_Mes += "\n Quaternion.z = ";
                    m_Mes += Q.z.ToString();
                    m_Mes += "\n Quaternion.w = ";
                    m_Mes += Q.w.ToString();
                }
            }
            // テキストUIに表示
            m_Text.text = m_Mes;
        }
    }

    // ジャイロシステムの更新処理
    void JIROSystem()
    {
        // NpadのIDをHandheldに設定
        NpadId npadId = NpadId.Handheld;
        // NpadのスタイルをNoneに設定
        NpadStyle npadStyle = NpadStyle.None;
        // 現在のNpadスタイルを取得
        npadStyle = Npad.GetStyleSet(npadId);

        // Handheldスタイルでない場合はNo1を試す
        if (npadStyle != NpadStyle.Handheld)
        {
            npadId = NpadId.No1;
            npadStyle = Npad.GetStyleSet(npadId);
        }

        // パッドの状態を更新
        if (UpdatePadState())
        {
            // ハンドルごとに処理を実行
            for (int i = 0; i < handleCount; i++)
            {
                // ６軸センサの状態を取得
                SixAxisSensor.GetState(ref state, handle[i]);

                // クォータニオンを取得
                state.GetQuaternion(ref npadQuaternion);

                // クォータニオンを設定
                quaternion.Set(npadQuaternion.x, npadQuaternion.z, npadQuaternion.y, -npadQuaternion.w);
                // 取得したクォータニオンをリストに追加
                m_JIROPoint.Add(quaternion);

                // コントローラーの回転を設定
                if (controllers.Length > 0)
                {
                    if (handleCount > 0)
                    {
                        controllers[i].transform.rotation = quaternion;

                        if (handleCount == 1)
                        {
                            controllers[i + 1].transform.rotation = quaternion;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// パットの情報更新
    /// </summary>
    /// <returns>状態が更新されたかどうかを示す真偽値</returns>
    private bool UpdatePadState()
    {
        // Handheldスタイルの取得
        NpadStyle handheldStyle = Npad.GetStyleSet(NpadId.Handheld);
        // Handheldの状態を取得
        NpadState handheldState = npadState;
        if (handheldStyle != NpadStyle.None)
        {
            // Handheldの状態を更新
            Npad.GetState(ref handheldState, NpadId.Handheld, handheldStyle);
            if (handheldState.buttons != NpadButton.None)
            {
                // NpadIdまたはNpadStyleが異なる場合はセンサを再取得
                if ((npadId != NpadId.Handheld) || (npadStyle != handheldStyle))
                {
                    this.GetSixAxisSensor(NpadId.Handheld, handheldStyle);
                }
                // 更新された情報を設定
                npadId = NpadId.Handheld;
                npadStyle = handheldStyle;
                npadState = handheldState;
                return true;
            }
        }

        // No1スタイルの取得
        NpadStyle no1Style = Npad.GetStyleSet(NpadId.No1);
        // No1の状態を取得
        NpadState no1State = npadState;
        if (no1Style != NpadStyle.None)
        {
            // No1の状態を更新
            Npad.GetState(ref no1State, NpadId.No1, no1Style);
            if (no1State.buttons != NpadButton.None)
            {
                // NpadIdまたはNpadStyleが異なる場合はセンサを再取得
                if ((npadId != NpadId.No1) || (npadStyle != no1Style))
                {
                    this.GetSixAxisSensor(NpadId.No1, no1Style);
                }
                // 更新された情報を設定
                npadId = NpadId.No1;
                npadStyle = no1Style;
                npadState = no1State;
                return true;
            }
        }

        // 状態に応じてNpadIdとNpadStyleを設定
        if ((npadId == NpadId.Handheld) && (handheldStyle != NpadStyle.None))
        {
            npadId = NpadId.Handheld;
            npadStyle = handheldStyle;
            npadState = handheldState;
        }
        else if ((npadId == NpadId.No1) && (no1Style != NpadStyle.None))
        {
            npadId = NpadId.No1;
            npadStyle = no1Style;
            npadState = no1State;
        }
        else
        {
            // 無効な状態に設定
            npadId = NpadId.Invalid;
            npadStyle = NpadStyle.Invalid;
            npadState.Clear();
            return false;
        }
        return true;
    }

    // ６軸センサを取得する処理
    private void GetSixAxisSensor(NpadId id, NpadStyle style)
    {
        // すべてのセンサを停止
        for (int i = 0; i < handleCount; i++)
        {
            SixAxisSensor.Stop(handle[i]);
        }

        // ハンドルカウントを取得
        handleCount = SixAxisSensor.GetHandles(handle, 2, id, style);

        // すべてのセンサを開始
        for (int i = 0; i < handleCount; i++)
        {
            SixAxisSensor.Start(handle[i]);
        }
    }
}