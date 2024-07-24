using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nn.hid;

public class MPFT_NTD_VBControlSystem : MonoBehaviour
{

    private NpadId npadId = NpadId.Invalid;
    private NpadStyle npadStyle = NpadStyle.Invalid;
    private NpadState npadState = new NpadState();

    private const int vibrationDeviceCountMax = 2;
    private int vibrationDeviceCount = 0;
    private VibrationDeviceHandle[] vibrationDeviceHandles = new VibrationDeviceHandle[vibrationDeviceCountMax];
    private VibrationDeviceInfo[] vibrationDeviceInfos = new VibrationDeviceInfo[vibrationDeviceCountMax];
    private VibrationValue vibrationValue = VibrationValue.Make();

    private byte[] fileA;
    private byte[] fileB;
    private VibrationFileInfo fileInfoA = new VibrationFileInfo();
    private VibrationFileInfo fileInfoB = new VibrationFileInfo();
    private VibrationFileParserContext fileContextA = new VibrationFileParserContext();
    private VibrationFileParserContext fileContextB = new VibrationFileParserContext();
    private int sampleA;
    private int sampleB;
    // Start is called before the first frame update
    void Start()
    {
        Npad.Initialize();

        Npad.SetSupportedStyleSet(
            NpadStyle.Handheld |
            NpadStyle.JoyDual |
            NpadStyle.FullKey);
        NpadId[] npadIds =
            {
            NpadId.Handheld,
            NpadId.No1
        };
        Npad.SetSupportedIdType(npadIds);
    }

    // Update is called once per frame
    void Update()
    {
        if (UpdatePadState())
        {
            vibrationValue.Clear();

            if ((npadState.buttons & NpadButton.A) != 0)
            {
                vibrationValue.amplitudeLow = Random.Range(0f, 1f);
                vibrationValue.amplitudeHigh = 0f;
            }

            if ((npadState.buttons & NpadButton.B) != 0)
            {
                vibrationValue.amplitudeLow = 0f;
                vibrationValue.amplitudeHigh = Random.Range(0f, 1f);
            }

            for (int i = 0; i < vibrationDeviceCount; i++)
            {
                Vibration.SendValue(vibrationDeviceHandles[i], vibrationValue);
            }
        }
    }
    private bool UpdatePadState()
    {
        NpadStyle handheldStyle = Npad.GetStyleSet(NpadId.Handheld);
        NpadState handheldState = npadState;
        if (handheldStyle != NpadStyle.None)
        {
            Npad.GetState(ref handheldState, NpadId.Handheld, handheldStyle);
            if (handheldState.buttons != NpadButton.None)
            {
                if ((npadId != NpadId.Handheld) || (npadStyle != handheldStyle))
                {
                    this.GetVibrationDevice(NpadId.Handheld, handheldStyle);
                }
                npadId = NpadId.Handheld;
                npadStyle = handheldStyle;
                npadState = handheldState;
                return true;
            }
        }

        NpadStyle no1Style = Npad.GetStyleSet(NpadId.No1);
        NpadState no1State = npadState;
        if (no1Style != NpadStyle.None)
        {
            Npad.GetState(ref no1State, NpadId.No1, no1Style);
            if (no1State.buttons != NpadButton.None)
            {
                if ((npadId != NpadId.No1) || (npadStyle != no1Style))
                {
                    this.GetVibrationDevice(NpadId.No1, no1Style);
                }
                npadId = NpadId.No1;
                npadStyle = no1Style;
                npadState = no1State;
                return true;
            }
        }

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
            npadId = NpadId.Invalid;
            npadStyle = NpadStyle.Invalid;
            npadState.Clear();
            return false;
        }
        return true;
    }

    private void GetVibrationDevice(NpadId id, NpadStyle style)
    {
        vibrationValue.Clear();
        for (int i = 0; i < vibrationDeviceCount; i++)
        {
            Vibration.SendValue(vibrationDeviceHandles[i], vibrationValue);
        }

        vibrationDeviceCount = Vibration.GetDeviceHandles(
            vibrationDeviceHandles, vibrationDeviceCountMax, id, style);

        for (int i = 0; i < vibrationDeviceCount; i++)
        {
            Vibration.InitializeDevice(vibrationDeviceHandles[i]);
            Vibration.GetDeviceInfo(ref vibrationDeviceInfos[i], vibrationDeviceHandles[i]);
        }
    }
}
