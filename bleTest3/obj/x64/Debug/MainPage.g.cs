﻿#pragma checksum "C:\Users\cthom\Lumi_Windows_App\bleTest3\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "880030FE20AB8EC03EBF36E71937DA42"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace bleTest3
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
                    this.pvtPortSettings = (global::Windows.UI.Xaml.Controls.PivotItem)(target);
                }
                break;
            case 3:
                {
                    this.tabTSB = (global::Windows.UI.Xaml.Controls.PivotItem)(target);
                }
                break;
            case 4:
                {
                    global::Windows.UI.Xaml.Controls.Button element4 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 178 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element4).Click += this.Button_ReadFlash;
                    #line default
                }
                break;
            case 5:
                {
                    this.cmbFlashDisplay = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 179 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbFlashDisplay).SelectionChanged += this.displayFlashType_SelectionChanged;
                    #line default
                }
                break;
            case 6:
                {
                    this.txbFilePathForFlashOutput = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 7:
                {
                    this.btnSelectFileForFlashOutput = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 186 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnSelectFileForFlashOutput).Click += this.Button_ReadFlash;
                    #line default
                }
                break;
            case 8:
                {
                    global::Windows.UI.Xaml.Controls.Button element8 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 187 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element8).Click += this.OpenFile_ButtonClick;
                    #line default
                }
                break;
            case 9:
                {
                    this.txbOpenFilePath = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 10:
                {
                    this.cmbPort = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 119 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbPort).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 11:
                {
                    this.cmbBaud = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 122 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbBaud).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 12:
                {
                    this.cmbDataBits = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 125 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbDataBits).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 13:
                {
                    this.cmbStopBits = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 128 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbStopBits).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 14:
                {
                    this.cmbParity = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 131 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbParity).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 15:
                {
                    this.cmbHandshaking = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 134 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbHandshaking).SelectionChanged += this.cmbPort_SelectionChanged;
                    #line default
                }
                break;
            case 16:
                {
                    this.btnSendText = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 94 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnSendText).Click += this.btnSendText_Click;
                    #line default
                }
                break;
            case 17:
                {
                    this.btnClearSendText = (global::Windows.UI.Xaml.Controls.Button)(target);
                }
                break;
            case 18:
                {
                    this.rtbSendTextBlock = (global::Windows.UI.Xaml.Controls.RichEditBox)(target);
                }
                break;
            case 19:
                {
                    this.rtbMainDisplay = (global::Windows.UI.Xaml.Controls.RichTextBlock)(target);
                }
                break;
            case 20:
                {
                    this.theOneParagraph = (global::Windows.UI.Xaml.Documents.Paragraph)(target);
                }
                break;
            case 21:
                {
                    this.theOneRun = (global::Windows.UI.Xaml.Documents.Run)(target);
                }
                break;
            case 22:
                {
                    this.connectionLabelBackGround = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 23:
                {
                    this.btnConnect = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 49 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnConnect).Click += this.btnConnect_Click;
                    #line default
                }
                break;
            case 24:
                {
                    this.cmbDeviceSelector = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 50 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbDeviceSelector).SelectionChanged += this.cmbDeviceSelector_SelectionChanged_1;
                    #line default
                }
                break;
            case 25:
                {
                    this.cmbFoundDevices = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 54 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cmbFoundDevices).SelectionChanged += this.cmbFoundDevices_SelectionChanged;
                    #line default
                }
                break;
            case 26:
                {
                    this.btnBleSearch = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 56 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnBleSearch).Click += this.btnBleSearch_Click;
                    #line default
                }
                break;
            case 27:
                {
                    global::Windows.UI.Xaml.Controls.Button element27 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 57 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element27).Click += this.Button_Click;
                    #line default
                }
                break;
            case 28:
                {
                    this.btnTsbConnect = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 61 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnTsbConnect).Click += this.Button_Click_1;
                    #line default
                }
                break;
            case 29:
                {
                    this.btnTest = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 62 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnTest).Click += this.btnTest_Click;
                    #line default
                }
                break;
            case 30:
                {
                    this.pbSys = (global::Windows.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 31:
                {
                    this.cbiSerialPort = (global::Windows.UI.Xaml.Controls.ComboBoxItem)(target);
                }
                break;
            case 32:
                {
                    this.cbiBluetooth = (global::Windows.UI.Xaml.Controls.ComboBoxItem)(target);
                }
                break;
            case 33:
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

