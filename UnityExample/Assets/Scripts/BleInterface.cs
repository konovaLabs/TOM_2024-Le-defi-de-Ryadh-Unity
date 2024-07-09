using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR_OSX || UNITY_IOS
using UnityCoreBluetooth;

public class WheelSample
{
    public int counter;
    public int timestamp;

    public WheelSample(byte[] data)
    {
        if (data.Length < 7)
        {
            return;
        }

        this.counter = BitConverter.ToInt32(data, 1);
        this.timestamp = BitConverter.ToInt16(data, 5);
    }   
}

public class BleInterface : MonoBehaviour
{


    public Text wheel_left_text;
    public Text wheel_right_text;

    private CoreBluetoothManager manager;
    private CoreBluetoothCharacteristic characteristic_left_wheel = null;
    private CoreBluetoothCharacteristic characteristic_left_led = null;
    private CoreBluetoothCharacteristic characteristic_right_wheel = null;
    private CoreBluetoothCharacteristic characteristic_right_led = null;

    const string UUID_service_left_wheel = "02345678-1234-5678-1234-56789ABCDEF0";
    const string UUID_service_right_wheel = "12345678-1234-5678-1234-56789ABCDEF0";
    const string UUID_characteristic_left_wheel = "02345678-1234-5678-1234-56789ABCDEF1";
    const string UUID_characteristic_left_led = "02345678-1234-5678-1234-56789ABCDEF2";
    const string UUID_characteristic_right_wheel = "12345678-1234-5678-1234-56789ABCDEF1";
    const string UUID_characteristic_right_led = "12345678-1234-5678-1234-56789ABCDEF2";

    const string periph_name = "La Roue"; 

    byte[] value_1 = null;
    byte[] value_2 = null;

    WheelSample last_sample_left_wheel = null;
    WheelSample last_sample_right_wheel = null;

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
            //if (peripheral.name != "")
            //{
            //    //Debug.Log("discover peripheral name: " + peripheral.name);
            //}
            if (peripheral.name != "La Roue")
            {
                return;
            }
            //Debug.Log("Scan to peripheral name: " + peripheral.name);
            //manager.StopScan(); 
            manager.ConnectToPeripheral(peripheral);
            //if(characteristic != null && characteristic2 != null)
            //{
            //    manager.StopScan();
            //}
        });

        manager.OnConnectPeripheral((CoreBluetoothPeripheral peripheral) =>
        {
            //Debug.Log("connected peripheral name: " + peripheral.name);
            peripheral.discoverServices();
        });

        manager.OnDiscoverService((CoreBluetoothService service) =>
        {
            //Debug.Log("discover service uuid: " + service.uuid + " / " + UUID_service_left_wheel);
            //if (service.uuid == UUID_service_left_wheel || service.uuid == UUID_service_right_wheel)
            //{
            //        service.discoverCharacteristics();

            //}
            switch (service.uuid)
            {
                case UUID_service_left_wheel:
                case UUID_service_right_wheel:
                    service.discoverCharacteristics();
                    break;
                default:
                    //Debug.Log("Return");
                    return;
            }
        });


        manager.OnDiscoverCharacteristic((CoreBluetoothCharacteristic characteristic) =>
        {
            //Debug.Log("OnDiscoverCharacteristic " + characteristic.Uuid);
            //if (characteristic.Uuid == UUID_characteristic_left_wheel)
            //{
            //    this.characteristic_left_wheel = characteristic;
            //}
            //else if (characteristic.Uuid == UUID_characteristic_right_wheel)
            //{
            //    this.characteristic_right_wheel = characteristic;
            //}
            //else
            //{
            //    return;
            //}
            switch (characteristic.Uuid)
            {
                case UUID_characteristic_left_wheel:
                    this.characteristic_left_wheel = characteristic;
                    break;
                case UUID_characteristic_right_wheel:
                    this.characteristic_right_wheel = characteristic;
                    break;
                case UUID_characteristic_left_led:
                    this.characteristic_left_led = characteristic;
                    break;
                case UUID_characteristic_right_led:
                    this.characteristic_right_led = characteristic;
                    break;
                default:
                    return;
            }
            string uuid = characteristic.Uuid;
            string[] usage = characteristic.Propertis;
            //Debug.Log("discover characteristic uuid: " + uuid + ", usage: " + usage);
            for (int i = 0; i < usage.Length; i++)
            {
                //Debug.Log("discover characteristic uuid: " + uuid + ", usage: " + usage[i]);
                if (usage[i] == "notify")
                    characteristic.SetNotifyValue(true);
            }
        });

        manager.OnUpdateValue((CoreBluetoothCharacteristic characteristic, byte[] data) =>
        {
            if (this.characteristic_left_wheel != null && characteristic.Uuid == this.characteristic_left_wheel.Uuid)
            {
                this.value_1 = data;
                this.last_sample_left_wheel = new WheelSample(data);
            }
            else if (this.characteristic_right_wheel != null && characteristic.Uuid == this.characteristic_right_wheel.Uuid)
            {
                this.value_2 = data;
                this.last_sample_right_wheel = new WheelSample(data);
            }
            else
            {
                return;
            }
            this.flag = true;
        });
        manager.Start();
    }

    private bool flag = false;

    // Update is called once per frame 
    void Update()
    {
        if (flag == false) return;
        flag = false;

        if (this.last_sample_left_wheel != null)
        {
            wheel_left_text.text = $"Left: {this.last_sample_left_wheel.counter} / @{this.last_sample_left_wheel.timestamp}";
        }
        if (this.last_sample_right_wheel != null)
        {
            wheel_right_text.text = $"Right: {this.last_sample_right_wheel.counter} / @{this.last_sample_right_wheel.timestamp}";
        }

    }

    public void SetLeftLed(byte red, byte green, byte blue)
    {
        if(characteristic_left_led == null)
        {
            return;
        }
        characteristic_left_led.Write(new byte[] { red, green , blue});
    }

    public void SetRightLed(byte red, byte green, byte blue)
    {
        if (characteristic_right_led == null)
        {
            return;
        }
        //Debug.Log("Set Right led " + characteristic_right_led.Uuid);
        characteristic_right_led.Write(new byte[] { red, green, blue });
    }

    void OnDestroy()
    {
        if(manager != null)
        {
            manager.Stop();
        }
        characteristic_left_wheel = null;
        characteristic_right_wheel = null;
        characteristic_right_led = null;
        characteristic_left_led = null;
        last_sample_right_wheel = null;
        last_sample_left_wheel = null;
    }

    private int counter = 0;

    public void Write()
    {
        //characteristic.Write(System.Text.Encoding.UTF8.GetBytes($"{counter}"));
        //counter++;
        SetRightLed(0x00, 0xFF, 0x00);
        //characteristic_right_led.Write(System.Text.Encoding.UTF8.GetBytes("ABCD"));
        //characteristic_left_led.Write(System.Text.Encoding.UTF8.GetBytes("ABCD"));
    }

    public WheelSample GetLastWheelLeftEvent()
    {
        return this.last_sample_left_wheel;
    }

    public WheelSample GetLastWheelRightEvent()
    {
        return this.last_sample_right_wheel;
    }

    public bool IsLeftWheelConneted()
    {
        return (this.characteristic_left_wheel != null);
    }

    public bool IsRightWheelConnected()
    {
        return (this.characteristic_right_wheel != null);
    }
}
#endif
