﻿using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

public class IslandArea : MonoBehaviour
{
    //================================================
    //
    // 섬의 정보와 영역을 나타내는 스크립트 입니다.
    // 플레이어가 fullArea범위 내로 섬에 접근하면 고유의 연출을 낼 수 있습니다.
    // 또한, 플레이어가 섬에 가까워짐에 따라 파도를 잦아들게 설정할 수 있습니다.
    //
    //================================================

    [SerializeField] private string islandID;               // 섬 구분 ID
    [SerializeField] private LocalizedString islandName;    // 섬 이름 ( UI )
    [SerializeField] private float areaFadeStart = 50f;     // 섬 진입 구역 넓이
    [SerializeField] private float fullArea = 100f;         // 섬 진입 시작 구역
    [SerializeField] private EventReference sound_Enter;    // 섬 진입 시 사운드

    [FoldoutGroup("EnvoirmentSettings"), SerializeField]
    private bool supressWave = true;                        // true일 시 섬 구역 진입시 파도를 잦아들게함
    [FoldoutGroup("EnvoirmentSettings"), SerializeField]
    private float waveIntensity = 0.1f;                     // 섬 구역 진입시 파도가 얼마나 잦아들지 설정

    private bool playerEnterFlag = false;
    private Transform playerPosition;

#if UNITY_EDITOR
#pragma warning disable CS0414
    [Title("Info")]
    [SerializeField, ReadOnly, LabelText("DistanceFromPlayer")] private float debug_distanceFromPlayer;
#pragma warning restore CS0414
#endif

    private void Start()
    {
        if(PlayerCore.IsInstanceValid)
        {
            playerPosition = PlayerCore.Instance.transform;
            if (GetAreaInterpolation(playerPosition.position) > 0) playerEnterFlag = true;
        }
    }

    private void OnEnable()
    {
        if (PlayerCore.IsInstanceValid)
        {
            playerPosition = PlayerCore.Instance.transform;
            if (GetAreaInterpolation(playerPosition.position) > 0) playerEnterFlag = true;
        }
    }

    private void Update()
    {
        if(playerPosition != null)
        {
            float distanceValue = GetAreaInterpolation(playerPosition.position);
#if UNITY_EDITOR
            debug_distanceFromPlayer = distanceValue;
#endif

            if (playerEnterFlag == false)
            {
                if(Vector3.Distance(playerPosition.position,transform.position) < areaFadeStart)
                {
                    playerEnterFlag = true;
                    OnEnterIslandRegion();
                }
            }
            else
            {
                if (Vector3.Distance(playerPosition.position, transform.position) > areaFadeStart)
                {
                    playerEnterFlag = false;
                }
            }
        
            if(distanceValue > 0)
            {
                if(supressWave)
                {
                    GlobalOceanManager.Instance.IslandregionIntensityFactor = Mathf.Lerp(1.0f,waveIntensity,distanceValue);
                }
            }
        }
    }

    public float GetAreaInterpolation(Vector3 t_postion)
    // 플레이어가 섬에 접근을 시작했을 때 접근이 얼마나 진행되었는지 0.0~1.0값으로 표현합니다.
    {
        if (Vector3.Distance(transform.position, t_postion) > fullArea) return 0;
        else if(Vector3.Distance(transform.position, t_postion) < areaFadeStart) return 1;
        else
        {
            return Mathf.InverseLerp(fullArea, areaFadeStart, Vector3.Distance(transform.position, t_postion));
        }
    }

    private void OnEnterIslandRegion()
    {
        RegionEnter regionEnter = RegionEnter.Instance;
        if (regionEnter != null)
        {
            regionEnter.OnRegionEnter(islandName.GetLocalizedString());
            RuntimeManager.PlayOneShot(sound_Enter);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 1.0f);
        Gizmos.DrawWireSphere(transform.position, areaFadeStart);
        Gizmos.color = new Color(1f, 1f, 0f, 1.0f);
        Gizmos.DrawWireSphere(transform.position, fullArea);
    }
}
