using BLE_2.BLE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace BLE_2
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        #region Variable Discover Devices
        private ObservableCollection<BluetoothLEDeviceDisplay> KnownDevices = new ObservableCollection<BluetoothLEDeviceDisplay>();
        private List<DeviceInformation> UnknownDevices = new List<DeviceInformation>();

        private DeviceWatcher deviceWatcher;
        #endregion

        #region Variables Connect to Device
        private ObservableCollection<BluetoothLEAttributeDisplay> ServiceCollection = new ObservableCollection<BluetoothLEAttributeDisplay>();
        private ObservableCollection<BluetoothLEAttributeDisplay> CharacteristicCollection = new ObservableCollection<BluetoothLEAttributeDisplay>();

        private BluetoothLEDevice bluetoothLeDevice = null;
        private GattCharacteristic selectedCharacteristic;

        // Only one registered characteristic at a time.
        private GattCharacteristic registeredCharacteristic;
        private GattPresentationFormat presentationFormat;

        #region TT Variablen
        private DeviceInformation CurrentBTDeviceInfo { get; set; }
        #endregion

        #region BLE Error Codes
        readonly int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        readonly int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        readonly int E_ACCESSDENIED = unchecked((int)0x80070005);
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        #endregion
        #endregion

        public SettingsPage()
        {
            this.InitializeComponent();

            ResultsListView.ItemsSource = KnownDevices;
            //ResultsListView.DisplayMemberPath = "Name";
            StartBleDeviceWatcher();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
        
        #region UI Code

        private void EnumerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (deviceWatcher == null)
            {
                StartBleDeviceWatcher();
                EnumerateButton.Content = "Start enumerating";
                NotifyUser($"Device watscher started.", NotifyType.StatusMessage);
            }
            else
            {
                StopBleDeviceWatcher();
                EnumerateButton.Content = "Start enumerating";
                NotifyUser($"Device watcher stopped.", NotifyType.StatusMessage);
            }

        }

        private async void Connect(/*DeviceInformation SelectedBleDevice*/)
        {
            try
            {
                // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                //BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
                bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(bleDeviceDisplay.Id/*SelectedBleDevice.Id*/);

                if (bluetoothLeDevice == null)
                {
                    NotifyUser("Failed to connect to device.", NotifyType.ErrorMessage);
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                NotifyUser("Bluetooth radio is not on.", NotifyType.ErrorMessage);
            }

            if (bluetoothLeDevice != null)
            {
                // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
                GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    NotifyUser(String.Format("Found {0} services", services.Count), NotifyType.StatusMessage);
                    foreach (var service in services)
                    {
                        ServiceCollection.Add(new BluetoothLEAttributeDisplay(service));
                    }
                    ConnectButton.Visibility = Visibility.Collapsed;
                    ServiceList.Visibility = Visibility.Visible;
                }
                else
                {
                    NotifyUser("Device unreachable", NotifyType.ErrorMessage);
                }
            }
        }
        BluetoothLEDeviceDisplay bleDeviceDisplay;
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectButton.IsEnabled = false;

            if (!await ClearBluetoothLEDeviceAsync())
            {
                NotifyUser("Error: Unable to reset state, try again.", NotifyType.ErrorMessage);
                ConnectButton.IsEnabled = false;
                return;
            }
            /*if (CurrentBTDeviceInfo != null) // already connected -> disconnect
            {
                return;
            }*/

            bleDeviceDisplay = ResultsListView.SelectedItem as BluetoothLEDeviceDisplay;

            if (ResultsListView.SelectedItem != null)
                Debug.WriteLine("Test1");
            else
                Debug.WriteLine("Test2");

            if (ResultsListView.SelectedItem is BluetoothLEDeviceDisplay)//DeviceInformation)
                Debug.WriteLine("Test3");
            else
                Debug.WriteLine("Test4");


            if (bleDeviceDisplay != null)
            {
                var SelectedBleDeviceId = bleDeviceDisplay.Id;
                var SelectedBleDeviceName = bleDeviceDisplay.Name;
                Connect(/*SelectedBleDeviceId*/);
            }
            /*if (ResultsListView.SelectedItem != null &&
                ResultsListView.SelectedItem is BluetoothLEDeviceDisplay)//DeviceInformation)
            {
                var dev = ResultsListView.SelectedItem as BluetoothLEDeviceDisplay;//DeviceInformation;
                Connect(dev);
            }*/

            ConnectButton.IsEnabled = true;
        }
        #endregion


        #region Enumerating Services
        private async Task<bool> ClearBluetoothLEDeviceAsync()
        {
            if (subscribedForNotifications)
            {
                // Need to clear the CCCD from the remote device so we stop receiving notifications
                var result = await registeredCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                if (result != GattCommunicationStatus.Success)
                {
                    return false;
                }
                else
                {
                    selectedCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                    subscribedForNotifications = false;
                }
            }
            bluetoothLeDevice?.Dispose();
            bluetoothLeDevice = null;
            CurrentBTDeviceInfo = null;
            ServiceCollection = new ObservableCollection<BluetoothLEAttributeDisplay>();
            return true;
        }
        #endregion

        #region Enumerating Characteristics
        private async void ServiceList_SelectionChanged()
        {
            var attributeInfoDisp = (BluetoothLEAttributeDisplay)ServiceList.SelectedItem;

            CharacteristicCollection.Clear();
            RemoveValueChangedHandler();

            IReadOnlyList<GattCharacteristic> characteristics = null;
            try
            {
                // Ensure we have access to the device.
                var accessStatus = await attributeInfoDisp.service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                    // and the new Async functions to get the characteristics of unpaired devices as well. 
                    var result = await attributeInfoDisp.service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        characteristics = result.Characteristics;
                    }
                    else
                    {
                        NotifyUser("Error accessing service.", NotifyType.ErrorMessage);

                        // On error, act as if there are no characteristics.
                        characteristics = new List<GattCharacteristic>();
                    }
                }
                else
                {
                    // Not granted access
                    NotifyUser("Error accessing service.", NotifyType.ErrorMessage);

                    // On error, act as if there are no characteristics.
                    characteristics = new List<GattCharacteristic>();

                }
            }
            catch (Exception ex)
            {
                NotifyUser("Restricted service. Can't read characteristics: " + ex.Message,
                    NotifyType.ErrorMessage);
                // On error, act as if there are no characteristics.
                characteristics = new List<GattCharacteristic>();
            }

            foreach (GattCharacteristic c in characteristics)
            {
                CharacteristicCollection.Add(new BluetoothLEAttributeDisplay(c));
            }
            CharacteristicList.Visibility = Visibility.Visible;
        }
        #endregion

        #region rest
        private void AddValueChangedHandler()
        {
            ValueChangedSubscribeToggle.Content = "Unsubscribe from value changes";
            if (!subscribedForNotifications)
            {
                registeredCharacteristic = selectedCharacteristic;
                registeredCharacteristic.ValueChanged += Characteristic_ValueChanged;
                subscribedForNotifications = true;
            }
        }

        private void RemoveValueChangedHandler()
        {
            ValueChangedSubscribeToggle.Content = "Subscribe to value changes";
            if (subscribedForNotifications)
            {
                registeredCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                registeredCharacteristic = null;
                subscribedForNotifications = false;
            }
        }

        private async void CharacteristicList_SelectionChanged()
        {
            selectedCharacteristic = null;

            var attributeInfoDisp = (BluetoothLEAttributeDisplay)CharacteristicList.SelectedItem;
            if (attributeInfoDisp == null)
            {
                EnableCharacteristicPanels(GattCharacteristicProperties.None);
                return;
            }

            selectedCharacteristic = attributeInfoDisp.characteristic;
            if (selectedCharacteristic == null)
            {
                NotifyUser("No characteristic selected", NotifyType.ErrorMessage);
                return;
            }

            // Get all the child descriptors of a characteristics. Use the cache mode to specify uncached descriptors only 
            // and the new Async functions to get the descriptors of unpaired devices as well. 
            var result = await selectedCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
            {
                NotifyUser("Descriptor read failure: " + result.Status.ToString(), NotifyType.ErrorMessage);
            }

            // BT_Code: There's no need to access presentation format unless there's at least one. 
            presentationFormat = null;
            if (selectedCharacteristic.PresentationFormats.Count > 0)
            {

                if (selectedCharacteristic.PresentationFormats.Count.Equals(1))
                {
                    // Get the presentation format since there's only one way of presenting it
                    presentationFormat = selectedCharacteristic.PresentationFormats[0];
                }
                else
                {
                    // It's difficult to figure out how to split up a characteristic and encode its different parts properly.
                    // In this case, we'll just encode the whole thing to a string to make it easy to print out.
                }
            }

            // Enable/disable operations based on the GattCharacteristicProperties.
            EnableCharacteristicPanels(selectedCharacteristic.CharacteristicProperties);
        }

        private void SetVisibility(UIElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void EnableCharacteristicPanels(GattCharacteristicProperties properties)
        {
            // BT_Code: Hide the controls which do not apply to this characteristic.
            SetVisibility(CharacteristicReadButton, properties.HasFlag(GattCharacteristicProperties.Read));

            SetVisibility(CharacteristicWritePanel,
                properties.HasFlag(GattCharacteristicProperties.Write) ||
                properties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse));
            CharacteristicWriteValue.Text = "";

            SetVisibility(ValueChangedSubscribeToggle, properties.HasFlag(GattCharacteristicProperties.Indicate) ||
                                                       properties.HasFlag(GattCharacteristicProperties.Notify));

        }

        private async void CharacteristicReadButton_Click()
        {
            // BT_Code: Read the actual value from the device by using Uncached.
            GattReadResult result = await selectedCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (result.Status == GattCommunicationStatus.Success)
            {
                string formattedResult = FormatValueByPresentation(result.Value, presentationFormat);
                NotifyUser($"Read result: {formattedResult}", NotifyType.StatusMessage);
            }
            else
            {
                NotifyUser($"Read failed: {result.Status}", NotifyType.ErrorMessage);
            }
        }

        private async void CharacteristicWriteButton_Click()
        {
            if (!String.IsNullOrEmpty(CharacteristicWriteValue.Text))
            {
                var writeBuffer = CryptographicBuffer.ConvertStringToBinary(CharacteristicWriteValue.Text,
                    BinaryStringEncoding.Utf8);

                var writeSuccessful = await WriteBufferToSelectedCharacteristicAsync(writeBuffer);
            }
            else
            {
                NotifyUser("No data to write to device", NotifyType.ErrorMessage);
            }
        }

        private async void CharacteristicWriteButtonInt_Click()
        {
            if (!String.IsNullOrEmpty(CharacteristicWriteValue.Text))
            {
                var isValidValue = Int32.TryParse(CharacteristicWriteValue.Text, out int readValue);
                if (isValidValue)
                {
                    var writer = new DataWriter();
                    writer.ByteOrder = ByteOrder.LittleEndian;
                    writer.WriteInt32(readValue);

                    var writeSuccessful = await WriteBufferToSelectedCharacteristicAsync(writer.DetachBuffer());
                }
                else
                {
                    NotifyUser("Data to write has to be an int32", NotifyType.ErrorMessage);
                }
            }
            else
            {
                NotifyUser("No data to write to device", NotifyType.ErrorMessage);
            }
        }

        private async Task<bool> WriteBufferToSelectedCharacteristicAsync(IBuffer buffer)
        {
            try
            {
                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await selectedCharacteristic.WriteValueWithResultAsync(buffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    NotifyUser("Successfully wrote value to device", NotifyType.StatusMessage);
                    return true;
                }
                else
                {
                    NotifyUser($"Write failed: {result.Status}", NotifyType.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_INVALID_PDU)
            {
                NotifyUser(ex.Message, NotifyType.ErrorMessage);
                return false;
            }
            catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED || ex.HResult == E_ACCESSDENIED)
            {
                // This usually happens when a device reports that it support writing, but it actually doesn't.
                NotifyUser(ex.Message, NotifyType.ErrorMessage);
                return false;
            }
        }

        private bool subscribedForNotifications = false;
        private async void ValueChangedSubscribeToggle_Click()
        {
            if (!subscribedForNotifications)
            {
                // initialize status
                GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
                var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
                if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                {
                    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
                }

                else if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
                }

                try
                {
                    // BT_Code: Must write the CCCD in order for server to send indications.
                    // We receive them in the ValueChanged event handler.
                    status = await selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                    if (status == GattCommunicationStatus.Success)
                    {
                        AddValueChangedHandler();
                        NotifyUser("Successfully subscribed for value changes", NotifyType.StatusMessage);
                    }
                    else
                    {
                        NotifyUser($"Error registering for value changes: {status}", NotifyType.ErrorMessage);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support indicate, but it actually doesn't.
                    NotifyUser(ex.Message, NotifyType.ErrorMessage);
                }
            }
            else
            {
                try
                {
                    // BT_Code: Must write the CCCD in order for server to send notifications.
                    // We receive them in the ValueChanged event handler.
                    // Note that this sample configures either Indicate or Notify, but not both.
                    var result = await
                            selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (result == GattCommunicationStatus.Success)
                    {
                        subscribedForNotifications = false;
                        RemoveValueChangedHandler();
                        NotifyUser("Successfully un-registered for notifications", NotifyType.StatusMessage);
                    }
                    else
                    {
                        NotifyUser($"Error un-registering for notifications: {result}", NotifyType.ErrorMessage);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support notify, but it actually doesn't.
                    NotifyUser(ex.Message, NotifyType.ErrorMessage);
                }
            }
        }

        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // BT_Code: An Indicate or Notify reported that the value has changed.
            // Display the new value with a timestamp.
            var newValue = FormatValueByPresentation(args.CharacteristicValue, presentationFormat);
            var message = $"Value at {DateTime.Now:hh:mm:ss.FFF}: {newValue}";
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => CharacteristicLatestValue.Text = message);
        }

        private string FormatValueByPresentation(IBuffer buffer, GattPresentationFormat format)
        {
            // BT_Code: For the purpose of this sample, this function converts only UInt32 and
            // UTF-8 buffers to readable text. It can be extended to support other formats if your app needs them.
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);
            if (format != null)
            {
                if (format.FormatType == GattPresentationFormatTypes.UInt32 && data.Length >= 4)
                {
                    return BitConverter.ToInt32(data, 0).ToString();
                }
                else if (format.FormatType == GattPresentationFormatTypes.Utf8)
                {
                    try
                    {
                        return Encoding.UTF8.GetString(data);
                    }
                    catch (ArgumentException)
                    {
                        return "(error: Invalid UTF-8 string)";
                    }
                }
                else
                {
                    // Add support for other format types as needed.
                    return "Unsupported format: " + CryptographicBuffer.EncodeToHexString(buffer);
                }
            }
            else if (data != null)
            {
                // We don't know what format to use. Let's try some well-known profiles, or default back to UTF-8.
                if (selectedCharacteristic.Uuid.Equals(GattCharacteristicUuids.HeartRateMeasurement))
                {
                    try
                    {
                        return "Heart Rate: " + ParseHeartRateValue(data).ToString();
                    }
                    catch (ArgumentException)
                    {
                        return "Heart Rate: (unable to parse)";
                    }
                }
                else if (selectedCharacteristic.Uuid.Equals(GattCharacteristicUuids.BatteryLevel))
                {
                    try
                    {
                        // battery level is encoded as a percentage value in the first byte according to
                        // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.battery_level.xml
                        return "Battery Level: " + data[0].ToString() + "%";
                    }
                    catch (ArgumentException)
                    {
                        return "Battery Level: (unable to parse)";
                    }
                }
                // This is our custom calc service Result UUID. Format it like an Int
                else if (selectedCharacteristic.Uuid.Equals(Constants.ResultCharacteristicUuid))
                {
                    return BitConverter.ToInt32(data, 0).ToString();
                }
                // No guarantees on if a characteristic is registered for notifications.
                else if (registeredCharacteristic != null)
                {
                    // This is our custom calc service Result UUID. Format it like an Int
                    if (registeredCharacteristic.Uuid.Equals(Constants.ResultCharacteristicUuid))
                    {
                        return BitConverter.ToInt32(data, 0).ToString();
                    }
                }
                else
                {
                    try
                    {
                        return "Unknown format: " + Encoding.UTF8.GetString(data);
                    }
                    catch (ArgumentException)
                    {
                        return "Unknown format";
                    }
                }
            }
            else
            {
                return "Empty data received";
            }
            return "Unknown format";
        }

        /// <summary>
        /// Process the raw data received from the device into application usable data,
        /// according the the Bluetooth Heart Rate Profile.
        /// https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.heart_rate_measurement.xml&u=org.bluetooth.characteristic.heart_rate_measurement.xml
        /// This function throws an exception if the data cannot be parsed.
        /// </summary>
        /// <param name="data">Raw data received from the heart rate monitor.</param>
        /// <returns>The heart rate measurement value.</returns>
        private static ushort ParseHeartRateValue(byte[] data)
        {
            // Heart Rate profile defined flag values
            const byte heartRateValueFormat = 0x01;

            byte flags = data[0];
            bool isHeartRateValueSizeLong = ((flags & heartRateValueFormat) != 0);

            if (isHeartRateValueSizeLong)
            {
                return BitConverter.ToUInt16(data, 1);
            }
            else
            {
                return data[1];
            }
        }
        #endregion

        #region Device discovery

        /// <summary>
        /// Starts a device watcher that looks for all nearby Bluetooth devices (paired or unpaired). 
        /// Attaches event handlers to populate the device collection.
        /// </summary>
        private void StartBleDeviceWatcher()
        {
            // Additional properties we would like about the device.
            // Property strings are documented here https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

            // BT_Code: Example showing paired and non-paired in a single query.
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start over with an empty collection.
            KnownDevices.Clear();

            // Start the watcher.
            deviceWatcher.Start();
        }

        /// <summary>
        /// Stops watching for all nearby Bluetooth devices.
        /// </summary>
        private void StopBleDeviceWatcher()
        {
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }
        }

        private BluetoothLEDeviceDisplay FindBluetoothLEDeviceDisplay(string id)
        {
            foreach (BluetoothLEDeviceDisplay bleDeviceDisplay in KnownDevices)
            {
                if (bleDeviceDisplay.Id == id)
                {
                    return bleDeviceDisplay;
                }
            }
            return null;
        }

        private DeviceInformation FindUnknownDevices(string id)
        {
            foreach (DeviceInformation bleDeviceInfo in UnknownDevices)
            {
                if (bleDeviceInfo.Id == id)
                {
                    return bleDeviceInfo;
                }
            }
            return null;
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Added {0}{1}", deviceInfo.Id, deviceInfo.Name));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Make sure device isn't already present in the list.
                        if (FindBluetoothLEDeviceDisplay(deviceInfo.Id) == null)
                        {
                            if (deviceInfo.Name != string.Empty)
                            {
                                // If device has a friendly name display it immediately.
                                KnownDevices.Add(new BluetoothLEDeviceDisplay(deviceInfo));
                            }
                            else
                            {
                                // Add it to a list in case the name gets updated later. 
                                UnknownDevices.Add(deviceInfo);
                            }
                        }

                    }
                }
            });
        }

        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Updated {0}{1}", deviceInfoUpdate.Id, ""));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        BluetoothLEDeviceDisplay bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                        if (bleDeviceDisplay != null)
                        {
                            // Device is already being displayed - update UX.
                            bleDeviceDisplay.Update(deviceInfoUpdate);
                            return;
                        }

                        DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                        if (deviceInfo != null)
                        {
                            deviceInfo.Update(deviceInfoUpdate);
                            // If device has been updated with a friendly name it's no longer unknown.
                            if (deviceInfo.Name != String.Empty)
                            {
                                KnownDevices.Add(new BluetoothLEDeviceDisplay(deviceInfo));
                                UnknownDevices.Remove(deviceInfo);
                            }
                        }
                    }
                }
            });
        }

        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Removed {0}{1}", deviceInfoUpdate.Id, ""));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Find the corresponding DeviceInformation in the collection and remove it.
                        BluetoothLEDeviceDisplay bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                        if (bleDeviceDisplay != null)
                        {
                            KnownDevices.Remove(bleDeviceDisplay);
                        }

                        DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                        if (deviceInfo != null)
                        {
                            UnknownDevices.Remove(deviceInfo);
                        }
                    }
                }
            });
        }

        private async void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    NotifyUser($"{KnownDevices.Count} devices found. Enumeration completed.",
                        NotifyType.StatusMessage);
                }
            });
        }

        private async void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    NotifyUser($"No longer watching for devices.",
                            sender.Status == DeviceWatcherStatus.Aborted ? NotifyType.ErrorMessage : NotifyType.StatusMessage);
                }
            });
        }

        /// <summary>
        /// Display a message to the user.
        /// This method may be called from any thread.
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update.
            if (Dispatcher.HasThreadAccess)
            {
                UpdateStatus(strMessage, type);
            }
            else
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateStatus(strMessage, type));
            }
        }

        private void UpdateStatus(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }

            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
            if (StatusBlock.Text != String.Empty)
            {
                StatusBorder.Visibility = Visibility.Visible;
                StatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
                StatusPanel.Visibility = Visibility.Collapsed;
            }
        }
        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

        #endregion

        #region Pairing

        private bool isBusy = false;

        private async void PairButton_Click()
        {
            // Do not allow a new Pair operation to start if an existing one is in progress.
            if (isBusy)
            {
                return;
            }

            isBusy = true;

            NotifyUser("Pairing started. Please wait...", NotifyType.StatusMessage);

            // For more information about device pairing, including examples of
            // customizing the pairing process, see the DeviceEnumerationAndPairing sample.

            // Capture the current selected item in case the user changes it while we are pairing.
            var bleDeviceDisplay = ResultsListView.SelectedItem as BluetoothLEDeviceDisplay;

            if (bleDeviceDisplay != null)
            {
                // BT_Code: Pair the currently selected device.
                DevicePairingResult result = await bleDeviceDisplay.DeviceInformation.Pairing.PairAsync();
                NotifyUser($"Pairing result = {result.Status}",
                    result.Status == DevicePairingResultStatus.Paired || result.Status == DevicePairingResultStatus.AlreadyPaired
                        ? NotifyType.StatusMessage
                        : NotifyType.ErrorMessage);
            }

            isBusy = false;
        }

        #endregion
    }
}
