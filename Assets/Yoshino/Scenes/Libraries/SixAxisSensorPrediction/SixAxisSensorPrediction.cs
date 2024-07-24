/*--------------------------------------------------------------------------------*
  Copyright (C)Nintendo All rights reserved.

  These coded instructions, statements, and computer programs contain proprietary
  information of Nintendo and/or its licensed developers and are protected by
  national and international copyright laws. They may not be disclosed to third
  parties or copied or duplicated in any form, in whole or in part, without the
  prior written consent of Nintendo.

  The content herein is highly confidential and should be handled accordingly.
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using System.Runtime.InteropServices;

namespace nns
{
    public static class SixAxisSensorPrediction
    {
        public enum PredictionType
        {
            Disabled,
            UniformMotion,
            Max,
        }
#if !UNITY_SWITCH || UNITY_EDITOR
        public static void Initialize() { }

        public static bool GetSixAxisSensorStateCandidate(ref Quaternion pOutValue, PredictionType predictionType, int timeSpan)
        {
            pOutValue = Quaternion.identity;
            return true;
        }

        public static Quaternion GetRecenterQuaternion()
        {
            return Quaternion.identity;
        }
#else
        [DllImport("__Internal",
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "nns_InitializeSixAxisSensorPrediction")]
        public static extern void Initialize();

        public static bool GetSixAxisSensorStateCandidate(ref Quaternion pOutValue, PredictionType predictionType, int timeSpan)
        {
            return GetSixAxisSensorStateCandidateInternal(ref pOutValue, (int)predictionType, timeSpan);
        }

        [DllImport("__Internal",
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "nns_GetSixAxisSensorStateCandidateInternal")]
        private static extern bool GetSixAxisSensorStateCandidateInternal(ref Quaternion pOutValue, int predictionType, int timeSpan);

        public static Quaternion GetRecenterQuaternion()
        {
            Quaternion recenter = Quaternion.identity;

            GetSixAxisSensorStateCandidate(ref recenter, PredictionType.UniformMotion, 0);

            recenter.x = 0;
            recenter.z = 0;
            recenter.w = -recenter.w;
            recenter.Normalize();
            return recenter;
        }
#endif

    }
}