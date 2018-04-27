//#define DEBUG_NET

#if DEBUG_NET
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.IO;

namespace ML.Net
{
    /// <summary>
    /// 用字节数组实现的一个先进先出的环形BUFFER
    /// </summary>
    public sealed class CirculeBuffer
    {
        private const int INITCAPACITY = 8 * 1024; // 默认初始容量
        private const int INCREMENTSIZE = 2 * 1024;// 自动扩展时一次最少扩展多少字节

        private object mSyncObj = new object(); //用于加锁

        private byte[] mBuffer;    // 内部缓冲区
        private int mCurrentCapacity;  // 缓冲区的容量
        private int mLength;     // 缓冲区中当前有效数据的长度
        private bool mExpandable;  //是否可自动扩展容量
        private int mUppderCapacity;  //可扩展到的最大容量-1会让无限扩展

        private int mReadPos;         // 读指针的偏移位置（指向流中第一个有效字节）
        private int mWritePos;         //写指针的偏移位置（指向流中最后一个有效字节）

        public int WritePos
        {
            get
            {
                return mWritePos;
            }
        }

        public int ReadPos
        {
            get
            {
                return mReadPos;
            }
        }

#if DEBUG_NET
        public void PrintBufferState()
        {
            Logger.Log(string.Format("network! CirculeBuffer State, Init Cap: {0}, Incre: {1},  CurCap: {2}, Expandable: {3}, Upper: {4}, CurLen: {5}, ReadPos: {6}, WritePos: {7}", INITCAPACITY, INCREMENTSIZE, mCurrentCapacity, mExpandable, mUppderCapacity, mLength, mReadPos, mWritePos));

            Logger.Log("network! Data In Buffer: ");

            const int rowLen = 16;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(512);

            byte[] bb = new byte[this.Length];
            this.Peek(bb, 0, this.Length);
            for(int i = 0; i < bb.Length; i++)
            {
                sb.AppendFormat("{0:X2} ", bb[i]);
                if((i + 1) % rowLen == 0)
                {
                    sb.AppendLine();
                }
            }
            Logger.Log(sb.ToString());
        }
#endif

        public int CurrentCapacity
        {
            get
            {
                return mCurrentCapacity;
            }
        }

        public CirculeBuffer()
            : this(INITCAPACITY)
        {

        }

        public CirculeBuffer(int capacity)
            : this(capacity, true)
        {

        }

        public CirculeBuffer(int capacity, bool expandable)
            : this(capacity, expandable, -1)
        {

        }

        public CirculeBuffer(int capacity, bool expandable, int maxCapacity)
        {
            if (capacity < 0)
            {
                mCurrentCapacity = INITCAPACITY;
            }
            else
            {
                mCurrentCapacity = capacity;
            }

            if (expandable && (maxCapacity != -1 && maxCapacity < capacity))
            {
                mExpandable = false;
                mUppderCapacity = -1;
            }
            else
            {
                mExpandable = expandable;
                mUppderCapacity = maxCapacity;
            }
            mLength = 0;
            mBuffer = new byte[mCurrentCapacity];
            mReadPos = 0;
            mWritePos = 0;
        }

        public int Length
        {
            get
            {
                lock (mSyncObj)
                {
                    return mLength;
                }
            }
        }

        //TODO: 组合数据保证顺序
        public byte[] GetBuffer()
        {
            return mBuffer;
        }

        public void Reset()
        {
            mLength = 0;
            mReadPos = 0;
            mWritePos = 0;

#if _DEBUG
            UnityEngine.Debug.LogWarning("Reset the CirculeBuffer..!");
#endif
            //      Array.Clear(mBuffer, 0, mBuffer.Length);
        }

        public int Peek(byte[] dest, int offset, int count)
        {
            if (dest == null || offset < 0
                || count < 0
                || (dest.Length - offset) < count)
            {
                return 0;
            }

            lock (mSyncObj)
            {
                // 真正要读取的字节数
                int readLen = Math.Min(mLength, count);
                if (readLen == 0)
                {
                    return 0;
                }

                if (mReadPos < mWritePos)
                {
                    Buffer.BlockCopy(mBuffer, mReadPos, dest, offset, count);
                }
                else
                {
                    int afterReadPosLen = mCurrentCapacity - mReadPos;
                    if (afterReadPosLen >= count)
                    {
                        Buffer.BlockCopy(mBuffer, mReadPos, dest, offset, count);
                    }
                    else
                    {
                        Buffer.BlockCopy(mBuffer, mReadPos, dest, offset, afterReadPosLen);
                        int restLen = count - afterReadPosLen;
                        Buffer.BlockCopy(mBuffer, 0, dest, offset + afterReadPosLen, restLen);
                    }
                }

                return readLen;
            }
        }

