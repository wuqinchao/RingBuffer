using System;

namespace Common
{
    /// <summary>
    /// C#版环形缓冲区（不处理线程安全）
    /// </summary>
    /// <remarks>一次读写数据不能超出缓冲区总大小</remarks>
    public class RingBuffer
    {
        /// <summary>
        /// 数据缓存区
        /// </summary>
        protected byte[] _buffer;
        /// <summary>
        /// 当前已缓存数据总数
        /// </summary>
        protected int _size = 0;
        /// <summary>
        /// 缓存数据头索引
        /// </summary>
        protected int _head = 0;
        /// <summary>
        /// 缓存数据尾索引
        /// </summary>
        protected int _tail = 0;
        /// <summary>
        /// 缓存总大小
        /// </summary>
        public int BufferSize { get { return _buffer.Length; } }
        /// <summary>
        /// 已缓存数据数量
        /// </summary>
        public int Count { get { return _size; } }
        public RingBuffer(int capacity = 4096)
        {
            _buffer = new byte[capacity];
        }
        /// <summary>
        /// 缓存区是否没有缓存数据
        /// </summary>
        public bool IsEmpty
        {
            get { return _size == 0; }
        }
        /// <summary>
        /// 缓存区是否已满
        /// </summary>
        public bool IsFull
        {
            get { return _size == BufferSize; }
        }
        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            Array.Clear(_buffer, 0, BufferSize);
            _head = 0;
            _tail = 0;
            _size = 0;
        }
        /// <summary>
        /// 获取实际索引
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int InternalIndex(int index)
        {
            return _head + (index < (BufferSize - _head) ? index : index - BufferSize);
        }
        public byte this[int index]
        {
            get
            {
                if (index >= _size)
                {
                    throw new Exception("数组下标超限");
                }
                int actualIndex = InternalIndex(index);
                return _buffer[actualIndex];
            }
        }
        /// <summary>
        /// 缓存多个数据
        /// </summary>
        /// <param name="inputs">数据源</param>
        /// <param name="offset">要缓存数据在数据源中的起始索引</param>
        /// <param name="count">要缓存数据长度</param>
        /// <exception cref="Exception">缓存溢出</exception>
        public void Write(byte[] inputs, int offset, int count)
        {
            if (count + _size > BufferSize)
            {
                throw new Exception("缓存溢出");
            }
            if (_tail + count <= BufferSize)
            {
                Buffer.BlockCopy(inputs, offset, _buffer, _tail, count);
                _tail = (_tail + count) % BufferSize;
            }
            else
            {
                var fcount = BufferSize - _tail;
                Buffer.BlockCopy(inputs, offset, _buffer, _tail, fcount);
                _tail = (_tail + fcount) % BufferSize;
                var scount = count - fcount;
                Buffer.BlockCopy(inputs, offset + fcount, _buffer, _tail, scount);
                _tail = (_tail + scount) % BufferSize;
            }
            _size += count;
        }
        /// <summary>
        /// 写单个数据
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="Exception"></exception>
        public void Write(byte input)
        {
            if (1 + _size > BufferSize)
            {
                throw new Exception("缓存溢出");
            }
            _buffer[_tail] = input;
            _tail = (_tail + 1) % BufferSize;
            ++_size;
        }
        /// <summary>
        /// 预读指定数量数据, 但指针不变
        /// </summary>
        /// <param name="count">要读取的数量</param>
        /// <returns></returns>
        public byte[] Peek(int count, int offset = 0)
        {
            byte[] _out;
            if (count + offset > _size)
                return null;
            _out = new byte[count];

            if (_head + offset + count <= BufferSize)
            {
                Buffer.BlockCopy(_buffer, _head + offset, _out, 0, count);
            }
            else
            {
                var fcount = BufferSize - _head - offset;
                Buffer.BlockCopy(_buffer, _head + offset, _out, 0, fcount);
                var scount = count - fcount;
                Buffer.BlockCopy(_buffer, (_head + offset + fcount) % BufferSize, _out, fcount, scount);
            }

            return _out;
        }
        /// <summary>
        /// 向前移动读指针
        /// </summary>
        /// <param name="count"></param>
        public void Seek(int count)
        {
            if (_head + count <= BufferSize)
            {
                _head = (_head + count) % BufferSize;
            }
            else
            {
                var fcount = BufferSize - _head;
                _head = (_head + fcount) % BufferSize;
                var scount = count - fcount;
                _head = (_head + scount) % BufferSize;
            }
            _size -= count;
        }
        /// <summary>
        /// 读取指定数量数据，指针相应移动
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] Read(int count)
        {
            byte[] _out;
            if (count > _size)
                count = _size;
            _out = new byte[count];

            if (_head + count <= BufferSize)
            {
                Buffer.BlockCopy(_buffer, _head, _out, 0, count);
                _head = (_head + count) % BufferSize;
            }
            else
            {
                var fcount = BufferSize - _head;
                Buffer.BlockCopy(_buffer, _head, _out, 0, fcount);
                _head = (_head + fcount) % BufferSize;
                var scount = count - fcount;
                Buffer.BlockCopy(_buffer, _head, _out, fcount, scount);
                _head = (_head + scount) % BufferSize;
            }
            _size -= count;

            return _out;
        }
        /// <summary>
        /// 获取全部缓存数据，各指针不变
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            byte[] _out;
            _out = new byte[_size];
            if (_head + _size <= BufferSize)
            {
                Buffer.BlockCopy(_buffer, _head, _out, 0, _size);
            }
            else
            {
                var fcount = BufferSize - _head;
                Buffer.BlockCopy(_buffer, _head, _out, 0, fcount);
                var scount = _size - fcount;
                Buffer.BlockCopy(_buffer, (_head + fcount) % BufferSize, _out, fcount, scount);
            }
            return _out;
        }
    }
}
