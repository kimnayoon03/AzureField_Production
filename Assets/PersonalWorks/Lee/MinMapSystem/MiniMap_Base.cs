using AmplifyShaderEditor;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MiniMapMod
{
    Mini,
    Fullscreen
}

public class MiniMap_Base : MonoBehaviour
{
    public static MiniMap_Base Instance;
    [SerializeField] Vector2 worldsize;
    [SerializeField] 
    Vector2 fullScrennDimensions = new Vector2(1920,1080);
    [SerializeField] float zoomSpeed = 0.1f;
    [SerializeField] float maxZoom= 10f;
    [SerializeField] float minZoom = 1f;

    [SerializeField]
    RectTransform scrollViewRectTransform;
    [SerializeField]
    RectTransform contentRectTransform;

    [SerializeField] private CanvasGroup MinimapObj;

    [SerializeField] MiniMap_Icon miniMapPrefab;

    [SerializeField] GameObject Player;
    Matrix4x4 transformationMatix;

    private MiniMapMod currentMiniMapMode =  MiniMapMod.Mini;
    private MiniMap_Icon followIcon;
    private Vector2 scrollViewDefaultSize;
    private Vector2 scrollViewDefaultPosition;

   private bool setMap;

    Dictionary<MiniMap_Object,MiniMap_Icon> miniMapWorldObjectLookup =
    new Dictionary<MiniMap_Object, MiniMap_Icon>();

    private void Awake()
    {
        Instance = this;
        scrollViewDefaultSize = scrollViewRectTransform.sizeDelta;
        scrollViewDefaultPosition = scrollViewRectTransform.anchoredPosition;

    }

    private void Start() 
    {
        CalculateTransformationMatrix();
    }

    private void Update()
    {
        if(Keyboard.current[Key.M].wasPressedThisFrame)
        {
            SetMinimapMode(currentMiniMapMode == MiniMapMod.Mini ? MiniMapMod.Fullscreen : MiniMapMod.Mini);
            // if(setMap)
            // {
            //     SetObj();
            // }
            // else
            // {
            //     DeltObj();
            // };
        }
 

       float zoom = Mouse.current.scroll.ReadValue().y;
       ZoomMap(zoom);
       UpdateMiniMapIcons();
       CenterMapOnIcon();
    }

    private void SetObj()
    {
        MinimapObj.gameObject.SetActive(true);
    }

    private void DeltObj()
    {
        MinimapObj.gameObject.SetActive(false);
    }

    public void RegisterMinimapWorldObject(MiniMap_Object miniMap_Object, bool followObject = false)
    {
        var minimapIcon = Instantiate(miniMapPrefab);
        minimapIcon.transform.SetParent(contentRectTransform);
        minimapIcon.transform.SetParent(contentRectTransform);
        minimapIcon.Image.sprite = miniMap_Object.MiniMapIcon;
        miniMapWorldObjectLookup[miniMap_Object] = minimapIcon;

        if(followObject)
        {
            followIcon = minimapIcon;
        }

    }

    public void RemoveMiniMapWolrdObject(MiniMap_Object minimapobject)
    {
        if(miniMapWorldObjectLookup.TryGetValue(minimapobject,out MiniMap_Icon icon))
        {
            miniMapWorldObjectLookup.Remove(minimapobject);
            Destroy(icon.gameObject);
        }
    }


    // 미니맵 크기 설정
    private Vector2 halfVector2 = new Vector2(0.5f,0.5f);
    public void SetMinimapMode(MiniMapMod mod)
    {
        const float defaultScaleWhenFullScreen = 1.3f;

        if(mod == currentMiniMapMode)
        return;

        switch (mod)
        {
            case MiniMapMod.Mini:
            scrollViewRectTransform.sizeDelta = scrollViewDefaultSize;
            scrollViewRectTransform.anchorMin = Vector2.one;
            scrollViewRectTransform.anchorMax = Vector2.one;
            scrollViewRectTransform.pivot = Vector2.one;
            scrollViewRectTransform.anchoredPosition =scrollViewDefaultPosition;
            currentMiniMapMode = MiniMapMod.Mini;
            break;

            case MiniMapMod.Fullscreen:
            scrollViewRectTransform.sizeDelta = fullScrennDimensions;
            scrollViewRectTransform.anchorMin = halfVector2;
            scrollViewRectTransform.anchorMax = halfVector2;
            scrollViewRectTransform.pivot = halfVector2;
            scrollViewRectTransform.anchoredPosition = Vector2.zero;
            currentMiniMapMode = MiniMapMod.Fullscreen;
            contentRectTransform.transform.localScale = Vector3.one *
            defaultScaleWhenFullScreen;           
            break;   
        }
    }

    private void ZoomMap(float zoom)
    {
        if(zoom == 0)
        return;

        float currentMapScale = contentRectTransform.localScale.x;
        float zoomAmount = (zoom > 0 ? zoomSpeed : -zoomSpeed) * currentMapScale;
        float newScale = currentMapScale + zoomAmount;
        float clampedScale = Mathf.Clamp(newScale, minZoom, maxZoom);
        contentRectTransform.localScale = Vector3.one * clampedScale;
    }

    private void CenterMapOnIcon()
    {
        if(followIcon != null)
        {
            float mapScale = contentRectTransform.transform.localScale.x;

            contentRectTransform.anchoredPosition = (followIcon.rectTransform.anchoredPosition * mapScale);

        }
    }

    private void UpdateMiniMapIcons()
    {
        foreach (var kvp in miniMapWorldObjectLookup)
    {
        var miniMap_Object = kvp.Key;
        var miniMapIcon = kvp.Value;
        
        // 세계 좌표를 미니맵 좌표로 변환
        var mapPosition = WorldPositionToMapPosition(miniMap_Object.transform.position);

        // 미니맵 아이콘 위치 업데이트
        miniMapIcon.rectTransform.anchoredPosition = mapPosition;

        // 미니맵 아이콘 회전 업데이트 (선택 사항)
        var rotation = miniMap_Object.transform.rotation.eulerAngles;
        miniMapIcon.iconRectTrans.localRotation = Quaternion.AngleAxis(-rotation.y, Vector3.forward);
    }


    }

    private Vector2 WorldPositionToMapPosition(Vector3 worldPos)
    {
         // 세계 좌표를 미니맵의 로컬 좌표로 변환
        var localPos = worldPos - Player.transform.position;

        // 세계 크기와 미니맵 크기 사이의 비율 계산
        var ratioX = localPos.x / worldsize.x;
        var ratioY = localPos.z / worldsize.y;

        // 미니맵의 크기를 기반으로 로컬 좌표를 변환하여 미니맵 좌표로 반환
        var mapPos = new Vector2(ratioX * scrollViewRectTransform.rect.width, ratioY * scrollViewRectTransform.rect.height);

        return mapPos;
       
    }

    private void CalculateTransformationMatrix()
    {
        var minimapSize = contentRectTransform.rect.size;
        var worldSize = new Vector2(this.worldsize.x, this.worldsize.y);

        var translation = -minimapSize / 2;
        var scaleRatio = minimapSize / worldSize;

        transformationMatix = Matrix4x4.TRS(translation, Quaternion.identity, scaleRatio);
    }

}
