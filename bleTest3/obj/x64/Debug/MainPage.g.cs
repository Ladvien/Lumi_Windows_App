﻿#pragma checksum "C:\Users\cthom\Lumi_Windows_App\bleTest3\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "E6B7203E1300538A32344DFD815DD9EE"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Lumi
{
    partial class MainPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                {
                    this.consolePanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 2:
                {
                    this.mainDisplayScroll = (global::Windows.UI.Xaml.Controls.ScrollViewer)(target);
                }
                break;
            case 3:
                {
                    this.mainPivotTable = (global::Windows.UI.Xaml.Controls.Pivot)(target);
                }
                break;
            case 4:
                {
                    this.pvtPortSettings = (global::Windows.UI.Xaml.Controls.PivotItem)(target);
                }
                break;
            case 5:
                {
                    this.tabOTA = (global::Windows.UI.Xaml.Controls.PivotItem)(target);
                }
                break;
            case 6:
                {
                    this.tabTSB = (global::Windows.UI.Xaml.Controls.PivotItem)(target);
                }
                break;
            case 7:
                {
                    global::Windows.UI.Xaml.Controls.Button element7 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 203 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element7).Click += this.Button_ReadFlash;
                    #line default
                }
                break;
            case 8:
                {
                    this.cmbFlashDisplay = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 204 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbFlashDisplay).SelectionChanged += this.displayFlashType_SelectionChanged;
                    #line default
                }
                break;
            case 9:
                {
                    this.txbFilePathForFlashOutput = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 10:
                {
                    this.btnSelectFileForFlashOutput = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 211 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnSelectFileForFlashOutput).Click += this.Button_ReadFlash;
                    #line default
                }
                break;
            case 11:
                {
                    global::Windows.UI.Xaml.Controls.Button element11 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 212 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element11).Click += this.OpenFile_ButtonClick;
                    #line default
                }
                break;
            case 12:
                {
                    global::Windows.UI.Xaml.Controls.Button element12 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 213 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element12).Click += this.Button_WriteFlash;
                    #line default
                }
                break;
            case 13:
                {
                    this.txbOpenFilePath = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 14:
                {
                    this.cmbOTADevice = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 177 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbOTADevice).SelectionChanged += this.cmbOTADevice_SelectionChanged;
                    #line default
                }
                break;
            case 15:
                {
                    this.cmbResetPin = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 182 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbResetPin).SelectionChanged += this.cmbResetPinSelector_SelectionChanged;
                    #line default
                }
                break;
            case 16:
                {
                    this.cmbPort = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 119 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbPort).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 17:
                {
                    this.cmbBaud = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 122 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbBaud).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 18:
                {
                    this.cmbDataBits = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 125 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbDataBits).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 19:
                {
                    this.cmbStopBits = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 128 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbStopBits).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 20:
                {
                    this.cmbParity = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 131 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbParity).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 21:
                {
                    this.cmbHandshaking = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 134 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbHandshaking).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 22:
                {
                    this.btnSendText = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 94 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnSendText).Click += this.btnSendText_Click;
                    #line default
                }
                break;
            case 23:
                {
                    this.btnClearSendText = (global::Windows.UI.Xaml.Controls.Button)(target);
                }
                break;
            case 24:
                {
                    this.rtbSendTextBlock = (global::Windows.UI.Xaml.Controls.RichEditBox)(target);
                }
                break;
            case 25:
                {
                    this.rtbMainDisplay = (global::Windows.UI.Xaml.Controls.RichTextBlock)(target);
                }
                break;
            case 26:
                {
                    this.theOneParagraph = (global::Windows.UI.Xaml.Documents.Paragraph)(target);
                }
                break;
            case 27:
                {
                    this.theOneRun = (global::Windows.UI.Xaml.Documents.Run)(target);
                }
                break;
            case 28:
                {
                    this.connectionLabelBackGround = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 29:
                {
                    this.btnConnect = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 48 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnConnect).Click += this.btnConnect_Click;
                    #line default
                }
                break;
            case 30:
                {
                    this.cmbDeviceSelector = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 49 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbDeviceSelector).SelectionChanged += this.cmbDeviceSelector_SelectionChanged_1;
                    #line default
                }
                break;
            case 31:
                {
                    this.cmbFoundDevices = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 53 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbFoundDevices).SelectionChanged += this.cmbFoundDevices_SelectionChanged;
                    #line default
                }
                break;
            case 32:
                {
                    this.btnWirelessSearch = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 55 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnWirelessSearch).Click += this.btnWirelessSearch_Click;
                    #line default
                }
                break;
            case 33:
                {
                    global::Windows.UI.Xaml.Controls.Button element33 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 56 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element33).Click += this.Button_Click;
                    #line default
                }
                break;
            case 34:
                {
                    this.btnTsbConnect = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 60 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnTsbConnect).Click += this.btnTsbConnect_Click;
                    #line default
                }
                break;
            case 35:
                {
                    this.btnReset = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 61 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnReset).Click += this.btnReset_Click;
                    #line default
                }
                break;
            case 36:
                {
                    this.pbSys = (global::Windows.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 37:
                {
                    this.btnBleSearch = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 63 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnBleSearch).Click += this.btnBleSearch_Click;
                    #line default
                }
                break;
            case 38:
                {
                    this.cbiSerialPort = (global::Windows.UI.Xaml.Controls.ComboBoxItem)(target);
                }
                break;
            case 39:
                {
                    this.cbiBluetooth = (global::Windows.UI.Xaml.Controls.ComboBoxItem)(target);
                }
                break;
            case 40:
                {
                    this.labelConnectionStatus = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

