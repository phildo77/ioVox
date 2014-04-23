using System.Collections.Generic;

namespace ioSS
{
    public class CDataArray3D
    {

        //Internal Properties ---------------------------------------------------------------
        private List<CompData> DataBlock;
        readonly public IVector3 Dims;

        //Internal structs ------------------------------------------------------------------
        
        /// <summary>
        /// CompData -- Compressed Data.  
        /// Struct used to represent data in the CDataArray.
        /// Contains binary data and repeat count.  
        /// </summary>
        private struct CompData
        {
            public int count;
            public ushort Data;

            public CompData(ushort _data, int _count)
            {
                count = _count;
                Data = _data;
            }
        }

        //Constructor ------------------------------------------------------------------------

        public CDataArray3D(int _x, int _y, int _z)
        {

            Dims = new IVector3(_x, _y, _z);
            DataBlock = new List<CompData>();
            DataBlock.Add(new CompData(0, Dims.x * Dims.y * Dims.z));
        }

        public CDataArray3D(IVector3 _size)
        {

            Dims = _size;
            DataBlock = new List<CompData>();
            DataBlock.Add(new CompData(0, Dims.x * Dims.y * Dims.z));
        }

        /// <summary>
        /// Returns binary data at specified coordinate.
        /// </summary>
        /// <param name="_x">X coordinate</param>
        /// <param name="_y">Y coordinate</param>
        /// <param name="_z">Z coordinate</param>
        /// <returns></returns>
        public ushort? this[int _x, int _y, int _z]
        {
            get {
                if (!CoordInRange(new IVector3(_x, _y, _z)))
                {
                    return null;
                }

                return GetData(_x, _y, _z).Data; 
            }


            set { Update((ushort)value, new IVector3(_x, _y, _z)); }

        }

        /// <summary>
        /// Returns binary data at specified coordinate
        /// </summary>
        /// <param name="_coord">Coordinate of binary data to be returned</param>
        /// <returns></returns>
        public ushort? this[IVector3 _coord]
        {
            get
            {
                if (!CoordInRange(_coord))
                {
                    return null;
                }

                return GetData(_coord).Data;
            }

            set { Update((ushort)value, _coord); }

        }

        //Internal Methods ---------------------------------------------------------------------

