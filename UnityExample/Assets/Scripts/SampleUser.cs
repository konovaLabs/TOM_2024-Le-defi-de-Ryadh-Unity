using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR_OSX || UNITY_IOS
using UnityCoreBluetooth;

public class SampleUser : MonoBehaviour
{

    public Text text;
    public Text text2;

    private CoreBluetoothManager manager;
    private CoreBluetoothCharacteristic characteristic;
    private CoreBluetoothCharacteristic characteristic2;


    public String peripheral_name;

    byte[] value_1, value_2;
    // Use this for initialization 
    void Start()
    {
        manager = CoreBluetoothManager.Shared;

        manager.OnUpdateState((string state) =>
        {
            Debug.Log("state: " + state);
            if (state != "poweredOn") return;
            manager.StartScan();
        });

        manager.OnDiscoverPeripheral((CoreBluetoothPeripheral peripheral) =>
        {
            if (peripheral.name != "")
                Debug.Log("discover peripheral name: " + peripheral.name + " / " + peripheral_name);
            if (peripheral.name != "TOM HotWheel" && peripheral.name != "TOM HotWheel2")
            {
                return;
            }
            //manager.StopScan(); 
            manager.ConnectToPeripheral(peripheral);
        });

        manager.OnConnectPeripheral((CoreBluetoothPeripheral peripheral) =>
        {
            Debug.Log("connected peripheral name: " + peripheral.name);
            peripheral.discoverServices();
        });

        manager.OnDiscoverService((CoreBluetoothService service) =>
        {
            Debug.Log("discover service uuid: " + service.uuid);
            switch (service.uuid)
            {
                case "12345678-1234-5678-1234-56789ABCDEF0":
                case "02345678-1234-5678-1234-56789ABCDEF0":
                    service.discoverCharacteristics();
                    break;
                default:
                    return;
            }
        });


        manager.OnDiscoverCharacteristic((CoreBluetoothCharacteristic characteristic) =>
        {
            switch (characteristic.Uuid)
            {
                case "12345678-1234-5678-1234-56789ABCDEF1":
                    this.characteristic = characteristic;
                    break;
                case "02345678-1234-5678-1234-56789ABCDEF1":
                    this.characteristic2 = characteristic;
                    break;
                default:
                    return;
            }
            string uuid = characteristic.Uuid;
            string[] usage = characteristic.Propertis;
            Debug.Log("discover characteristic uuid: " + uuid + ", usage: " + usage);
            for (int i = 0; i < usage.Length; i++)
            {
                Debug.Log("discover characteristic uuid: " + uuid + ", usage: " + usage[i]);
                if (usage[i] == "notify")
                    characteristic.SetNotifyValue(true);
            }
        });

        manager.OnUpdateValue((CoreBluetoothCharacteristic characteristic, byte[] data) =>
        {
            if (characteristic.Uuid == this.characteristic.Uuid)
            {
                this.value_1 = data;
            }
            else if (characteristic.Uuid == this.characteristic2.Uuid)
            {
                this.value_2 = data;
            }
            this.flag = true;
        });
        manager.Start();
    }

    private bool flag = false;
    private byte[] value = new byte[20];

    private float vy = 0.0f;

    // Update is called once per frame 
    void Update()
    {
        if (this.transform.position.y < 0)
        {
            vy = 0.0f;
            transform.position = new Vector3(0, 0, 0);
        }
        else
        {
            vy -= 0.006f;
            transform.position += new Vector3(0, vy, 0);
        }
        this.transform.Rotate(2, -3, 4);
        if (flag == false) return;
        flag = false;
        if (value_1.Length > 0)
        {
            text.text = $"Notify: {BitConverter.ToInt32(value_1, 0)}";
        }
        if (value_2.Length > 0)
        {
            text2.text = $"Notify: {BitConverter.ToInt32(value_2, 0)}";
        }
        vy += 0.1f;
        transform.position += new Vector3(0, vy, 0);
    }

    void OnDestroy()
    {
        manager.Stop();
    }

    private int counter = 0;

    public void Write()
    {
        characteristic.Write(System.Text.Encoding.UTF8.GetBytes($"{counter}"));
        counter++;
    }
}
#endif
