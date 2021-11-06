﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Vectoreyes
{
    // Slightly modified from https://stackoverflow.com/a/34801225/14226597
    internal class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; }
        public int[] Bits { get; }
        public bool Disposed { get; private set; }
        public int Height { get; }
        public int Width { get; }

        protected GCHandle BitsHandle { get; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new int[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, Color color)
        {
            var index = x + y * Width;
            var col = color.ToArgb();

            Bits[index] = col;
        }

        public Color GetPixel(int x, int y)
        {
            var index = x + y * Width;
            var col = Bits[index];
            var result = Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}