        /// <summary>
        /// 从Buffer读取数据
        /// </summary>
        /// <param name="buffer">包含所读取到的字节。</param>
        /// <param name="offset">buffer 中的字节偏移量。</param>
        /// <param name="count">最多读取的字节数。</param>
        /// <returns>成功读取到的总字节数。可能为0</returns>
        public int Read(byte[] dest, int offset, int count)
        {
            if (dest == null || offset < 0
                || count < 0
                || (dest.Length - offset) < count)
            {
                return 0;
            }

            lock (mSyncObj)
            {
                // 真正要读取的字节数
                int readLen = Math.Min(mLength, count);
                if (readLen == 0)
                {
                    return 0;
                }

                ReadInternal(dest, offset, readLen);

                return readLen;
            }
        }

        /// <summary>
        /// 写入BUFFER
        /// </summary>
        /// <param name="buffer">源数据。</param>
        /// <param name="offset">偏移</param>
        /// <param name="count">字节数。</param>
        public int Write(byte[] src, int offset, int count)
        {
            if (src == null || offset < 0
                         || count < 0
                         || (src.Length - offset) < count)
            {
                return 0;
            }

            lock (mSyncObj)
            {
                // 要往流中写入 buffer 中的数据，流的容量至少要是这么多
                int minCapacityNeeded = Length + count;

                // 如果需要扩展流则扩展流
                ExpandBufferIfNeed(minCapacityNeeded);
                // 如果无法再容纳下指定的字节数
                if (minCapacityNeeded <= mCurrentCapacity)
                {
                    this.WriteInternal(src, offset, count);
                }
                else
                {
                    throw new IndexOutOfRangeException("Buffer full, can't write any more.");
                }

                return count;
            }
        }

        public bool IsFull()
        {
            return mLength == mCurrentCapacity;
        }

        public bool IsEmpty()
        {
            return mLength == 0;
        }

        private void WriteInternal(byte[] src, int offset, int count)
        {
            if (mReadPos > mWritePos)
            {
                Buffer.BlockCopy(src, offset, mBuffer, mWritePos, count);
            }
            else
            {
                int afterWritePosLen = mCurrentCapacity - mWritePos;
                if (afterWritePosLen >= count)
                {
                    Buffer.BlockCopy(src, offset, mBuffer, mWritePos, count);
                }
                else
                {
                    Buffer.BlockCopy(src, offset, mBuffer, mWritePos, afterWritePosLen);
                    int restLen = count - afterWritePosLen;
                    Buffer.BlockCopy(src, offset + afterWritePosLen, mBuffer, 0, restLen);
                }
            }

            mWritePos += count;
            mWritePos %= mCurrentCapacity;
            mLength += count;
#if DEBUG_NET
            Logger.Log("After Write Buffer of Data: " + count);
            PrintBufferState();
#endif
        }

        private void ReadInternal(byte[] buffer, int offset, int count)
        {
            if (mReadPos < mWritePos)
            {
                Buffer.BlockCopy(mBuffer, mReadPos, buffer, offset, count);
            }
            else
            {
                int afterReadPosLen = mCurrentCapacity - mReadPos;

                if (afterReadPosLen >= count)
                {
                    Buffer.BlockCopy(mBuffer, mReadPos, buffer, offset, count);
                }
                else
                {
                    Buffer.BlockCopy(mBuffer, mReadPos, buffer, offset, afterReadPosLen);
                    int restLen = count - afterReadPosLen;
                    Buffer.BlockCopy(mBuffer, 0, buffer, offset + afterReadPosLen, restLen);
                }
            }

            mReadPos += count;
            mReadPos %= mCurrentCapacity;
            mLength -= count;

            //重置读取，判断更方便
            if (mLength == 0)
            {
                mReadPos = mWritePos = 0;
            }

#if DEBUG_NET
            Logger.Log("network! After Read Buffer of Data: " + count);
            PrintBufferState();
#endif
        }

        //如果频繁地扩展缓冲区或者要复制的数据量较大时性能可能比较低。
        private void ExpandBufferIfNeed(int minSize)
        {
            if (!mExpandable)
            {
                return;
            }

            // 不需要扩展
            if (mCurrentCapacity >= minSize)
            {
                return;
            }

            // 无法扩展
            if (mUppderCapacity != -1 && (mUppderCapacity - mCurrentCapacity) < INCREMENTSIZE)
            {
                return;
            }

            // 计算要扩展几块（INCREMENTSIZE 的倍数）
            int blocksNum = (int)Math.Ceiling((double)(minSize - mCurrentCapacity) / INCREMENTSIZE);

            byte[] buffNew = new byte[mCurrentCapacity + blocksNum * INCREMENTSIZE];

#if DEBUG_NET
            Logger.Log("Expand Buffer Of Blocks: " + blocksNum);
#endif
            ReadInternal(buffNew, 0, Length);
            mBuffer = buffNew;
            mReadPos = 0;
            mWritePos = Length;
            mCurrentCapacity = buffNew.Length;
        }



    }//end of class.




}
