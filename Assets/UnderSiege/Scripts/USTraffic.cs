﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILranch
{
    //IL.ranch, 2021. ILonion32@gmail.com
    public class USTraffic : MonoBehaviour
    {
        [Header("maximum vehicle in scene:")]
        public int MaxVehicleCount = 50;
        [Header("spawn frequency (min time):")]
        public float MinSpawnInterval = 0.3f;
        [Header("spawn frequency (max time):")]
        public float MaxSpawnInterval = 1.3f;
        [Header("start position scatter:")]
        public float MaxStartScatter = 3.0f;
        [Header("vehicle variety array:")]
        public GameObject[] Vehicle;
        [Header("speed variety:")]
        public float CarLineSpeedVarMin = -2f;
        public float CarLineSpeedVarMax = 5f;

        [HideInInspector]
        public int VehicleCount;
        [HideInInspector]
        public Transform[] SpawnPoints;

        bool locked;
        float[] CarSpeedVariations;
        GameObject VehicleContainer;

        //collect all spawn points (must be children)
        void Awake()
        {
            if (transform.childCount != 0)
            {
                SpawnPoints = new Transform[transform.childCount];
            }
            else
            {
                Debug.Log(" <color=yellow> No spawn points! </color>");
                locked = true;
                return;
            }

            for (int k = 0; k < transform.childCount; k++)
            {
                SpawnPoints[k] = transform.GetChild(k);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks * 1000);
            StartCoroutine(SpawnInterval(Random.Range(MinSpawnInterval, MaxSpawnInterval)));
            VehicleContainer = new GameObject { };
            VehicleContainer.name = "VehicleContainer";

            if (!locked)
            {
                CarSpeedVariations = new float[SpawnPoints.Length];
                for (int k0 = 0; k0 < CarSpeedVariations.Length; k0++)
                {
                    CarSpeedVariations[k0] = Random.Range(CarLineSpeedVarMin, CarLineSpeedVarMax);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log(VehicleCount);
        }

        IEnumerator SpawnInterval(float rTime)
        {
            yield return new WaitForSeconds(rTime);

            if (!locked)
            {
                if (VehicleCount < MaxVehicleCount)
                {
                    for (int k0 = 0; k0 < SpawnPoints.Length; k0++)
                    {
                        GameObject obj = Instantiate(Vehicle[Random.Range(0, Vehicle.Length)]);

                        obj.transform.position = SpawnPoints[k0].position + Random.insideUnitSphere * MaxStartScatter;
                        obj.transform.localRotation = SpawnPoints[k0].localRotation;

                        if (obj.transform.gameObject.GetComponent<USEnemyShip>())
                        {
                            obj.transform.gameObject.GetComponent<USEnemyShip>().USTrafficLink = transform.gameObject.GetComponent<USTraffic>();
                            obj.transform.gameObject.GetComponent<USEnemyShip>().Speed += CarSpeedVariations[k0];
                        }
                        else
                        {
                            Debug.Log(" <color=yellow> Wrong vehicle! 'USEnemyShip' script is required </color>");
                            locked = true;
                            break;
                        }
                        obj.transform.parent = VehicleContainer.transform;
                        VehicleCount++;

                    }
                }
                StartCoroutine(SpawnInterval(Random.Range(MinSpawnInterval, MaxSpawnInterval)));
            }
        }

        void OnDrawGizmos()
        {
            for (int k0 = 0; k0 < transform.childCount; k0++)
            {
                Vector3 Pos = transform.GetChild(k0).transform.localPosition;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(Pos, MaxStartScatter);
                Gizmos.DrawLine(transform.GetChild(k0).position, transform.GetChild(k0).position + transform.GetChild(k0).transform.TransformVector(Vector3.forward) * 130);
                Gizmos.color = Color.yellow;
                for (int k1 = 0; k1 < 4; k1++)
                {
                    Gizmos.DrawWireSphere(Pos += transform.GetChild(k0).transform.TransformVector(Vector3.forward) * 30, MaxStartScatter);
                }
            }
        }
    }
}