        /// <summary>
        /// Returns true if _pos coordinate is within dimensions and false if not.
        /// </summary>
        /// <param name="_pos"></param>
        /// <returns></returns>
        private bool CoordInRange(IVector3 _pos)
        {
            if (_pos.x >= Dims.x || _pos.y >= Dims.y || _pos.z >= Dims.z ||
				_pos.x < 0 || _pos.y < 0 || _pos.z < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Inspects data strips around the specified index and combines them into one strip if the data is the same.
        /// </summary>
        /// <param name="_index"></param>
        private void Meld(int _index, MeldDir _dir)
        {
			int index = _index;
            if (index != 0 && (_dir == MeldDir.Front || _dir == MeldDir.Both))
            {
                //Meld to prior index if possible
                if (DataBlock[index].Data == DataBlock[index - 1].Data)
                {
                    int newCount = DataBlock[index].count + DataBlock[index - 1].count;
                    DataBlock[index - 1] = new CompData(DataBlock[index].Data, newCount);
                    DataBlock.RemoveAt(index);
					index--;
                }
            }

            if (index != DataBlock.Count - 1 && (_dir == MeldDir.Back || _dir == MeldDir.Both))
            {
                //Meld to following index if possible
                if (DataBlock[index].Data == DataBlock[index + 1].Data)
                {
                    int newCount = DataBlock[index].count + DataBlock[index + 1].count;
                    DataBlock[index] = new CompData(DataBlock[index].Data, newCount);
                    DataBlock.RemoveAt(index + 1);
                }
            }

        }
        enum MeldDir : byte
        {
            Both,
            Front,
            Back
        }

        /// <summary>
        /// Get Data from specified position.
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        /// <returns></returns>
        private CompData GetData(int _x, int _y, int _z)
        {
            int junk1, junk2;
            return GetData(new IVector3(_x, _y, _z), out junk1, out junk2);
        }
        private CompData GetData(IVector3 _coord)
        {
            int junk1, junk2;
            return GetData(_coord, out junk1, out junk2);
        }
        private CompData GetData(IVector3 _coord, out int _index, out int _stripPos)
        {
            //Check that coords are valid
            if (CoordInRange(_coord))
            {
                int curBlkLinear = 0;
                int trgBlkLinear = _coord.x + _coord.y * Dims.x + _coord.z * Dims.y * Dims.x;

                if (trgBlkLinear == 0)
                {
                    _index = 0;
                    _stripPos = 0;
                    return DataBlock[_index];
                }

                _index = 0;
                while (true)
                {

                    curBlkLinear += DataBlock[_index].count;
                    if (curBlkLinear > trgBlkLinear)
                    {
                        curBlkLinear -= DataBlock[_index].count;
                        _stripPos = trgBlkLinear - curBlkLinear;
                        break;
                    }
                    _index++;
                }

                return DataBlock[_index];
            }
            else
            {
                throw new System.ArgumentException("Coords (" + _coord.x + ", " + _coord.y + ", " + _coord.z + ") out of range.  Dims (" + Dims.x + ", " + Dims.y + ", " + Dims.z + ")", "x, y, z");
                
            }
        }




        //Public methods --------------------------------------------------------------------
        public struct SlicedData {
            public IVector3 BeginCoord;
            public IVector3 EndCoord;
            public ushort Data;
        }

        public SlicedData GetSliceAt(IVector3 _coord)
        {
            SlicedData slice = new SlicedData();

            int junk, stripPos;

            CompData curData = GetData(_coord, out junk, out stripPos);
            slice.Data = curData.Data;

            if (_coord.x <= stripPos)
            {
                slice.BeginCoord = new IVector3(0, _coord.y, _coord.z);
            }
            else
            {
                slice.BeginCoord = new IVector3(_coord.x - stripPos, _coord.y, _coord.z);
            }

            if (_coord.x + curData.count >= Dims.x)
            {
                slice.EndCoord = new IVector3(Dims.x - 1, _coord.y, _coord.z);
            }
            else
            {
                slice.EndCoord = new IVector3(_coord.x + curData.count, _coord.y, _coord.z);
            }
            return slice;
        }

        public List<SlicedData> GetSlicesIn(IVector3 _root, IVector3 _size)
        {
            
            List<SlicedData> SliceList = new List<SlicedData>();
            if (!CoordInRange(_root)) return SliceList;

            for (int z = _root.z; z < _root.z + _size.z; ++z)
            {
                for (int y = _root.y; y < _root.y + _size.y; ++y)
                {
                    int x =_root.x;
                    while(true) {
                        SlicedData slice = new SlicedData();
                        int junk, stripPos;
                        CompData data = GetData(new IVector3(x,y,z), out junk, out stripPos);
                        
                        slice.BeginCoord = new IVector3(x, y, z);
                        slice.Data = data.Data;
                        
                        int remDataCount = data.count - stripPos;
                        int remStripCount = _root.x + _size.x - x;
                        if (remDataCount > remStripCount)
                        {
                            slice.EndCoord = new IVector3(_root.x + _size.x - 1, y, z);
                            SliceList.Add(slice);
                            break;
                        }
                        else
                        {
                            slice.EndCoord = new IVector3(x + remDataCount - 1, y, z);
                            SliceList.Add(slice);
                            x += remDataCount;
                            if (x >= _root.x + _size.x) break;
                        }


                    }
                }
            }
            return SliceList;
        }

        public List<SlicedData> GetAllSlices()
        {
            List<SlicedData> SliceList = new List<SlicedData>();

            IVector3 curCoord = new IVector3(0, 0, 0);

            while (curCoord.z < Dims.z)
            {
                SlicedData slice = GetSliceAt(curCoord);
                SliceList.Add(slice);

                curCoord = new IVector3(slice.EndCoord.x + 1, slice.EndCoord.y, slice.EndCoord.z);

                if (curCoord.x >= Dims.x)
                {
                    curCoord.x = 0;
                    curCoord.y++;
                }

                if (curCoord.y >= Dims.y)
                {
                    curCoord.y = 0;
                    curCoord.z++;
                }
            }
            return SliceList;
        }

        public CDataArray3D GetChunk(IVector3 _root, IVector3 _size)
        {
            CDataArray3D chunk = new CDataArray3D(_size);
            
            for(int x = 0; x < _size.x; ++x)
                for(int y = 0; y < _size.y; ++y)
                    for (int z = 0; z < _size.z; ++z)
                    {
                        chunk[x, y, z] = this[_root.x + x, _root.y + y, _root.z + z];
                    }

            return chunk;

        }


        /// <summary>
        /// Debug tool.  Returns the entire DataBlock array in the form of a string -->  (Data Strip Count) x (Data), ....
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string dataString = "";
            for (int i = 0; i < DataBlock.Count; ++i)
            {
                dataString += DataBlock[i].count + "x" + DataBlock[i].Data + ", ";
            }
            return dataString;

        }

        /// <summary>
        /// Updates the data at _coord to _data
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="_coord"></param>
        /// <returns></returns>
        public bool Update(ushort _data, IVector3 _coord)
        {

            if (!CoordInRange(_coord))
            {
                throw new System.IndexOutOfRangeException(this.GetType().Name + "-- Index out of range: " + _coord.ToString());
            }
            //Get data at position
            int dataIndex, dataStripPos;
            //CompData[] workBlocks = GetDataGroup(_coord, out dataIndex, out dataStripPos);
            CompData workBlock = GetData(_coord, out dataIndex, out dataStripPos);

            //Check for bad index
            if (workBlock.count == 0)
            {
                throw new System.Exception(this.GetType().Name + "-- Update: Found zero count DataStrip at index " + dataIndex + ".");
            }

            //Check no change condition
            if (workBlock.Data == _data) return true;

            //Check single item strip condition
            if (workBlock.count == 1)
            {
                CompData updatedBlock = new CompData(_data, 1);
                DataBlock[dataIndex] = updatedBlock;
                Meld(dataIndex, MeldDir.Both);
                return true;
            }

            //Check middle of strip condition
            if ((workBlock.count > 2) && (dataStripPos != 0) && (dataStripPos != workBlock.count - 1))
            {
                //Current data strip shrink to data in front
                CompData frontStrip = new CompData(workBlock.Data, dataStripPos);

                //Insert this block
                CompData newStrip = new CompData(_data, 1);

                //Create data in back
                CompData backStrip = new CompData(workBlock.Data, workBlock.count - dataStripPos - 1);

                //Update Data Array
                DataBlock.Insert(dataIndex + 1, backStrip);
                DataBlock.Insert(dataIndex + 1, newStrip);
                DataBlock[dataIndex] = frontStrip;
                //Meld(dataIndex);

                return true;
            }

            //Check beginning of strip condition
            if (dataStripPos == 0)
            {
                CompData newStrip = new CompData(_data, 1);

                CompData backStrip = new CompData(workBlock.Data, workBlock.count - 1);

                DataBlock.Insert(dataIndex + 1, backStrip);
                DataBlock[dataIndex] = newStrip;
                Meld(dataIndex, MeldDir.Front);

                return true;

            }

            //Check end of strip condition
            if (dataStripPos == DataBlock[dataIndex].count - 1)
            {
                CompData newStrip = new CompData(_data, 1);

                CompData frontStrip = new CompData(workBlock.Data, workBlock.count - 1);

                DataBlock.Insert(dataIndex, frontStrip);
                DataBlock[dataIndex] = newStrip;
                Meld(dataIndex, MeldDir.Back);

                return true;

            }

            return false;
        }



        //---------------------------------------------------------------------UNUSED
        /// <summary>
        /// Retrieves data at coord and the data before and after the coord inline.  Also retrieves index of data position in array.
        /// </summary>
        /// <param name="_coord"></param>
        /// <param name="_position"></param>
        /// <returns></returns>
        /*private CompData[] GetDataGroup(IVector3 _coord, out int _index, out int _stripPos)
        {
            //Check that coords are valid
            if (CoordInRange(_coord))
            {
                int curBlkLinear = 0;
                int trgBlkLinear = _coord.x + _coord.y * Dims.x + _coord.z * Dims.y * Dims.x;
                int maxBlk = Dims.x * Dims.y * Dims.z;
            
            
                //Create empty return block data
                CompData[] threeBlocks = new CompData[3] { new CompData(0, 0), new CompData(0, 0), new CompData(0, 0) };
            
                _stripPos = -1;
                _index = 0;
                if (trgBlkLinear != 0 && trgBlkLinear != maxBlk)
                {
                    while (true)
                    {

                        curBlkLinear += DataBlock[_index].count;
                        if (curBlkLinear > trgBlkLinear)
                        {
                            //Step back
                            curBlkLinear -= DataBlock[_index].count;

                            //Get position in block
                            _stripPos = trgBlkLinear - curBlkLinear;
                            break;
                        }
                        _index++;
                    }

                }
                else
                {
                    if (trgBlkLinear == maxBlk) _index = DataBlock.Count - 1;
                }


            
                //Check if beginning of block and/or end of block
                if (_stripPos == 0)
                {
                    if (trgBlkLinear != 0) //Not 0,0,0
                    {
                        threeBlocks[0] = new CompData(DataBlock[_index - 1].Data, DataBlock[_index - 1].count);
                    }
                }
                else
                {
                    threeBlocks[0] = new CompData(DataBlock[_index].Data, DataBlock[_index].count);
                }
                if (_stripPos == DataBlock[_index].count - 1)
                {
                    threeBlocks[2] = new CompData(DataBlock[_index + 1].Data, DataBlock[_index + 1].count);
                }
                else
                {
                    threeBlocks[2] = new CompData(DataBlock[_index].Data, DataBlock[_index].count);
                }

                threeBlocks[1] = new CompData(DataBlock[_index].Data, DataBlock[_index].count);

                return threeBlocks;
            }
            else
            {
                throw new System.ArgumentException("Coords (" + _coord.x + ", " + _coord.y + ", " + _coord.z + ") out of range.  Dims (" + Dims.x + ", " + Dims.y + ", " + Dims.z + ")", "x, y, z");

            }
        }*/


        /// <summary>
        /// Resize keeping integrity of the data adding _fillType for expansion or removing only by dimension.
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        /*public void Resize(int _x, int _y, int _z, byte _fillType = 0)
        {
            //Determine new size and record new dimensions
            int size = _x * _y * _z;
            IVector3 newDims = new IVector3(_x, _y, _z);

            //X case
            if (_x > Dims.x) //x dim has grown
            {
                for (int y = 0; y < Dims.y; ++y)
                {
                    for (int z = 0; z < Dims.z; ++z)
                    {
                        //Get strip and type
                    
                    }
                }
            



            }
            else //x dim has shrunk
            {

            }
        }*/


    }

}