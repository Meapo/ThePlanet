﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionControl : MonoBehaviour
{
    [Tooltip("退潮需要的时间")]
    public float ebbTime;
    private float nowEbbTime;
    [Tooltip("开采陨石需要时间")]
    public float mineTime;
    public float nowMineTime;
    [Tooltip("异常加速度")]
    public float abnormalAc;
    [Tooltip("温度颜色变化提示温度")]
    public float warningTemperature = 10f;
    [Tooltip("异常温度值")]
    public float abnormalTemperature;
    [Tooltip("异常温度值改变状态需要的累计时间")]
    public float changeTime;
    [Tooltip("最高温度")]
    public float maxTemperature;
    // 高温累计时间
    float temperatureToohighTime;
    // 低温累计时间
    float temperatureToolowTime;
    [Tooltip("温度改变速率")]
    public float temperatureUpRatio;
    [Tooltip("初始温度")]
    public float initTemperature;
    public float temperature;
    //人口可以增长的两个临界值
    public float polMinTemp;
    public float polMaxTemp;
    public Region region;
    public float decreasePolF;
    public int decreasePol;
    public float polF;
    public int pol;
    public GameObject alternator;
    SpriteRenderer sprite;
    Earth earth;
    RotateControl rotateControlInstance;
    RegionSprite regionSprite;
    public GameObject IndustryAAG;
    public GameObject InformationAAG;
    public GameObject AtomicAAG;
    public GameObject nowAAG;
    public int nowCityLevel;

    public GameObject particle;
    // Start is called before the first frame update
    void Start()
    {
        nowCityLevel = -1;
        sprite = GetComponentInChildren<SpriteRenderer>();
        temperature = initTemperature;
        earth = Earth.earth;
        temperatureToohighTime = 0f;
        regionSprite = RegionSprite.regionSprite;
        rotateControlInstance = RotateControl.rotateControlInstance;
        nowEbbTime = 0f;
        LoadImage();
        if(GetComponentInChildren<ParticleSystem>(true) != null)
        {
            particle = GetComponentInChildren<ParticleSystem>(true).gameObject;
        }
        
    }

    void LoadImage()
    {
        switch (region)
        {
            case Region.Desert:
                {
                    sprite.sprite = regionSprite.desert;
                    break;
                }
            case Region.FlatGround:
                {
                    sprite.sprite = regionSprite.flatground;
                    break;
                }
            case Region.Forest:
                {
                    sprite.sprite = regionSprite.forest;
                    break;
                }
            case Region.Sea:
                {
                    sprite.sprite = regionSprite.sea;
                    break;
                }
            case Region.SeaGround:
                {
                    sprite.sprite = regionSprite.seaGround;
                    break;
                }
            case Region.ironGround:
                {
                    sprite.sprite = regionSprite.ironGround;
                    break;
                }
            case Region.Motor:
                {
                    sprite.sprite = regionSprite.motor;
                    break;
                }
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (!rotateControlInstance.startGame)
        {
            return;
        }
        if (earth.era < Era.AtomicEra)
        {
            if (region == Region.City && isReachDestoryTemp())
            {
                if (nowAAG != null)
                {
                    nowAAG.SetActive(false);
                    nowAAG = null;
                }
                changeRegionTo(Region.Desert);
            }
        }
        if (isUnderSunshine())
        {
            temperature += temperatureUpRatio * Time.fixedDeltaTime;
        }
        else
        {
            temperature -= temperatureUpRatio * Time.fixedDeltaTime;
        }
        temperature = Mathf.Clamp(temperature, -maxTemperature, maxTemperature);

        // 计算异常时间累计值
        if (temperature > abnormalTemperature)
        {
            temperatureToohighTime += Time.fixedDeltaTime;
        }
        else if (temperature < -abnormalTemperature)
        {
            temperatureToolowTime += Time.fixedDeltaTime;
        }
        else
        {
            temperatureToohighTime -= Time.fixedDeltaTime;
            temperatureToohighTime = Mathf.Clamp(temperatureToohighTime, 0f, float.MaxValue);
            temperatureToolowTime -= Time.fixedDeltaTime;
            temperatureToolowTime = Mathf.Clamp(temperatureToolowTime, 0f, float.MaxValue);
        }
        if(earth.era < Era.AtomicEra)
        {
            // 温度过高一律变成沙漠
            if (temperatureToohighTime >= changeTime)
            {
                if (nowAAG != null)
                {
                    nowAAG.SetActive(false);
                    nowAAG = null;
                }
                changeRegionTo(Region.Desert);
            }
            // 温度过低，海洋和Sea Ground不变沙漠
            if (temperatureToolowTime >= changeTime && region != Region.Sea && region != Region.SeaGround)
            {
                if (nowAAG != null)
                {
                    nowAAG.SetActive(false);
                    nowAAG = null;
                }
                
                
                changeRegionTo(Region.Desert);
            }
        }

        
        if(earth.era<Era.AtomicEra)
        {
            AddTip();
            // 海洋淹没
            Flood();
        }

        // Sea Ground退潮
        Ebb();

        //ironGround 开采陨石建设风力发电机
        Mine();
    }
    void AddTip()
    {
        if(temperature>10)
        {
            EventTip.eventTip.AddTips(global::Tip.HeatWave);
        }
        else if(temperature<-10)
        {
            EventTip.eventTip.AddTips(global::Tip.ClodWave);
        }
    }
    public bool isUnderSunshine()
    {
        if (transform.rotation.eulerAngles.z >= 45f && transform.rotation.eulerAngles.z < 225f)
        {
            return true;
        }
        return false;
    }
    public bool isOverNormalTemp()
    {
        if(temperature>10 || temperature<-10)
        {
            return true;
        }
        return false;
    }
    public bool isReachDestoryTemp()
    {
        if(temperature>15 || temperature<-15)
        {
            return true;
        }
        return false;
    }


    public void changeRegionTo(Region region)
    {
        if(region!=Region.Motor)
        {
            if (this.region == Region.City || this.region == Region.SeaCity)
            {
                if (region != Region.City && region != Region.SeaCity)
                {
                    EventTip.eventTip.AddTips(Tip.CityDestory);
                }
            }
            if (this.region == Region.City)
            {
                if (region != Region.SeaCity)
                {
                    pol = 0;
                    polF = 0;
                    decreasePol = 0;
                    decreasePolF = 0;
                }
                else
                {
                    polF = 0.7f * pol;
                    // 人口减少
                    pol = (int)polF;
                }
            }
            this.region = region;
        }
        else
        {
            this.region = Region.Motor;
            sprite.sortingOrder = -10;
            sprite.gameObject.transform.localPosition = new Vector3(-0.05f, -0.6f, 0);
            Animator animator = GetComponentInChildren<Animator>();
            Debug.Log(animator);
            if(animator!=null)
            {
                animator.enabled = true;
            }

        }
        LoadImage();
    }
    
    private void Flood()
    {
        if (region == Region.Sea && Mathf.Abs(rotateControlInstance.earthAc) > abnormalAc)
        {
            if(rotateControlInstance.earthAc * rotateControlInstance.earthS> 0)
            {
                EventTip.eventTip.AddTips(Tip.QuickAcc);
            }
            else if(rotateControlInstance.earthAc*rotateControlInstance.earthS<0)
            {
                EventTip.eventTip.AddTips(Tip.QuickPause);
            }
            RegionControl[] regionControls = transform.parent.GetComponentsInChildren<RegionControl>();
            int ind = transform.GetSiblingIndex();
            int targetInd;
            if (rotateControlInstance.earthAc > 0)
            {
                if (ind == 0)
                {
                    targetInd = regionControls.Length - 1;
                }
                else
                {
                    targetInd = ind - 1;
                }
            }
            else
            {
                if (ind == regionControls.Length - 1)
                {
                    targetInd = 0;
                }
                else
                {
                    targetInd = ind + 1;
                }
            }
            Flood(targetInd);
        }
    }
    public void FloodAround()
    {
        RegionControl[] regionControls = transform.parent.GetComponentsInChildren<RegionControl>();
        int ind = transform.GetSiblingIndex();
        int targetInd;
        if (ind == 0)
        {
            targetInd = 9;
        }
        else
        {
            targetInd = ind - 1;
        }
        Flood(targetInd);

        if (ind == 9)
        {
            targetInd = 0;
        }
        else
        {
            targetInd = ind + 1;
        }
        Flood(targetInd);
        // 变SeaGround
        
    }

    public void Flood(int targetInd)
    {
        EventTip.eventTip.AddTips(Tip.Tsunami);
        RegionControl[] regionControls = transform.parent.GetComponentsInChildren<RegionControl>();
        if(regionControls[targetInd].region!=Region.Sea)
        {
            if (regionControls[targetInd].region == Region.City ||
            regionControls[targetInd].region == Region.SeaCity)
            {
                if(earth.era<Era.InformationEra)
                {
                    regionControls[targetInd].changeRegionTo(Region.SeaCity);
                    regionControls[targetInd].nowEbbTime = ebbTime;
                }
            }
            else{
            regionControls[targetInd].changeRegionTo(Region.SeaGround);
            regionControls[targetInd].nowEbbTime = ebbTime;
            }
        }
    }
    
    private void Ebb()
    {
        if (region == Region.SeaGround)
        {
            nowEbbTime -= Time.fixedDeltaTime;
            if (nowEbbTime <= 0f)
            {
                changeRegionTo(Region.FlatGround);
            }
        }
        else if (region == Region.SeaCity)
        {
            nowEbbTime -= Time.fixedDeltaTime;
            if (nowEbbTime <= 0f)
            {
                changeRegionTo(Region.City);
            }
        }
    }
    private void Mine()
    {
        if(earth.era>=Era.IndutrialEra)
        {
            if (region == Region.ironGround)
            { 
                nowMineTime -= Time.fixedDeltaTime;
                if (nowMineTime <= 0f)
                {
                    nowMineTime -= Time.fixedDeltaTime;
                    if (nowMineTime <= 0f)
                    {
                        changeRegionTo(Region.FlatGround);
                        alternator.SetActive(true);
                    }
                }
            }
        }
        
    }
    public void SetAlternatorActive(bool active)
    {

        alternator.SetActive(active);
    }
}
