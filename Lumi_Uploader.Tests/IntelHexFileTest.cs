﻿using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Reflection;
using System.IO;
using Windows.ApplicationModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Lumi;
using System.Collections;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class IntelHexFileTestClass
    {

        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;


        [TestMethod]
        public async Task shouldReadIntelHexFileIntoArray()
        {
            // 1. Load test hex file
            // 2. Extract data using InelHexFileArray
            // 3. Compare to the manually extracted data.

            byte[] intelHexFileTestDataByteArray = { 0x28, 0xC0, 0x4C, 0xC0, 0x44, 0xC1, 0x4A, 0xC0, 0x49, 0xC0, 0x59, 0xC3, 0x47, 0xC0, 0x46, 0xC0, 0x45, 0xC0, 0x44, 0xC0, 0x43, 0xC0,
                0x42, 0xC0, 0x41, 0xC0, 0x40, 0xC0, 0x3F, 0xC0, 0x01, 0x02, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x20, 0x04, 0x10, 0x08,
                0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00, 0x38, 0x00, 0x00, 0x00, 0x00, 0x00, 0x37, 0x00,
                0x5D, 0x00, 0x63, 0x00, 0x11, 0x24, 0x1F, 0xBE, 0xCF, 0xE5, 0xD2, 0xE0, 0xDE, 0xBF, 0xCD, 0xBF, 0x10, 0xE0, 0xA0, 0xE6, 0xB0, 0xE0, 0xE2, 0xE9, 0xF8, 0xE0, 0x02, 0xC0,
                0x05, 0x90, 0x0D, 0x92, 0xA0, 0x37, 0xB1, 0x07, 0xD9, 0xF7, 0x20, 0xE0, 0xA0, 0xE7, 0xB0, 0xE0, 0x01, 0xC0, 0x1D, 0x92, 0xAC, 0x3D, 0xB2, 0x07, 0xE1, 0xF7, 0x10, 0xE0,
                0xC0, 0xE5, 0xD0, 0xE0, 0x03, 0xC0, 0x22, 0x97, 0xFE, 0x01, 0xF0, 0xD3, 0xCE, 0x34, 0xD1, 0x07, 0xD1, 0xF7, 0x08, 0xD3, 0xEF, 0xC3, 0xB1, 0xCF, 0x60, 0xE0, 0x83, 0xE0,
                0x6D, 0xD2, 0x61, 0xE0, 0x84, 0xE0, 0x6A, 0xD2, 0x40, 0xE8, 0x55, 0xE2, 0x60, 0xE0, 0x70, 0xE0, 0x80, 0xE7, 0x90, 0xE0, 0xE7, 0xC1, 0x08, 0x95, 0x20, 0xE0, 0x44, 0xE0,
                0x63, 0xE0, 0x80, 0xE7, 0x90, 0xE0, 0xB2, 0xC1, 0x80, 0xE7, 0x90, 0xE0, 0x5B, 0xC1, 0x20, 0x91, 0xD1, 0x00, 0x30, 0x91, 0xD2, 0x00, 0x28, 0x17, 0x39, 0x07, 0xB9, 0xF4,
                0x90, 0x91, 0x8F, 0x00, 0x80, 0x91, 0x90, 0x00, 0x98, 0x17, 0x89, 0xF0, 0xE0, 0x91, 0x8F, 0x00, 0xF0, 0xE0, 0xEF, 0x56, 0xFF, 0x4F, 0x80, 0x81, 0x20, 0x91, 0x8F, 0x00,
                0x30, 0xE0, 0x2F, 0x5F, 0x3F, 0x4F, 0x2F, 0x73, 0x33, 0x27, 0x20, 0x93, 0x8F, 0x00, 0x90, 0xE0, 0x08, 0x95, 0x8F, 0xEF, 0x9F, 0xEF, 0x08, 0x95, 0x1F, 0x93, 0xCF, 0x93,
                0xDF, 0x93, 0xDC, 0x01, 0x5C, 0x96, 0x8D, 0x91, 0x9C, 0x91, 0x5D, 0x97, 0x00, 0x97, 0x49, 0xF4, 0x81, 0xE0, 0x90, 0xE0, 0x13, 0x96, 0x9C, 0x93, 0x8E, 0x93, 0x12, 0x97,
                0x80, 0xE0, 0x90, 0xE0, 0x3B, 0xC0, 0x51, 0x96, 0xED, 0x91, 0xFC, 0x91, 0x52, 0x97, 0x50, 0x96, 0x2C, 0x91, 0x50, 0x97, 0x32, 0x2F, 0x30, 0x95, 0x1F, 0xB7, 0x5E, 0x96,
                0x4C, 0x91, 0x5E, 0x97, 0x41, 0xFB, 0x77, 0x27, 0x70, 0xF9, 0x41, 0xFD, 0x60, 0x95, 0xF8, 0x94, 0x40, 0x81, 0x77, 0x23, 0x11, 0xF0, 0x42, 0x2B, 0x01, 0xC0, 0x43, 0x23,
                0x40, 0x83, 0xEC, 0x01, 0x21, 0x97, 0xF1, 0xF7, 0x48, 0xE0, 0x50, 0x81, 0x60, 0xFF, 0x02, 0xC0, 0x52, 0x2B, 0x01, 0xC0, 0x53, 0x23, 0x50, 0x83, 0xEC, 0x01, 0x21, 0x97,
                0xF1, 0xF7, 0x66, 0x95, 0x41, 0x50, 0x99, 0xF7, 0x80, 0x81, 0x77, 0x23, 0x11, 0xF0, 0x83, 0x23, 0x01, 0xC0, 0x82, 0x2B, 0x80, 0x83, 0x1F, 0xBF, 0x5C, 0x96, 0x8D, 0x91,
                0x9C, 0x91, 0x5D, 0x97, 0x01, 0x97, 0xF1, 0xF7, 0x81, 0xE0, 0x90, 0xE0, 0xDF, 0x91, 0xCF, 0x91, 0x1F, 0x91, 0x08, 0x95, 0x08, 0x95, 0x20, 0x91, 0xD1, 0x00, 0x30, 0x91,
                0xD2, 0x00, 0x28, 0x17, 0x39, 0x07, 0x71, 0xF4, 0x90, 0x91, 0x8F, 0x00, 0x80, 0x91, 0x90, 0x00, 0x98, 0x17, 0x41, 0xF0, 0xE0, 0x91, 0x8F, 0x00, 0xF0, 0xE0, 0xEF, 0x56,
                0xFF, 0x4F, 0x80, 0x81, 0x90, 0xE0, 0x08, 0x95, 0x8F, 0xEF, 0x9F, 0xEF, 0x08, 0x95, 0x20, 0x91, 0xD1, 0x00, 0x30, 0x91, 0xD2, 0x00, 0x28, 0x17, 0x39, 0x07, 0x69, 0xF4,
                0x80, 0x91, 0x90, 0x00, 0x20, 0x91, 0x8F, 0x00, 0x90, 0xE0, 0x80, 0x5C, 0x9F, 0x4F, 0x82, 0x1B, 0x91, 0x09, 0x60, 0xE4, 0x70, 0xE0, 0xCD, 0xD2, 0x08, 0x95, 0x80, 0xE0,
                0x90, 0xE0, 0x08, 0x95, 0xE0, 0x91, 0xD1, 0x00, 0xF0, 0x91, 0xD2, 0x00, 0xE8, 0x17, 0xF9, 0x07, 0x69, 0xF4, 0xA3, 0x89, 0xB4, 0x89, 0x9C, 0x91, 0x85, 0x89, 0x80, 0x95,
                0x89, 0x23, 0x8C, 0x93, 0x10, 0x92, 0xD2, 0x00, 0x10, 0x92, 0xD1, 0x00, 0x81, 0xE0, 0x08, 0x95, 0x80, 0xE0, 0x08, 0x95, 0xCF, 0x93, 0xDF, 0x93, 0xEC, 0x01, 0x8A, 0x8D,
                0x9B, 0x8D, 0x89, 0x2B, 0xE9, 0xF0, 0x80, 0x91, 0xD1, 0x00, 0x90, 0x91, 0xD2, 0x00, 0x8C, 0x17, 0x9D, 0x07, 0xB1, 0xF0, 0x00, 0x97, 0x09, 0xF0, 0xD9, 0xDF, 0x8E, 0x8D,
                0x8E, 0x7F, 0x8E, 0x8F, 0x10, 0x92, 0x90, 0x00, 0x10, 0x92, 0x8F, 0x00, 0xD0, 0x93, 0xD2, 0x00, 0xC0, 0x93, 0xD1, 0x00, 0xEB, 0x89, 0xFC, 0x89, 0x80, 0x81, 0x9D, 0x89,
                0x89, 0x2B, 0x80, 0x83, 0x81, 0xE0, 0x01, 0xC0, 0x80, 0xE0, 0xDF, 0x91, 0xCF, 0x91, 0x08, 0x95, 0x1F, 0x92, 0x0F, 0x92, 0x0F, 0xB6, 0x0F, 0x92, 0x11, 0x24, 0x2F, 0x93,
                0x3F, 0x93, 0x4F, 0x93, 0x5F, 0x93, 0x6F, 0x93, 0x7F, 0x93, 0x8F, 0x93, 0x9F, 0x93, 0xAF, 0x93, 0xBF, 0x93, 0xEF, 0x93, 0xFF, 0x93, 0xE0, 0x91, 0xD1, 0x00, 0xF0, 0x91,
                0xD2, 0x00, 0x30, 0x97, 0x09, 0xF4, 0x51, 0xC0, 0x86, 0x8D, 0xA6, 0x85, 0xB7, 0x85, 0x95, 0x85, 0x81, 0xFF, 0x04, 0xC0, 0x8C, 0x91, 0x89, 0x23, 0x29, 0xF4, 0x47, 0xC0,
                0x8C, 0x91, 0x89, 0x23, 0x09, 0xF0, 0x43, 0xC0, 0xA3, 0x89, 0xB4, 0x89, 0x9C, 0x91, 0x85, 0x89, 0x80, 0x95, 0x89, 0x23, 0x8C, 0x93, 0x86, 0x89, 0x97, 0x89, 0x01, 0x97,
                0xF1, 0xF7, 0x20, 0x8D, 0x31, 0x8D, 0xA6, 0x85, 0xB7, 0x85, 0x75, 0x85, 0x58, 0xE0, 0x40, 0xE0, 0xC9, 0x01, 0x01, 0x97, 0xF1, 0xF7, 0x84, 0x2F, 0x90, 0xE0, 0x95, 0x95,
                0x87, 0x95, 0x48, 0x2F, 0x6C, 0x91, 0x67, 0x23, 0x09, 0xF0, 0x40, 0x68, 0x51, 0x50, 0x91, 0xF7, 0x86, 0x8D, 0x81, 0xFD, 0x40, 0x95, 0x80, 0x91, 0x90, 0x00, 0x90, 0xE0,
                0x01, 0x96, 0x8F, 0x73, 0x99, 0x27, 0x20, 0x91, 0x8F, 0x00, 0x28, 0x17, 0x49, 0xF0, 0xA0, 0x91, 0x90, 0x00, 0xB0, 0xE0, 0xAF, 0x56, 0xBF, 0x4F, 0x4C, 0x93, 0x80, 0x93,
                0x90, 0x00, 0x03, 0xC0, 0x86, 0x8D, 0x81, 0x60, 0x86, 0x8F, 0x82, 0x8D, 0x93, 0x8D, 0x01, 0x97, 0xF1, 0xF7, 0xA3, 0x89, 0xB4, 0x89, 0x8C, 0x91, 0x95, 0x89, 0x89, 0x2B,
                0x8C, 0x93, 0xFF, 0x91, 0xEF, 0x91, 0xBF, 0x91, 0xAF, 0x91, 0x9F, 0x91, 0x8F, 0x91, 0x7F, 0x91, 0x6F, 0x91, 0x5F, 0x91, 0x4F, 0x91, 0x3F, 0x91, 0x2F, 0x91, 0x0F, 0x90,
                0x0F, 0xBE, 0x0F, 0x90, 0x1F, 0x90, 0x18, 0x95, 0x24, 0xE6, 0x30, 0xE0, 0xFC, 0x01, 0x31, 0x83, 0x20, 0x83, 0x42, 0xCF, 0x1F, 0x93, 0xCF, 0x93, 0xDF, 0x93, 0xEC, 0x01,
                0x16, 0x2F, 0x6E, 0x8D, 0x66, 0x95, 0x61, 0x70, 0x81, 0xE0, 0x68, 0x27, 0x81, 0x2F, 0x25, 0xD1, 0x61, 0xE0, 0x81, 0x2F, 0xE9, 0xD0, 0x81, 0x2F, 0x90, 0xE0, 0xFC, 0x01,
                0xE8, 0x5D, 0xFF, 0x4F, 0xE4, 0x91, 0xE8, 0x8B, 0xFC, 0x01, 0xEE, 0x5C, 0xFF, 0x4F, 0xE4, 0x91, 0xF0, 0xE0, 0xEE, 0x0F, 0xFF, 0x1F, 0xEE, 0x5B, 0xFF, 0x4F, 0x85, 0x91,
                0x94, 0x91, 0x9A, 0x8B, 0x89, 0x8B, 0xDF, 0x91, 0xCF, 0x91, 0x1F, 0x91, 0x08, 0x95, 0x1F, 0x93, 0xCF, 0x93, 0xDF, 0x93, 0xEC, 0x01, 0x16, 0x2F, 0x60, 0xE0, 0x81, 0x2F,
                0xC9, 0xD0, 0x8E, 0x8D, 0x81, 0xFD, 0x03, 0xC0, 0x61, 0xE0, 0x81, 0x2F, 0xFC, 0xD0, 0x1C, 0x87, 0x81, 0x2F, 0x90, 0xE0, 0xFC, 0x01, 0xE8, 0x5D, 0xFF, 0x4F, 0xE4, 0x91,
                0xED, 0x87, 0xFC, 0x01, 0xEE, 0x5C, 0xFF, 0x4F, 0xE4, 0x91, 0xF0, 0xE0, 0xEE, 0x0F, 0xFF, 0x1F, 0xE4, 0x5C, 0xFF, 0x4F, 0x85, 0x91, 0x94, 0x91, 0x9F, 0x87, 0x8E, 0x87,
                0xDF, 0x91, 0xCF, 0x91, 0x1F, 0x91, 0x08, 0x95, 0xFF, 0x92, 0x0F, 0x93, 0x1F, 0x93, 0xCF, 0x93, 0xDF, 0x93, 0xEC, 0x01, 0xF6, 0x2E, 0x52, 0x2F, 0x1B, 0x82, 0x1A, 0x82,
                0x08, 0xEE, 0x13, 0xE0, 0x20, 0xE0, 0x30, 0xE0, 0x0C, 0x83, 0x1D, 0x83, 0x2E, 0x83, 0x3F, 0x83, 0x84, 0xE6, 0x90, 0xE0, 0x99, 0x83, 0x88, 0x83, 0x1F, 0x8A, 0x1E, 0x8A,
                0x19, 0x8E, 0x18, 0x8E, 0x1B, 0x8E, 0x1A, 0x8E, 0x1D, 0x8E, 0x1C, 0x8E, 0x8E, 0x8D, 0x8E, 0x7F, 0x50, 0xFB, 0x81, 0xF9, 0x8E, 0x8F, 0x64, 0x2F, 0xCE, 0x01, 0x8C, 0xDF,
                0x6F, 0x2D, 0xCE, 0x01, 0xDF, 0x91, 0xCF, 0x91, 0x1F, 0x91, 0x0F, 0x91, 0xFF, 0x90, 0xAB, 0xCF, 0xCF, 0x93, 0xDF, 0x93, 0xEC, 0x01, 0x9A, 0x01, 0xAB, 0x01, 0x1B, 0x8E,
                0x1A, 0x8E, 0x19, 0x8E, 0x18, 0x8E, 0x1F, 0x8A, 0x1E, 0x8A, 0x60, 0xE9, 0x70, 0xED, 0x83, 0xE0, 0x90, 0xE0, 0xB4, 0xD1, 0xC9, 0x01, 0x24, 0x30, 0x31, 0x05, 0x20, 0xF0,
                0xB9, 0x01, 0x63, 0x50, 0x71, 0x09, 0x02, 0xC0, 0x61, 0xE0, 0x70, 0xE0, 0x7D, 0x8F, 0x6C, 0x8F, 0x6C, 0x85, 0x65, 0x30, 0x08, 0xF0, 0x3E, 0xC0, 0xB9, 0x01, 0x76, 0x95,
                0x67, 0x95, 0x64, 0x31, 0x71, 0x05, 0x18, 0xF0, 0x63, 0x51, 0x71, 0x09, 0x02, 0xC0, 0x61, 0xE0, 0x70, 0xE0, 0x7F, 0x8B, 0x6E, 0x8B, 0x06, 0x97, 0x18, 0xF0, 0xC9, 0x01,
                0x05, 0x97, 0x02, 0xC0, 0x81, 0xE0, 0x90, 0xE0, 0x99, 0x8F, 0x88, 0x8F, 0xC9, 0x01, 0x63, 0xE0, 0x70, 0xE0, 0x44, 0xD1, 0x96, 0x95, 0x87, 0x95, 0x96, 0x95, 0x87, 0x95,
                0x8D, 0x30, 0x91, 0x05, 0x10, 0xF0, 0x0C, 0x97, 0x02, 0xC0, 0x81, 0xE0, 0x90, 0xE0, 0x9B, 0x8F, 0x8A, 0x8F, 0x8B, 0xB7, 0x80, 0x62, 0x8B, 0xBF, 0x2C, 0x85, 0x25, 0x30,
                0x18, 0xF4, 0x85, 0xE3, 0x90, 0xE0, 0x02, 0xC0, 0x80, 0xE0, 0x90, 0xE0, 0x9C, 0x8B, 0x8B, 0x8B, 0x81, 0xE0, 0x01, 0xC0, 0x88, 0x0F, 0x2A, 0x95, 0xEA, 0xF7, 0x8D, 0x8B,
                0x8C, 0x8D, 0x9D, 0x8D, 0x01, 0x97, 0xF1, 0xF7, 0xCE, 0x01, 0xDF, 0x91, 0xCF, 0x91, 0x7A, 0xCE, 0x82, 0x30, 0x81, 0xF0, 0x18, 0xF4, 0x81, 0x30, 0x51, 0xF0, 0x08, 0x95,
                0x83, 0x30, 0x19, 0xF0, 0x84, 0x30, 0x09, 0xF0, 0x08, 0x95, 0x8C, 0xB5, 0x8F, 0x7D, 0x8C, 0xBD, 0x08, 0x95, 0x8A, 0xB5, 0x8F, 0x77, 0x02, 0xC0, 0x8A, 0xB5, 0x8F, 0x7D,
                0x8A, 0xBD, 0x08, 0x95, 0x8C, 0xB5, 0x80, 0x64, 0x8C, 0xBD, 0x08, 0x95, 0xCF, 0x93, 0xDF, 0x93, 0x90, 0xE0, 0xFC, 0x01, 0xE8, 0x5D, 0xFF, 0x4F, 0x24, 0x91, 0xFC, 0x01,
                0xEE, 0x5C, 0xFF, 0x4F, 0x84, 0x91, 0x88, 0x23, 0x49, 0xF1, 0x90, 0xE0, 0x88, 0x0F, 0x99, 0x1F, 0xFC, 0x01, 0xE8, 0x5B, 0xFF, 0x4F, 0xA5, 0x91, 0xB4, 0x91, 0x8E, 0x5B,
                0x9F, 0x4F, 0xFC, 0x01, 0xC5, 0x91, 0xD4, 0x91, 0x9F, 0xB7, 0x61, 0x11, 0x08, 0xC0, 0xF8, 0x94, 0x8C, 0x91, 0x20, 0x95, 0x82, 0x23, 0x8C, 0x93, 0x88, 0x81, 0x82, 0x23,
                0x0A, 0xC0, 0x62, 0x30, 0x51, 0xF4, 0xF8, 0x94, 0x8C, 0x91, 0x32, 0x2F, 0x30, 0x95, 0x83, 0x23, 0x8C, 0x93, 0x88, 0x81, 0x82, 0x2B, 0x88, 0x83, 0x04, 0xC0, 0xF8, 0x94,
                0x8C, 0x91, 0x82, 0x2B, 0x8C, 0x93, 0x9F, 0xBF, 0xDF, 0x91, 0xCF, 0x91, 0x08, 0x95, 0x0F, 0x93, 0x1F, 0x93, 0xCF, 0x93, 0xDF, 0x93, 0x1F, 0x92, 0xCD, 0xB7, 0xDE, 0xB7,
                0x28, 0x2F, 0x30, 0xE0, 0xF9, 0x01, 0xE2, 0x5E, 0xFF, 0x4F, 0x84, 0x91, 0xF9, 0x01, 0xE8, 0x5D, 0xFF, 0x4F, 0x14, 0x91, 0xF9, 0x01, 0xEE, 0x5C, 0xFF, 0x4F, 0x04, 0x91,
                0x00, 0x23, 0xC1, 0xF0, 0x88, 0x23, 0x19, 0xF0, 0x69, 0x83, 0x92, 0xDF, 0x69, 0x81, 0xE0, 0x2F, 0xF0, 0xE0, 0xEE, 0x0F, 0xFF, 0x1F, 0xEE, 0x5B, 0xFF, 0x4F, 0xA5, 0x91,
                0xB4, 0x91, 0x9F, 0xB7, 0xF8, 0x94, 0x8C, 0x91, 0x61, 0x11, 0x03, 0xC0, 0x10, 0x95, 0x81, 0x23, 0x01, 0xC0, 0x81, 0x2B, 0x8C, 0x93, 0x9F, 0xBF, 0x0F, 0x90, 0xDF, 0x91,
                0xCF, 0x91, 0x1F, 0x91, 0x0F, 0x91, 0x08, 0x95, 0xCF, 0x92, 0xDF, 0x92, 0xEF, 0x92, 0xFF, 0x92, 0x0F, 0x93, 0x1F, 0x93, 0xCF, 0x93, 0xDF, 0x93, 0x7C, 0x01, 0x6A, 0x01,
                0xEB, 0x01, 0x00, 0xE0, 0x10, 0xE0, 0x0C, 0x15, 0x1D, 0x05, 0x71, 0xF0, 0x69, 0x91, 0xD7, 0x01, 0xED, 0x91, 0xFC, 0x91, 0x01, 0x90, 0xF0, 0x81, 0xE0, 0x2D, 0xC7, 0x01,
                0x09, 0x95, 0x89, 0x2B, 0x19, 0xF0, 0x0F, 0x5F, 0x1F, 0x4F, 0xEF, 0xCF, 0xC8, 0x01, 0xDF, 0x91, 0xCF, 0x91, 0x1F, 0x91, 0x0F, 0x91, 0xFF, 0x90, 0xEF, 0x90, 0xDF, 0x90,
                0xCF, 0x90, 0x08, 0x95, 0x53, 0xD0, 0x64, 0xDF, 0xF7, 0xDC, 0xC0, 0xE0, 0xD0, 0xE0, 0x01, 0xDD, 0x20, 0x97, 0xE9, 0xF3, 0xA2, 0xDC, 0xFB, 0xCF, 0x1F, 0x92, 0x0F, 0x92,
                0x0F, 0xB6, 0x0F, 0x92, 0x11, 0x24, 0x2F, 0x93, 0x3F, 0x93, 0x8F, 0x93, 0x9F, 0x93, 0xAF, 0x93, 0xBF, 0x93, 0x80, 0x91, 0xD4, 0x00, 0x90, 0x91, 0xD5, 0x00, 0xA0, 0x91,
                0xD6, 0x00, 0xB0, 0x91, 0xD7, 0x00, 0x30, 0x91, 0xD3, 0x00, 0x20, 0xE3, 0x23, 0x0F, 0x2D, 0x37, 0x20, 0xF4, 0x40, 0x96, 0xA1, 0x1D, 0xB1, 0x1D, 0x05, 0xC0, 0x23, 0xEB,
                0x23, 0x0F, 0x41, 0x96, 0xA1, 0x1D, 0xB1, 0x1D, 0x20, 0x93, 0xD3, 0x00, 0x80, 0x93, 0xD4, 0x00, 0x90, 0x93, 0xD5, 0x00, 0xA0, 0x93, 0xD6, 0x00, 0xB0, 0x93, 0xD7, 0x00,
                0x80, 0x91, 0xD8, 0x00, 0x90, 0x91, 0xD9, 0x00, 0xA0, 0x91, 0xDA, 0x00, 0xB0, 0x91, 0xDB, 0x00, 0x01, 0x96, 0xA1, 0x1D, 0xB1, 0x1D, 0x80, 0x93, 0xD8, 0x00, 0x90, 0x93,
                0xD9, 0x00, 0xA0, 0x93, 0xDA, 0x00, 0xB0, 0x93, 0xDB, 0x00, 0xBF, 0x91, 0xAF, 0x91, 0x9F, 0x91, 0x8F, 0x91, 0x3F, 0x91, 0x2F, 0x91, 0x0F, 0x90, 0x0F, 0xBE, 0x0F, 0x90,
                0x1F, 0x90, 0x18, 0x95, 0x78, 0x94, 0x8A, 0xB5, 0x82, 0x60, 0x8A, 0xBD, 0x8A, 0xB5, 0x81, 0x60, 0x8A, 0xBD, 0x83, 0xB7, 0x82, 0x60, 0x83, 0xBF, 0x83, 0xB7, 0x81, 0x60,
                0x83, 0xBF, 0x89, 0xB7, 0x82, 0x60, 0x89, 0xBF, 0x80, 0xB7, 0x82, 0x60, 0x80, 0xBF, 0x32, 0x98, 0x31, 0x9A, 0x30, 0x9A, 0x37, 0x9A, 0x08, 0x95, 0x00, 0x24, 0x55, 0x27,
                0x04, 0xC0, 0x08, 0x0E, 0x59, 0x1F, 0x88, 0x0F, 0x99, 0x1F, 0x00, 0x97, 0x29, 0xF0, 0x76, 0x95, 0x67, 0x95, 0xB8, 0xF3, 0x71, 0x05, 0xB9, 0xF7, 0x80, 0x2D, 0x95, 0x2F,
                0x08, 0x95, 0x97, 0xFB, 0x07, 0x2E, 0x16, 0xF4, 0x00, 0x94, 0x06, 0xD0, 0x77, 0xFD, 0x08, 0xD0, 0x49, 0xD0, 0x07, 0xFC, 0x05, 0xD0, 0x3E, 0xF4, 0x90, 0x95, 0x81, 0x95,
                0x9F, 0x4F, 0x08, 0x95, 0x70, 0x95, 0x61, 0x95, 0x7F, 0x4F, 0x08, 0x95, 0xA1, 0xE2, 0x1A, 0x2E, 0xAA, 0x1B, 0xBB, 0x1B, 0xFD, 0x01, 0x0D, 0xC0, 0xAA, 0x1F, 0xBB, 0x1F,
                0xEE, 0x1F, 0xFF, 0x1F, 0xA2, 0x17, 0xB3, 0x07, 0xE4, 0x07, 0xF5, 0x07, 0x20, 0xF0, 0xA2, 0x1B, 0xB3, 0x0B, 0xE4, 0x0B, 0xF5, 0x0B, 0x66, 0x1F, 0x77, 0x1F, 0x88, 0x1F,
                0x99, 0x1F, 0x1A, 0x94, 0x69, 0xF7, 0x60, 0x95, 0x70, 0x95, 0x80, 0x95, 0x90, 0x95, 0x9B, 0x01, 0xAC, 0x01, 0xBD, 0x01, 0xCF, 0x01, 0x08, 0x95, 0x05, 0x2E, 0x97, 0xFB,
                0x16, 0xF4, 0x00, 0x94, 0x0F, 0xD0, 0x57, 0xFD, 0x05, 0xD0, 0xD6, 0xDF, 0x07, 0xFC, 0x02, 0xD0, 0x46, 0xF4, 0x08, 0xC0, 0x50, 0x95, 0x40, 0x95, 0x30, 0x95, 0x21, 0x95,
                0x3F, 0x4F, 0x4F, 0x4F, 0x5F, 0x4F, 0x08, 0x95, 0x90, 0x95, 0x80, 0x95, 0x70, 0x95, 0x61, 0x95, 0x7F, 0x4F, 0x8F, 0x4F, 0x9F, 0x4F, 0x08, 0x95, 0xAA, 0x1B, 0xBB, 0x1B,
                0x51, 0xE1, 0x07, 0xC0, 0xAA, 0x1F, 0xBB, 0x1F, 0xA6, 0x17, 0xB7, 0x07, 0x10, 0xF0, 0xA6, 0x1B, 0xB7, 0x0B, 0x88, 0x1F, 0x99, 0x1F, 0x5A, 0x95, 0xA9, 0xF7, 0x80, 0x95,
                0x90, 0x95, 0xBC, 0x01, 0xCD, 0x01, 0x08, 0x95, 0xEE, 0x0F, 0xFF, 0x1F, 0x05, 0x90, 0xF4, 0x91, 0xE0, 0x2D, 0x09, 0x94, 0x10, 0xE0, 0xC0, 0xE5, 0xD0, 0xE0, 0x03, 0xC0,
                0xFE, 0x01, 0xF6, 0xDF, 0x22, 0x96, 0xC2, 0x35, 0xD1, 0x07, 0xD1, 0xF7, 0xF8, 0x94, 0xFF, 0xCF, 0x00, 0x00, 0x00, 0x00, 0x87, 0x00, 0x2D, 0x03, 0xF2, 0x00, 0x66, 0x00,
                0xDA, 0x00, 0xD9, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };


            


            IntelHexFile intelHexFile = new IntelHexFile();
            Stream stream = null;
            try
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///IntelHexFileTest1.hex"));
                
                stream = await file.OpenStreamForReadAsync();
                //Debug.WriteLine(stream.ReadByte());
            } catch
            {
                Debug.WriteLine(Package.Current.InstalledLocation.Path);
            }

            byte[] intelHexFileArray = null;

            if(stream != null)
            {
                intelHexFileArray = intelHexFile.intelHexFileToArray(stream, 128);
            }



            /*
            StorageFile sampleFile = null;
            try
            {
                sampleFile = await storageFolder.CreateFileAsync("output.txt");
            } catch
            {
                Debug.WriteLine("Couldn't open file");
            }

            try
            {
                Debug.WriteLine(sampleFile.Path);
                int fileLengthCounter = 0;
                int lineIndexCounter = 0;
                //while (fileLengthCounter < intelHexFileArray.Length)
                //{

                    //while (lineIndexCounter < 16)//&& fileLengthCounter < intelHexFileArray.Length)
                    //{
                        //await FileIO.AppendTextAsync(sampleFile, "0x");
                        //await FileIO.AppendTextAsync(sampleFile, intelHexFileArray[fileLengthCounter].ToString("X2"));
                        //await FileIO.AppendTextAsync(sampleFile, ", ");
                        //fileLengthCounter++;
                        //lineIndexCounter++;
                    //}
                    //await FileIO.AppendTextAsync(sampleFile, "\r\n");
                    //lineIndexCounter = 0;
                //}


                Debug.WriteLine(" END HEX FILE");
            } catch
            {
                Debug.WriteLine("Exception in printing IntelHexFileArray");
            }
            Debug.WriteLine("The Length of the Should Be Data: " + intelHexFileTestDataByteArray.Length);
            Debug.WriteLine("The Length of the Extracted Data: " + intelHexFileArray.Length);
            Debug.WriteLine("The Length of the Should Be First Byte: " + intelHexFileTestDataByteArray[0]);
            Debug.WriteLine("The Length of the Extracted First Byte: " + intelHexFileArray[0]);
            Debug.WriteLine("The Length of the Should Last Byte: " + intelHexFileTestDataByteArray[intelHexFileTestDataByteArray.Length -121]);
            Debug.WriteLine("The Length of the Extracted Last Byte: " + intelHexFileArray[intelHexFileArray.Length-121]);

            for(int i = 0; i < intelHexFileArray.Length; i++)
            {
                if(intelHexFileArray[i] != intelHexFileTestDataByteArray[i])
                {
                    Debug.WriteLine("Failed at index: " + i);
                }
                
            }
            // SO Help: http://stackoverflow.com/questions/1375166/why-does-assert-areequalt-obj1-tobj2-fail-with-identical-byte-arrays
            */

            ICollection shouldBeData = intelHexFileTestDataByteArray;
            ICollection data = intelHexFileArray;

            CollectionAssert.AreEqual(shouldBeData, data);
            return;
        }
        public async Task tester()
        {
            var a = 1 + 2;
            Assert.AreEqual(a, 3);
            return;
        }
    }
}
