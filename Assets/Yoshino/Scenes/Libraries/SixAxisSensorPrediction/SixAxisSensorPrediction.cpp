/*--------------------------------------------------------------------------------*
  Copyright (C)Nintendo All rights reserved.

  These coded instructions, statements, and computer programs contain proprietary
  information of Nintendo and/or its licensed developers and are protected by
  national and international copyright laws. They may not be disclosed to third
  parties or copied or duplicated in any form, in whole or in part, without the
  prior written consent of Nintendo.

  The content herein is highly confidential and should be handled accordingly.
 *--------------------------------------------------------------------------------*/

#include <nn/util/util_Matrix.h>
#include <nn/util/util_Vector.h>
#include <nn/util/util_Quaternion.h>
#include <nn/hid.h>
#include <nn/hid/hid_NpadSixAxisSensor.h>
#include <nn/hid/hid_SixAxisSensor.h>
#include <nn/hid/hid_NpadHandheld.h>

namespace {
	enum SixAxisSensorPredictionType
	{
		SixAxisSensorPredictionType_Disabled,
		SixAxisSensorPredictionType_UniformMotion,
	};
}

static nn::util::MatrixRowMajor4x3f GetSensorRotationToMatrix(const nn::util::Quaternion& q)
{
    nn::util::MatrixRowMajor4x3f basis;
    nn::util::MatrixFromQuaternion(&basis, q);
    nn::util::Vector3f tX, tY, tZ, tW;
    basis.Get(&tX, &tY, &tZ, &tW);
    basis.SetAxisX(nn::util::Vector3f(-tX.GetX(), tX.GetZ(), -tX.GetY()));
    basis.SetAxisY(nn::util::Vector3f(-tY.GetX(), tY.GetZ(), -tY.GetY()));
    basis.SetAxisZ(nn::util::Vector3f(tZ.GetX(), -tZ.GetZ(), tZ.GetY()));
    return basis;
}

#define PI (3.14159265358979323846)

static bool g_Initialized;
static nn::hid::SixAxisSensorHandle g_Handle;

static bool Initialize()
{
	if(g_Initialized)
	{
		return true;
	}

	nn::hid::NpadStyleSet styleSet = nn::hid::GetNpadStyleSet(nn::hid::NpadId::Handheld);
	if (!styleSet.IsAnyOn())
	{
		return false;
	}

	int count = nn::hid::GetSixAxisSensorHandles(&g_Handle, 1, nn::hid::NpadId::Handheld, styleSet);
	if(count == 0)
	{
    	return false;
	}

	nn::hid::StartSixAxisSensor(g_Handle);
	nn::hid::EnableSixAxisSensorFusion(g_Handle, true);

	g_Initialized = true;
	return true;
}

static nn::util::Quaternion GetPredictedRotation(nn::util::Quaternion q, nn::util::Vector3f angularVelocity, float timeScaler)
{
	angularVelocity *= (PI * 2.0f) * timeScaler;

    nn::util::MatrixRowMajor4x3f basis;
    nn::util::MatrixFromQuaternion(&basis, q);

    nn::util::Quaternion rotAxis;
    nn::util::QuaternionRotateAxis(&rotAxis, basis.GetAxisX(), angularVelocity.GetX());
    nn::util::QuaternionMultiply(&q, rotAxis, q);
    nn::util::QuaternionRotateAxis(&rotAxis, basis.GetAxisY(), angularVelocity.GetY());
    nn::util::QuaternionMultiply(&q, rotAxis, q);
    nn::util::QuaternionRotateAxis(&rotAxis, basis.GetAxisZ(), angularVelocity.GetZ());
    nn::util::QuaternionMultiply(&q, rotAxis, q);
    return q;
}

extern "C" bool nns_InitializeSixAxisSensorPrediction()
{
	return Initialize();
}

extern "C" bool nns_GetSixAxisSensorStateCandidateInternal(
	nn::util::Quaternion& pOutValue, SixAxisSensorPredictionType predictionType, int timeSpan)
{
	pOutValue = nn::util::Quaternion::Identity();
	if( !Initialize() )
	{
		return false;
	}

	nn::hid::SixAxisSensorState states[1];
	int count = nn::hid::GetSixAxisSensorStates(states, 1, g_Handle);
	if(count == 0)
	{
		return false;
	}

	states[0].GetQuaternion(&pOutValue);

	if(predictionType != SixAxisSensorPredictionType_Disabled)
	{
		nn::util::Vector3f angularVelocity(states[0].angularVelocity);
		pOutValue = GetPredictedRotation(pOutValue, angularVelocity, (float)timeSpan / 1000.0f);
	}

	nn::util::MatrixRowMajor4x3f m = GetSensorRotationToMatrix(pOutValue);
	nn::util::QuaternionFromMatrix(&pOutValue, m);
	return true;
}
