/*--------------------------------------------------------------------------------*
  Copyright (C)Nintendo All rights reserved.

  These coded instructions, statements, and computer programs contain proprietary
  information of Nintendo and/or its licensed developers and are protected by
  national and international copyright laws. They may not be disclosed to third
  parties or copied or duplicated in any form, in whole or in part, without the
  prior written consent of Nintendo.

  The content herein is highly confidential and should be handled accordingly.
 *--------------------------------------------------------------------------------*/

#if UNITY_SWITCH || UNITY_EDITOR || NN_PLUGIN_ENABLE
using UnityEngine;
using nn.hid;

namespace nns
{
    public class CameraController : MonoBehaviour
    {
        void Start()
        {
            DebugPad.Initialize();
            Npad.Initialize();
            Npad.SetSupportedIdType(npadIds);
            Npad.SetSupportedStyleSet(NpadStyle.FullKey | NpadStyle.Handheld | NpadStyle.JoyDual);
        }

        void Update()
        {
            var moveForward = Vector3.zero;
            var padInput = new PadInput();

            var debugPadState = new DebugPadState();
            DebugPad.GetState(ref debugPadState);
            if ((debugPadState.attributes & DebugPadAttribute.IsConnected) != 0)
            {
                padInput.lx = debugPadState.analogStickL.fx;
                padInput.rx = debugPadState.analogStickR.fx;
                padInput.ly = debugPadState.analogStickL.fy;
                padInput.ry = debugPadState.analogStickR.fy;
                padInput.isLPressed = (debugPadState.buttons & (DebugPadButton.L | DebugPadButton.ZL)) != 0;
                padInput.isRPressed = (debugPadState.buttons & (DebugPadButton.R | DebugPadButton.ZR)) != 0;
            }
            else
            {
                var npadState = new NpadState();
                GetNpadState(ref npadState);
                if ((npadState.attributes & NpadAttribute.IsConnected) != 0)
                {
                    padInput.lx = npadState.analogStickL.fx;
                    padInput.rx = npadState.analogStickR.fx;
                    padInput.ly = npadState.analogStickL.fy;
                    padInput.ry = npadState.analogStickR.fy;
                    padInput.isLPressed = npadState.GetButton(NpadButton.L | NpadButton.ZL);
                    padInput.isRPressed = npadState.GetButton(NpadButton.R | NpadButton.ZR);
                }
            }

            if (padInput.isLPressed)
            {
                moveForward += this.transform.right * padInput.lx + this.transform.up * padInput.ly;
            }
            else
            {
                moveForward += this.transform.right * padInput.lx + this.transform.forward * padInput.ly;
            }
            this.transform.position += moveForward * moveSpeed;

            var rotX = (isXAxisReversed ? -padInput.rx : padInput.rx) * rotSpeed;
            var rotY = (isYAxisReversed ? padInput.ry : -padInput.ry) * rotSpeed;

            if (padInput.isRPressed)
            {
                rotX *= -1;
                var focusPos = this.transform.position + this.transform.forward * this.focusPosDist;
                this.transform.RotateAround(focusPos, Vector3.up, rotX);
                this.transform.RotateAround(focusPos, this.transform.right, rotY);
                var cameraRotRestriction = Vector3.Dot(this.transform.forward, Vector3.up);
                if (cameraRotRestriction > threshold || cameraRotRestriction < -threshold)
                {
                    this.transform.RotateAround(focusPos, this.transform.right, -rotY);
                }
                this.transform.LookAt(focusPos);
            }
            else
            {
                this.transform.RotateAround(this.transform.position, Vector3.up, rotX);
                this.transform.RotateAround(this.transform.position, this.transform.right, rotY);
                var cameraRotRestriction = Vector3.Dot(this.transform.forward, Vector3.up);
                if (cameraRotRestriction > threshold || cameraRotRestriction < -threshold)
                {
                    this.transform.RotateAround(this.transform.position, this.transform.right, -rotY);
                }
            }
        }

        void GetNpadState(ref NpadState state)
        {
            for (var i = 0; i < npadIds.Length; i++)
            {
                var npadStyle = Npad.GetStyleSet(npadIds[i]);
                if (npadStyle != NpadStyle.None)
                {
                    Npad.GetState(ref state, npadIds[i], npadStyle);
                    break;
                }
            }
        }

        [Range(0.1f, 5.0f)]
        public float moveSpeed = 2.0f;
        [Range(0.1f, 5.0f)]
        public float rotSpeed = 2.0f;
        [Range(1f, 50f)]
        public float focusPosDist = 20f;
        public bool isYAxisReversed = false;
        public bool isXAxisReversed = false;

        private struct PadInput
        {
            public float rx { get; set; }
            public float ry { get; set; }
            public float lx { get; set; }
            public float ly { get; set; }
            public bool isRPressed { get; set; }
            public bool isLPressed { get; set; }
        }

        private readonly NpadId[] npadIds = { NpadId.No8, NpadId.No7, NpadId.No6, NpadId.No5, NpadId.No4, NpadId.No3, NpadId.No2, NpadId.No1, NpadId.Handheld };
        private const float threshold = 0.99f;
    }
    #endif
}
